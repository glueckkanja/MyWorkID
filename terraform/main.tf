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

resource "azurerm_linux_web_app" "backend" {
  name                    = local.api_name
  location                = azurerm_resource_group.main.location
  resource_group_name     = azurerm_resource_group.main.name
  service_plan_id         = azurerm_service_plan.backend.id
  https_only              = true
  client_affinity_enabled = false

  identity {
    type = "SystemAssigned"
  }

  site_config {
    minimum_tls_version = "1.2"
    always_on           = false
  }
}

resource "azuread_app_role_assignment" "backend_managed_identity" {
  for_each            = toset(local.backend_graph_permissions)
  app_role_id         = data.azuread_service_principal.msgraph.app_role_ids[each.key]
  principal_object_id = azurerm_linux_web_app.backend.identity[0].principal_id
  resource_object_id  = data.azuread_service_principal.msgraph.object_id
}

resource "azurerm_static_site" "frontend" {
  name                = local.frontend_name
  resource_group_name = azurerm_resource_group.main.name
  location            = azurerm_resource_group.main.location
}

# Create Backed AppReg
resource "azuread_application" "backend" {
  display_name     = local.backed_appreg_name
  owners           = [data.azuread_client_config.current_user.object_id]
  sign_in_audience = "AzureADMyOrg"

  // Necessary as identifier_uris is set by azuread_application_identifier_uri and this would overwrite it
  lifecycle {
    ignore_changes = [
      identifier_uris,
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

# Create Frontend AppReg
resource "azuread_application" "frontend" {
  display_name     = local.frontend_appreg_name
  owners           = [data.azuread_client_config.current_user.object_id]
  sign_in_audience = "AzureADMyOrg"

  api {
    requested_access_token_version = 2
  }

  web {
    redirect_uris = ["https://${azurerm_static_site.frontend.default_host_name}/"]
  }

  required_resource_access {
    resource_app_id = azuread_application.backend.client_id

    resource_access {
      id   = azuread_application.backend.oauth2_permission_scope_ids.Access
      type = "Scope"
    }
  }

}
