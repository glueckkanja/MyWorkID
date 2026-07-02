terraform {
  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "4.79.0"
    }
    azuread = {
      source  = "hashicorp/azuread"
      version = "3.9.0"
    }
    time = {
      source  = "hashicorp/time"
      version = "0.14.0"
    }
  }
}

provider "azurerm" {
  resource_provider_registrations = "none"
  tenant_id                       = local.tenant_id
  subscription_id                 = local.subscription_id
  features {}
}

provider "azuread" {
  # Configuration options
}
