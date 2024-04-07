data "azurerm_kubernetes_cluster" "k8s" {
  name                = var.solution.cluster_name
  resource_group_name = var.solution.cluster_resource_group
}

data "azurerm_container_registry" "acr" {
  name                = var.solution.registry_name
  resource_group_name = var.solution.cluster_resource_group
}

locals {
  cluster_oidc_issuer_url = data.azurerm_kubernetes_cluster.k8s.oidc_issuer_url
}

