# Variable copies
locals {
  tenant_id                                        = var.tenant_id
  subscription_id                                  = var.subscription_id
  resource_group_name                              = var.resource_group_name
  resource_location                                = var.resource_location
  api_name                                         = var.api_name
  backed_appreg_name                               = var.backed_appreg_name
  frontend_appreg_name                             = var.frontend_appreg_name
  dismiss_user_risk_auth_context_id                = var.dismiss_user_risk_auth_context_id
  generate_tap_auth_context_id                     = var.generate_tap_auth_context_id
  reset_password_auth_context_id                   = var.reset_password_auth_context_id
  skip_actions_requiring_global_admin              = var.skip_actions_requiring_global_admin
  verified_id_jwt_signing_key_secret_name          = var.verified_id_jwt_signing_key_secret_name
  verified_id_decentralized_identifier_secret_name = var.verified_id_decentralized_identifier_secret_name
  verified_id_verify_security_attribute_set        = var.verified_id_verify_security_attribute_set
  verified_id_verify_security_attribute            = var.verified_id_verify_security_attribute
  verified_id_face_match_confidence_threshold      = var.verified_id_face_match_confidence_threshold
  tap_lifetime_in_minutes                          = var.tap_lifetime_in_minutes
  tap_is_usable_once                               = var.tap_is_usable_once
  custom_domains                                   = var.custom_domains
  custom_css_url                                   = var.custom_css_url
  app_title                                        = var.app_title
  favicon_url                                      = var.favicon_url
  enable_auto_update                               = var.enable_auto_update
  allow_credential_operations_for_privileged_users = var.allow_credential_operations_for_privileged_users
}

# Permissions necessary for the banend managed identity
locals {
  backend_graph_permissions = ["IdentityRiskyUser.ReadWrite.All", "CustomSecAttributeAssignment.ReadWrite.All"]
}

# settings that change if in dev
locals {
  frontend_dev_redirect_uris = var.is_dev ? var.dev_redirect_url : []
}

# Static variables
locals {
  verified_id_create_presentation_request_uri = "https://verifiedid.did.msidentity.com/v1.0/verifiableCredentials/createPresentationRequest"
  create_tap_app_role_name                    = "MyWorkID.CreateTAP"
  dismiss_user_risk_app_role_name             = "MyWorkID.DismissUserRisk"
  password_reset_app_role_name                = "MyWorkID.PasswordReset"
  validate_identity_app_role_name             = "MyWorkID.ValidateIdentity"
  latest_binaries_url                         = "https://github.com/glueckkanja/MyWorkID/releases/latest/download/binaries.zip"
}

# Helper properties
locals {
  is_custom_domain_configured = length(local.custom_domains) > 0
}

locals {
  backend_access_groups_map = {
    "CreateTAP" = {
      group_name = "${var.backend_access_group_names.create_tap}"
      app_role   = "${local.create_tap_app_role_name}"
    }
    "DismissUserRisk" = {
      group_name = "${var.backend_access_group_names.dismiss_user_risk}"
      app_role   = "${local.dismiss_user_risk_app_role_name}"
    }
    "PasswordReset" = {
      group_name = "${var.backend_access_group_names.password_reset}"
      app_role   = "${local.password_reset_app_role_name}"
    }
    "ValidateIdentity" = {
      group_name = "${var.backend_access_group_names.validate_identity}"
      app_role   = "${local.validate_identity_app_role_name}"
    }
  }

  base_access_groups_map = { for k, v in local.backend_access_groups_map : k => v if !var.skip_creation_backend_access_groups && !var.skip_actions_requiring_global_admin }
}
