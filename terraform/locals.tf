# Variable copies
locals {
  tenant_id           = var.tenant_id
  subscription_id     = var.subscription_id
  resource_group_name = var.resource_group_name
  resource_location   = var.resource_location
  api_name            = var.api_name
  frontend_name       = var.frontend_name
}

# Permissions necessary for the banend managed identity
locals {
  backend_graph_permissions = ["IdentityRiskyUser.ReadWrite.All", "UserAuthenticationMethod.ReadWrite.All"]
}