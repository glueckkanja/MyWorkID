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

variable "frontend_name" {
  type = string
  description = "Name of the StaticSite that hosts the frontend. Note this has to be globally unique."
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