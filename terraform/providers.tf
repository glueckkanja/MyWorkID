terraform {
  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "3.76.0"
    }
    azuread = {
      source = "hashicorp/azuread"
      version = "2.44.1"
    }
  }
}

provider "azurerm" {
  skip_provider_registration = true # Might be necessary 
  tenant_id       = local.tenant_id
  subscription_id = local.subscription_id
  features {}
}

provider "azuread" {
  # Configuration options
}