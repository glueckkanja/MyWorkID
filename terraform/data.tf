data "azuread_application_published_app_ids" "well_known" {}

data "azuread_service_principal" "msgraph" {
  client_id = data.azuread_application_published_app_ids.well_known.result["MicrosoftGraph"]
}

data "azuread_client_config" "current_user" {}

data "azuread_service_principal" "verifiable_credentials_service_request" {
  client_id = "3db474b9-6a0c-4840-96ac-1fceb342124f"
}