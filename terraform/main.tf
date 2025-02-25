resource "azurerm_resource_group" "main" {
  name     = local.resource_group_name
  location = local.resource_location
  lifecycle {
    ignore_changes = [
      tags
    ]
  }
}

# Create app service plan for backend API
resource "azurerm_service_plan" "backend" {
  name                = "asp-${local.api_name}"
  location            = azurerm_resource_group.main.location
  resource_group_name = azurerm_resource_group.main.name
  os_type             = "Linux"
  sku_name            = "B1"
  lifecycle {
    ignore_changes = [
      tags
    ]
  }
}

resource "azurerm_log_analytics_workspace" "backend_application_insights" {
  name                = "wsp-${local.api_name}"
  location            = azurerm_resource_group.main.location
  resource_group_name = azurerm_resource_group.main.name
  sku                 = "PerGB2018"
  retention_in_days   = 30
  lifecycle {
    ignore_changes = [
      tags
    ]
  }
}

resource "azurerm_application_insights" "backend" {
  name                = "ai-${local.api_name}"
  location            = azurerm_resource_group.main.location
  resource_group_name = azurerm_resource_group.main.name
  workspace_id        = azurerm_log_analytics_workspace.backend_application_insights.id
  application_type    = "web"
  lifecycle {
    ignore_changes = [
      tags
    ]
  }
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
    always_on           = true
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
    VerifiedId__JwtSigningKey                  = "@Microsoft.KeyVault(VaultName=${azurerm_key_vault.backend_secrets.name};SecretName=${local.verified_id_jwt_signing_key_secret_name})"
    VerifiedId__DecentralizedIdentifier        = "@Microsoft.KeyVault(VaultName=${azurerm_key_vault.backend_secrets.name};SecretName=${local.verified_id_decentralized_identifier_secret_name})"
    VerifiedId__TargetSecurityAttributeSet     = local.verified_id_verify_security_attribute_set
    VerifiedId__TargetSecurityAttribute        = local.verified_id_verify_security_attribute
    VerifiedId__BackendUrl                     = local.is_custom_domain_configured ? "https://${local.custom_domains[0]}" : "https://${local.api_name}.azurewebsites.net"
    VerifiedId__CreatePresentationRequestUri   = local.verified_id_create_presentation_request_uri
  }

  lifecycle {
    ignore_changes = [
      tags
    ]
  }
}

resource "azuread_app_role_assignment" "backend_managed_identity" {
  for_each            = local.skip_actions_requiring_global_admin ? toset([]) : toset(local.backend_graph_permissions)
  app_role_id         = data.azuread_service_principal.msgraph.app_role_ids[each.key]
  principal_object_id = azurerm_linux_web_app.backend.identity[0].principal_id
  resource_object_id  = data.azuread_service_principal.msgraph.object_id
}

# Necessary for the backend to be able to create taps and change the password of users - The password cannot be changed for users with some privilaged permissions - if it is desired to change the password of users with these permissions, the backend must have higher roles - for more info see https://learn.microsoft.com/en-us/entra/identity/role-based-access-control/privileged-roles-permissions?tabs=admin-center#who-can-reset-passwords and https://learn.microsoft.com/en-us/entra/identity/authentication/howto-authentication-temporary-access-pass#create-a-temporary-access-pass
resource "azuread_directory_role" "authentication_administrator" {
  count        = local.skip_actions_requiring_global_admin ? 0 : 1
  display_name = "Authentication Administrator"
  # display_name = "Privileged Authentication Administrator" #Necessary if privilaged users should also be able to use all functions (createTAP & changePassword) via myWorkID
}

resource "azuread_directory_role_assignment" "backend_managed_identity_authentication_administrator" {
  count               = local.skip_actions_requiring_global_admin ? 0 : 1
  role_id             = azuread_directory_role.authentication_administrator[0].template_id
  principal_object_id = azurerm_linux_web_app.backend.identity[0].principal_id
}

resource "azuread_service_principal" "verifiable_credentials_service_request" {
  count = local.skip_actions_requiring_global_admin ? 0 : 1
  client_id    = "3db474b9-6a0c-4840-96ac-1fceb342124f"
  use_existing = true
}

resource "azuread_app_role_assignment" "verifiable_credentials" {
  for_each            = local.skip_actions_requiring_global_admin ? toset([]) : toset(["VerifiableCredential.Create.All"])
  app_role_id         = "949ebb93-18f8-41b4-b677-c2bfea940027" // VerifiableCredential.Create.All
  principal_object_id = azurerm_linux_web_app.backend.identity[0].principal_id
  resource_object_id  = azuread_service_principal.verifiable_credentials_service_request[0].object_id
}

