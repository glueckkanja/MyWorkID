variable "tenant_id" {
  type = string
}

variable "subscription_id" {
  type = string
}

variable "resource_group_name" {
  type = string
  default = "rg-myAccountVNext"
}

variable "resource_location" {
  type = string
  default = "westeurope"
}

variable "api_name" {
  type = string
  description = "Name of the AppService that hosts the api. Note this has to be globally unique."
}

variable "backed_appreg_name" {
  type = string
  default = "ar-myAccountVNext-backend"
}

variable "frontend_appreg_name" {
  type = string
  default = "ar-myAccountVNext-frontend"
}

variable "dismiss_user_risk_auth_context_id"{
  type = string
  description = "AuthContext Id configured that is challenged for the dismissUser action"
}
variable "generate_tap_auth_context_id"{
  type = string
  description = "AuthContext Id configured that is challenged for the generateTAP action"
}
variable "reset_password_auth_context_id"{
  type = string
  description = "AuthContext Id configured that is challenged for the resetPassword action"
}
variable "is_dev" {
  type = bool
  default = false
}
variable "skip_actions_requiring_global_admin" {
  description = "Skip actions that require global admin permissions. If set to true you will have to set some settings, like the permission grants, manually. NOTE: If this ever was set to false a change to true will result in the previously set permissions being removed"
  type = bool
  default = false
}
variable "binaries_zip_path" {
  type = string
  description = "Path where binaries are located"
}