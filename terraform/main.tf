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
  for_each = toset(local.backend_graph_permissions)
  app_role_id         = data.azuread_service_principal.msgraph.app_role_ids[each.key]
  principal_object_id = azurerm_linux_web_app.backend.identity[0].principal_id
  resource_object_id  = data.azuread_service_principal.msgraph.object_id
}

resource "azurerm_static_site" "frontend" {
  name                = local.frontend_name
  resource_group_name = azurerm_resource_group.main.name
  location            = azurerm_resource_group.main.location
}