resource "azuread_service_principal_delegated_permission_grant" "frontend_backend_access" {
  count                                = local.skip_actions_requiring_global_admin ? 0 : 1
  service_principal_object_id          = azuread_service_principal.frontend.object_id
  resource_service_principal_object_id = azuread_service_principal.backend.object_id
  claim_values                         = ["openid", "Access"]
}

# Create Backed AppReg
resource "azuread_application" "backend" {
  display_name     = local.backed_appreg_name
  owners           = [data.azuread_client_config.current_user.object_id]
  sign_in_audience = "AzureADMyOrg"

  lifecycle {
    ignore_changes = [
      identifier_uris, #Necessary due to azuread_application_identifier_uri.backend
      tags
    ]
  }

  app_role {
    allowed_member_types = ["User"]
    description          = "Allows user to Create a temporary access token"
    display_name         = local.create_tap_app_role_name
    enabled              = true
    id                   = "16f5de80-8ee7-46e3-8bfe-7de7af6164ed"
    value                = local.create_tap_app_role_name
  }
  app_role {
    allowed_member_types = ["User"]
    description          = "Allows user to Dismiss its User Risk"
    display_name         = local.dismiss_user_risk_app_role_name
    enabled              = true
    id                   = "9262ab98-6c08-4e32-bae3-4c12d4ce2463"
    value                = local.dismiss_user_risk_app_role_name
  }
  app_role {
    allowed_member_types = ["User"]
    description          = "Allows user to Reset its password"
    display_name         = local.password_reset_app_role_name
    enabled              = true
    id                   = "13c4693c-84f1-43b4-85a2-5e51d41753ed"
    value                = local.password_reset_app_role_name
  }
  app_role {
    allowed_member_types = ["User"]
    description          = "Allows user to Validate its Identity by VerifiedId"
    display_name         = local.validate_identity_app_role_name
    enabled              = true
    id                   = "eeacf7de-5c05-4e21-a2be-a4d8e3435237"
    value                = local.validate_identity_app_role_name
  }

  api {
    oauth2_permission_scope {
      admin_consent_description  = "Access To MyWorkID backend"
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

  redirect_uris = setunion(
    formatlist("https://%s/", local.custom_domains),
    ["https://${azurerm_linux_web_app.backend.default_hostname}/"],
    local.frontend_dev_redirect_uris,
  )
}
# Frontend AppReg End

resource "azuread_service_principal" "frontend" {
  client_id = azuread_application_registration.frontend.client_id
  owners    = [data.azuread_client_config.current_user.object_id]
}
resource "azuread_service_principal" "msgraph" {
  client_id    = data.azuread_application_published_app_ids.well_known.result.MicrosoftGraph
  use_existing = true
}
resource "azuread_service_principal_delegated_permission_grant" "frontend" {
  count                                = local.skip_actions_requiring_global_admin ? 0 : 1
  service_principal_object_id          = azuread_service_principal.frontend.object_id
  resource_service_principal_object_id = azuread_service_principal.msgraph.object_id
  claim_values                         = ["User.Read"]
}

# Key vault
resource "azurerm_key_vault" "backend_secrets" {
  name                        = substr("kv-${local.api_name}", 0, 24)
  location                    = azurerm_resource_group.main.location
  resource_group_name         = azurerm_resource_group.main.name
  enabled_for_disk_encryption = true
  tenant_id                   = local.tenant_id
  soft_delete_retention_days  = 7
  purge_protection_enabled    = false
  enable_rbac_authorization   = true
  sku_name                    = "standard"
  lifecycle {
    ignore_changes = [
      tags
    ]
  }
}

resource "azurerm_role_assignment" "backend_key_vault_access" {
  depends_on           = [azurerm_linux_web_app.backend]
  scope                = azurerm_key_vault.backend_secrets.id
  role_definition_name = "Key Vault Secrets User"
  principal_id         = azurerm_linux_web_app.backend.identity[0].principal_id
}

resource "azuread_group" "backend_access" {
  for_each = local.base_access_groups_map

  display_name     = each.value.group_name
  description      = "Access group for MyWorkID backend permission ${each.value.app_role}"
  security_enabled = true
}

resource "azuread_app_role_assignment" "backend_access" {
  for_each = local.base_access_groups_map

  app_role_id         = azuread_service_principal.backend.app_role_ids[each.value.app_role]
  principal_object_id = azuread_group.backend_access[each.key].object_id
  resource_object_id  = azuread_service_principal.backend.object_id
}
