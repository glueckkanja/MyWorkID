# General
variable "tenant_id" {
  type = string
}
variable "subscription_id" {
  type = string
}
variable "resource_group_name" {
  type    = string
  default = "rg-myWorkID"
}
variable "resource_location" {
  type    = string
  default = "westeurope"
}
variable "binaries_zip_path" {
  type        = string
  description = "Path where binaries are located"
}

# Flags
variable "skip_actions_requiring_global_admin" {
  description = "Skip actions that require global admin permissions. If set to true you will have to set some settings, like the permission grants, manually. NOTE: If this ever was set to false a change to true will result in the previously set permissions being removed"
  type        = bool
  default     = false
}
variable "skip_creation_backend_access_groups" {
  type        = bool
  description = "Value to determine if the backend access groups should be created automatically or if this action should be skipped"
  default     = false
}

# AppService
variable "api_name" {
  type        = string
  description = "Name of the AppService that hosts the api. Note this has to be globally unique."
}
variable "custom_redirect_url" {
  type    = set(string)
  default = []
  description = "List of custom domains for MyWorkId. Must be configured at a later time"
}

# AppRegistrations
variable "backed_appreg_name" {
  type    = string
  default = "ar-myWorkID-backend"
}
variable "frontend_appreg_name" {
  type    = string
  default = "ar-myWorkID-frontend"
}

# AuthContexts
variable "dismiss_user_risk_auth_context_id" {
  type        = string
  description = "AuthContext Id configured that is challenged for the dismissUser action"
}
variable "generate_tap_auth_context_id" {
  type        = string
  description = "AuthContext Id configured that is challenged for the generateTAP action"
}
variable "reset_password_auth_context_id" {
  type        = string
  description = "AuthContext Id configured that is challenged for the resetPassword action"
}



# VerifiedId
variable "verified_id_jwt_signing_key_secret_name" {
  type        = string
  description = "KeyVault secret name for the signing key of the jwt used int the verifiedId callbacks"
  default     = "VerifiedId-JwtSigningKey"
}
variable "verified_id_decentralized_identifier_secret_name" {
  type        = string
  description = "KeyVault secret name for the Decentralized identifier of the tenant (https://learn.microsoft.com/en-us/entra/verified-id/verifiable-credentials-configure-verifier#gather-tenant-details-to-set-up-your-sample-application)"
  default     = "VerifiedId-DecentralizedIdentifier"
}
variable "verified_id_verify_security_attribute_set" {
  type        = string
  description = "The name of the custom security attribute set where the last verified date should be stored."
  default     = "myWorkID"
}
variable "verified_id_verify_security_attribute" {
  type        = string
  description = "The name of the custom security attribute where the last verified date should be stored."
  default     = "lastVerifiedFaceCheck"
}

# Backend Access Groups
variable "backend_access_group_names" {
  type = object({
    create_tap        = optional(string, "sec - MyWorkID - Create TAP")
    dismiss_user_risk = optional(string, "sec - MyWorkID - Dismiss User Risk")
    password_reset    = optional(string, "sec - MyWorkID - Password Reset")
    validate_identity = optional(string, "sec - MyWorkID - Validate Identity")
  })
  default = {}
}

# Dev
variable "is_dev" {
  type    = bool
  default = false
}
variable "dev_redirect_url" {
  type    = set(string)
  default = []
}