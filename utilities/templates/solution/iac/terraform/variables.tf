variable "solution" {
  type = object({
    github_account = string
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