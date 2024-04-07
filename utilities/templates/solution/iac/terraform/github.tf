// Provides access to the services required to deploy
// microserivces from GitHub Workflows.
// https://learn.microsoft.com/en-us/azure/developer/github/connect-from-azure?tabs=azure-cli%2Clinux

data "azurerm_subscription" "current" {
}

resource "azuread_application" "github_workflow" {
    display_name = "github-workflow-${lower(var.solution.name)}-${var.solution.environment}"
}

resource "azuread_service_principal" "github_workflow_sp" {
  client_id    = azuread_application.github_workflow.client_id
  use_existing = true
}

resource "azurerm_role_assignment" "workflow_role" {
  scope                = azurerm_resource_group.solution_rg.id
  role_definition_name = "Contributor"
  principal_id         = azuread_service_principal.github_workflow_sp.object_id
}

resource "azuread_application_federated_identity_credential" "github_federated_identity" {
  application_id = azuread_application.github_workflow.id
  display_name   = "GitHubWorkflowActions"
  description    = "Allows GitHub Workflow actions to have access to Azure Resources required for deployment."
  audiences      = ["api://AzureADTokenExchange"]
  issuer         = "https://token.actions.githubusercontent.com"
  subject        = "repo:${var.solution.github_account}/${var.solution.name}:environment:${var.solution.environment}"
}

// Roles required by executing workflows:

resource "azurerm_role_assignment" "AcrPull" {
  principal_id                     = azuread_service_principal.github_workflow_sp.object_id
  role_definition_name             = "AcrPull"
  scope                            = data.azurerm_container_registry.acr.id
  skip_service_principal_aad_check = true
}

resource "azurerm_role_assignment" "AcrPush" {
  principal_id                     = azuread_service_principal.github_workflow_sp.object_id
  role_definition_name             = "AcrPush"
  scope                            = data.azurerm_container_registry.acr.id
  skip_service_principal_aad_check = true
}

resource "azurerm_role_assignment" "AksContributor" {
  principal_id                     = azuread_service_principal.github_workflow_sp.object_id
  role_definition_name             = "Contributor"
  scope                            = data.azurerm_kubernetes_cluster.k8s.id
  skip_service_principal_aad_check = true
}

resource "azurerm_role_assignment" "AacReader" {
  principal_id                     = azuread_service_principal.github_workflow_sp.object_id
  role_definition_name             = "App Configuration Data Reader"
  scope                            = module.app_config.app_config_id
  skip_service_principal_aad_check = true
}


// Application Configuration Data required when deploying solution microservices:
resource "azurerm_app_configuration_key" "SolutionTenantId" {
  configuration_store_id = module.app_config.app_config_id
  key                    = "SolutionTenantId"
  value                  = local.tenant_id
}

resource "azurerm_app_configuration_key" "SolutionClientId" {
  configuration_store_id = module.app_config.app_config_id
  key                    = "SolutionClientId"
  value                  = local.workload_client_id
}

resource "azurerm_app_configuration_key" "SolutionAppConfigEndpoint" {
  configuration_store_id = module.app_config.app_config_id
  key                    = "SolutionAppConfigEndpoint"
  value                  = local.app_config_endpoint
}

resource "azurerm_app_configuration_key" "SolutionKeyVaultName" {
  configuration_store_id = module.app_config.app_config_id
  key                    = "SolutionKeyVaultName"
  value                  = local.key_vault_name
}
