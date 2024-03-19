solution = {
  name     = "HomeLink"
  location = "eastus"
  keyvault = {
    sku                        = "standard"
    soft_delete_retention_days = 7
    purge_protection_enabled   = true
  }
  appconfig = {
    namespace = "app-config-system"
  }
}