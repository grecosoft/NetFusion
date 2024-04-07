locals {
  unique_postfix = lower(random_string.unique_postfix.result)
}


resource "random_string" "unique_postfix" {
  length  = 7
  special = false
}

// Create an Azure resource group to contain the solution's related resources:
resource "azurerm_resource_group" "solution_rg" {
  name     = "${var.solution.name}-${var.solution.environment}"
  location = var.solution.location
}

// Create a Kubernetes namespace to contain the solution's related resources:
resource "kubernetes_namespace" "solution_ns" {
  metadata {
    name = "${lower(var.solution.name)}-${var.solution.environment}"
  }
}

// Create workload identity for the solution.  This identity will be used to
// authenticate the Azure resources used by the solution.
module "workload_identity" {
  source          = "./modules/workload_identity"
  oidc_issuer_url = local.cluster_oidc_issuer_url
  resource_group  = azurerm_resource_group.solution_rg.name
  location        = azurerm_resource_group.solution_rg.location
  identity_name   = var.solution.name
  namespace       = kubernetes_namespace.solution_ns.metadata[0].name
}

//-- Create the Azure resources used by the solution and the associated
//   Kubernetes controller used to integrate the Azure resource for use
//   by solution defined microservices:
module "key_vault" {
  source                     = "./modules/key_vault"
  identity                   = module.workload_identity.identity
  resource_group             = azurerm_resource_group.solution_rg.name
  location                   = azurerm_resource_group.solution_rg.location
  name                       = "${var.solution.name}-secrets"
  unique_postfix             = local.unique_postfix
  sku                        = var.solution.keyvault.sku
  soft_delete_retention_days = var.solution.keyvault.soft_delete_retention_days
  purge_protection_enabled   = var.solution.keyvault.purge_protection_enabled
}

//-- Create the Azure resources used by the solution and the associated
//   Kubernetes controller used to integrate the Azure resource for use
//   by solution defined microservices:
module "app_config" {
  source                  = "./modules/app_config"
  resource_group          = azurerm_resource_group.solution_rg.name
  resource_group_id       = azurerm_resource_group.solution_rg.id
  location                = azurerm_resource_group.solution_rg.location
  name                    = "${var.solution.name}-configs"
  unique_postfix          = local.unique_postfix
  identity                = module.workload_identity.identity
  cluster_oidc_issuer_url = local.cluster_oidc_issuer_url
  namespace               = var.solution.appconfig.namespace
  key_vault_key_id        = module.key_vault.key_vault_key_id
  depends_on              = [module.key_vault]
}

module "helm_installs" {
  source             = "./modules/helm_installs"
  solution_namespace = kubernetes_namespace.solution_ns.metadata[0].name
}

locals {
  tenant_id           = module.workload_identity.identity.tenant_id
  workload_client_id  = module.workload_identity.identity.client_id
  app_config_endpoint = module.app_config.app_config_endpoint
  key_vault_name      = module.key_vault.key_vault_name
}