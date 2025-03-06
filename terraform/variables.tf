# General
variable "tenant_id" {
  type = string
}
variable "subscription_id" {
  type = string
}
variable "resource_group_name" {
  type    = string
  default = "rg-MyWorkID"
}
variable "resource_location" {
  type    = string
  default = "westeurope"
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
variable "enable_auto_update" {
  type = bool
  description = "Decides wether the backend should be updated automatically. If set to false the backend will not be updated automatically. If set to true the backend will be updated automatically. NOTE: If this ever was set to false a change to true will result in the backend being recreated automatically"
  default = true
}
variable "allow_credential_operations_for_privileged_users"{
  type = bool
  description = "Allow credential operations for privileged users. If set to true, users with privileged roles (e.g. Global Admin or User Admin) can perform credential operations like create TAP and reset password"
  default = false
}

# AppService
variable "api_name" {
  type        = string
  description = "Name of the AppService that hosts the api. Note this has to be globally unique."
}
variable "custom_domains" {
  type    = list(string)
  default = []
  description = "OPTIONAL: List of custom domains for MyWorkID. Must be configured at a later time. NOTE: If specified the VerifiedId callbacks will always use the first domain in the list."
}

# AppRegistrations
variable "backed_appreg_name" {
  type    = string
  default = "ar-MyWorkID-backend"
  description = "Name of the AppRegistration that is used by the backend"
}
variable "frontend_appreg_name" {
  type    = string
  default = "ar-MyWorkID-frontend"
  description = "Name of the AppRegistration that is used by the frontend"
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
  default     = "MyWorkID"
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
  description = "Values for the backend access group names. Only relevant if skip_creation_backend_access_groups = false"
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
