variable "unique_postfix" {
  type        = string
  description = "Random string append to name of key vault."
}

variable "identity" {
  type        = any
  description = "The identity granted access for reading secrets."
}

variable "resource_group" {
  type        = string
  description = "The resource group in which the key vault is to be created."
}

variable "location" {
  type        = string
  description = "The location in which the key valult is to be created."
}

variable "name" {
  type        = string
  description = "The base name of the key vault."
}

variable "sku" {
  type        = string
  description = "The selected resource offer level."
}

variable "soft_delete_retention_days" {
  type        = number
  description = "The number of days that items should be retained for once soft-deleted."
  default     = 7
}

variable "purge_protection_enabled" {
  type        = bool
  description = "Deleting the Key Vault with Purge Protection Enabled will schedule the Key Vault to be deleted (which will happen by Azure in the configured number of days."
  default     = false
}