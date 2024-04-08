resource "azurerm_resource_group" "main" {
  name     = local.resource_group_name
  location = local.resource_location
}

# Create app service plan for backend API
resource "azurerm_service_plan" "backend" {
  name                = "asp-${local.api_name}"
  location            = azurerm_resource_group.main.location
  resource_group_name = azurerm_resource_group.main.name
  os_type             = "Linux"
  sku_name            = "B1"
}

resource "azurerm_log_analytics_workspace" "backend_application_insights" {
  name                = "wsp-${local.api_name}"
  location            = azurerm_resource_group.main.location
  resource_group_name = azurerm_resource_group.main.name
  sku                 = "PerGB2018"
  retention_in_days   = 30
}

resource "azurerm_application_insights" "backend" {
  name                = "ai-${local.api_name}"
  location            = azurerm_resource_group.main.location
  resource_group_name = azurerm_resource_group.main.name
  workspace_id        = azurerm_log_analytics_workspace.backend_application_insights.id
  application_type    = "web"
}

resource "azurerm_linux_web_app" "backend" {
  name                    = local.api_name
  location                = azurerm_resource_group.main.location
  resource_group_name     = azurerm_resource_group.main.name
  service_plan_id         = azurerm_service_plan.backend.id
  https_only              = true
  client_affinity_enabled = false
  zip_deploy_file         = local.binaries_zip_path

  identity {
    type = "SystemAssigned"
  }

  site_config {
    application_stack {
      dotnet_version = "8.0"
    }
    minimum_tls_version = "1.2"
    always_on           = false
  }

  app_settings = {
    AppFunctions__DismissUserRisk              = local.dismiss_user_risk_auth_context_id
    AppFunctions__GenerateTap                  = local.generate_tap_auth_context_id
    AppFunctions__ResetPassword                = local.reset_password_auth_context_id
    AzureAd__ClientId                          = azuread_application.backend.client_id
    AzureAd__TenantId                          = data.azuread_client_config.current_user.tenant_id
    AzureAd__Instance                          = "https://login.microsoftonline.com/"
    Frontend__FrontendClientId                 = azuread_application_registration.frontend.client_id
    Frontend__BackendClientId                  = azuread_application.backend.client_id
    Frontend__TenantId                         = data.azuread_client_config.current_user.tenant_id
    WEBSITE_RUN_FROM_PACKAGE                   = "1"
    APPLICATIONINSIGHTS_CONNECTION_STRING      = azurerm_application_insights.backend.connection_string
    ApplicationInsightsAgent_EXTENSION_VERSION = "~3"          #https://learn.microsoft.com/en-us/azure/azure-monitor/app/azure-web-apps-net-core?tabs=Windows%2Cwindows#application-settings-definitions
    XDT_MicrosoftApplicationInsights_Mode      = "recommended" #https://learn.microsoft.com/en-us/azure/azure-monitor/app/azure-web-apps-net-core?tabs=Windows%2Cwindows#application-settings-definitions
  }


}

resource "azuread_app_role_assignment" "backend_managed_identity" {
  for_each            = local.skip_actions_requiring_global_admin ? toset([]) : toset(local.backend_graph_permissions)
  app_role_id         = data.azuread_service_principal.msgraph.app_role_ids[each.key]
  principal_object_id = azurerm_linux_web_app.backend.identity[0].principal_id
  resource_object_id  = data.azuread_service_principal.msgraph.object_id
}

resource "azuread_directory_role" "user_administrator" {
  display_name = "User Administrator"
}

resource "azuread_directory_role_assignment" "backend_managed_identity_user_admin" {
  role_id = azuread_directory_role.user_administrator.template_id
  principal_object_id       = azurerm_linux_web_app.backend.identity[0].principal_id
}

# Create Backed AppReg
resource "azuread_application" "backend" {
  display_name     = local.backed_appreg_name
  owners           = [data.azuread_client_config.current_user.object_id]
  sign_in_audience = "AzureADMyOrg"

  lifecycle {
    ignore_changes = [
      identifier_uris, #Necessary due to azuread_application_identifier_uri.backend
    ]
  }

  app_role {
    allowed_member_types = ["User"]
    description          = "Allows user to Dismiss its User Risk"
    display_name         = "MyAccount.VNext.DismissUserRisk"
    enabled              = true
    id                   = "9262ab98-6c08-4e32-bae3-4c12d4ce2463"
    value                = "MyAccount.VNext.DismissUserRisk"
  }
  app_role {
    allowed_member_types = ["User"]
    description          = "Allows user to Create a temporary access token"
    display_name         = "MyAccount.VNext.CreateTAP"
    enabled              = true
    id                   = "16f5de80-8ee7-46e3-8bfe-7de7af6164ed"
    value                = "MyAccount.VNext.CreateTAP"
  }
  app_role {
    allowed_member_types = ["User"]
    description          = "Allows user to Reset its password"
    display_name         = "MyAccount.VNext.PasswordReset"
    enabled              = true
    id                   = "13c4693c-84f1-43b4-85a2-5e51d41753ed"
    value                = "MyAccount.VNext.PasswordReset"
  }

  api {
    oauth2_permission_scope {
      admin_consent_description  = "Access To MyAccount.VNext backend"
      admin_consent_display_name = "Access"
      enabled                    = true
      id                         = "7e119516-7dd5-4cc0-a906-5f1a9cfd5801"
      type                       = "Admin"
      value                      = "Access"
    }
  }

}
resource "azuread_application_identifier_uri" "backend" {
  application_id = azuread_application.backend.id
  identifier_uri = "api://${azuread_application.backend.client_id}"
}

resource "azuread_service_principal" "backend" {
  client_id = azuread_application.backend.client_id
  owners    = [data.azuread_client_config.current_user.object_id]
}

# Frontend AppReg Start
# azuread_application_registration is used due to azuread_application_redirect_uris beeing incompatible with azuread_application - azuread_application_redirect_uris.frontend_backend necessary as it creates a circle with azurerm_linux_web_app.backend
resource "azuread_application_registration" "frontend" {
  display_name                   = local.frontend_appreg_name
  sign_in_audience               = "AzureADMyOrg"
  requested_access_token_version = 2
}

resource "azuread_application_api_access" "frontend_backend" {
  application_id = azuread_application_registration.frontend.id
  api_client_id  = azuread_application.backend.client_id
  scope_ids = [
    azuread_application.backend.oauth2_permission_scope_ids.Access,
  ]
}

resource "azuread_application_api_access" "frontend_graph" {
  application_id = azuread_application_registration.frontend.id
  api_client_id  = data.azuread_application_published_app_ids.well_known.result["MicrosoftGraph"]
  scope_ids = [
    data.azuread_service_principal.msgraph.oauth2_permission_scope_ids["User.Read"],
  ]
}

resource "azuread_application_owner" "frontend_current_user" {
  application_id  = azuread_application_registration.frontend.id
  owner_object_id = data.azuread_client_config.current_user.object_id
}

resource "azuread_application_redirect_uris" "frontend_backend" {
  application_id = azuread_application_registration.frontend.id
  type           = "SPA"

  redirect_uris = concat(
    ["https://${azurerm_linux_web_app.backend.default_hostname}/"],
    local.frontend_dev_redirect_uris,
  )
}
# Frontend AppReg End

resource "azuread_service_principal" "frontend" {
  client_id = azuread_application_registration.frontend.client_id
  owners    = [data.azuread_client_config.current_user.object_id]
}
