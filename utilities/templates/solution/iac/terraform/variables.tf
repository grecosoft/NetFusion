variable "solution" {
  type = object({
    cluster_resource_group = string
    cluster_name = string
    registry_name = string
    github_account = string
    environment = string
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