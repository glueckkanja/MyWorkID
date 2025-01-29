# This backend config is an example. Modify it to match you environment
terraform {
  backend "azurerm" {
    tenant_id            = "YOUR_TENANTID"
    subscription_id      = "YOUR_SUBSCRIPTIONID"
    resource_group_name  = "rg-myworkid-terraform"
    storage_account_name = "YOUR_STORAGEACCOUNTNAME"
    container_name       = "YOUR_STORAGECONTAINERNAME"
    key                  = "prod.terraform.tfstate"
    use_azuread_auth     = true
  }
}