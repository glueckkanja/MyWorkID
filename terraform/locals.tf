# Variable copies
locals {
  tenant_id                           = var.tenant_id
  subscription_id                     = var.subscription_id
  resource_group_name                 = var.resource_group_name
  resource_location                   = var.resource_location
  api_name                            = var.api_name
  backed_appreg_name                  = var.backed_appreg_name
  frontend_appreg_name                = var.frontend_appreg_name
  dismiss_user_risk_auth_context_id   = var.dismiss_user_risk_auth_context_id
  generate_tap_auth_context_id        = var.generate_tap_auth_context_id
  reset_password_auth_context_id      = var.reset_password_auth_context_id
  skip_actions_requiring_global_admin = var.skip_actions_requiring_global_admin
  binaries_zip_path                   = var.binaries_zip_path
}

# Permissions necessary for the banend managed identity
locals {
  backend_graph_permissions = ["IdentityRiskyUser.ReadWrite.All", "UserAuthenticationMethod.ReadWrite.All", "User.ReadWrite.All"]
}

# settings that change if in dev
locals {
  frontend_dev_redirect_uris = var.is_dev ? ["https://localhost:5173/"] : []
}
