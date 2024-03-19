variable "resource_group" {
  type        = string
  description = "The resource group in which the workload identity should be created."
}

variable "location" {
  type        = string
  description = "The location where the workload identity should be created."
}

variable "identity_name" {
  type        = string
  description = "The name of the identity associated with workload."
}

variable "namespace" {
  type        = string
  description = "The Kubernetes namespace to create ServiceAccount for the workload identity."
}

variable "oidc_issuer_url" {
  description = "The OIDC issuer url of the AKS cluster."
  type        = string
}