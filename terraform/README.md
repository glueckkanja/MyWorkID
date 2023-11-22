# Demotenant (ApertureLaboratories) dev setup
## Necessary Permissions
You will need the `Storage Table Data Contributor` or higher to access on the `samyaccountvnexttf` storage account. Please reachout to @MichaMican to get the necessary permissions

## Files to create
To deploy into the Demotenant please create the following files in this folder (they are gitignored)
### backend.tf
```
# This backend config is for the Aperture Laboratories Demo tenant
terraform {
  backend "azurerm" {
    resource_group_name  = "rg-myAccountVNext"
    subscription_id      = "689d3b77-2c70-4441-aef7-0fc40855c83b"
    storage_account_name = "samyaccountvnexttf"
    container_name       = "tfstate"
    key                  = "prod.terraform.tfstate"
    tenant_id            = "a9ae459a-6068-4a03-915a-7031507edbc1"
    use_azuread_auth     = true
  }
}
```

### demotenant_config.auto.tfvars
```
tenant_id                         = "a9ae459a-6068-4a03-915a-7031507edbc1"
subscription_id                   = "689d3b77-2c70-4441-aef7-0fc40855c83b"
resource_group_name               = "rg-myAccountVNext"
resource_location                 = "westeurope"
api_name                          = "api-my-account-vnext-demo"
frontend_name                     = "my-account-vnext-demo"
dismiss_user_risk_auth_context_id = "c1"
generate_tap_auth_context_id      = "c1"
reset_password_auth_context_id    = "c1"
is_dev                            = true
```