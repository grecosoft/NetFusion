data "terraform_remote_state" "infrastructure" {
  backend = "azurerm"

  config = {
    resource_group_name  = "terraform"
    storage_account_name = "state1560806410"
    container_name       = "infrastructure"
    key                  = "cluster.tfstate"
  }
}

locals {
  cluster_resource_group_name = data.terraform_remote_state.infrastructure.outputs.resource_group_name
  cluster_name                = data.terraform_remote_state.infrastructure.outputs.cluster_name
  cluster_oidc_issuer_url     = data.terraform_remote_state.infrastructure.outputs.oidc_issuer_url
  registry_name               = data.terraform_remote_state.infrastructure.outputs.registry_name
}

// Reference to Kubernetes services to deploy the microservice solution:
data "azurerm_kubernetes_cluster" "k8s" {
  name                = local.cluster_name
  resource_group_name = local.cluster_resource_group_name
}

data "azurerm_container_registry" "acr" {
  name                = local.registry_name
  resource_group_name = local.cluster_resource_group_name
}
