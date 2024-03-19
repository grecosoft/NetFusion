variable "identity" {
  type        = any
  description = "The identity granted access for reading configurations."
}

variable "unique_postfix" {
  type        = string
  description = "Random string append to name of key vault."
}

variable "resource_group" {
  type        = string
  description = "The resource group in which the app configuration is to be created."
}

variable "resource_group_id" {
  type = string
}

variable "location" {
  type        = string
  description = "The location in which the app configuration is to be created."
}

variable "name" {
  type        = string
  description = "The name of the app configuration."
}

variable "cluster_oidc_issuer_url" {
  type        = string
  description = "The OIDC Issuer URL of the Kubernete's cluster."
}

variable "namespace" {
  type        = string
  description = "The name of the Kubernetes namespace into which the controller is installed."
}

variable "key_vault_key_id" {
  type        = string
  description = "The key vault key used to integrate key vault with app configuration service."
}