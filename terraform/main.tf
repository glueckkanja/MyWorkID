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

# Add necessary permissions to backend Managed Identity
resource "azuread_application_api_access" "example_msgraph" {
  application_id = format("%s/%s","applications",azurerm_linux_web_app.backend.identity[0].principal_id)
  api_client_id  = data.azuread_application_published_app_ids.well_known.result["MicrosoftGraph"]

  role_ids = [
    data.azuread_service_principal.msgraph.app_role_ids["IdentityRiskyUser.ReadWrite.All"],
    data.azuread_service_principal.msgraph.app_role_ids["UserAuthenticationMethod.ReadWrite.All"],
  ]
}

resource "azurerm_static_site" "frontend" {
  name                = local.frontend_name
  resource_group_name = azurerm_resource_group.main.name
  location            = azurerm_resource_group.main.location
}
