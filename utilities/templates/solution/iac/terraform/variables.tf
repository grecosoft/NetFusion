variable "solution" {
  type = object({
    name     = string
    location = string
    keyvault = object({
      sku                        = string
      soft_delete_retention_days = number
      purge_protection_enabled   = bool
    })
    appconfig = object({
      namespace = string
    })
  })
}