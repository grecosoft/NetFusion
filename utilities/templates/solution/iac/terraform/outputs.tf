output "resource_group_name" {
  value = azurerm_resource_group.solution_rg.name
}

output "key_vault_id" {
  value = module.key_vault.key_vault_id
}

output "app_config_id" {
  value = module.app_config.app_config_id
}

output "workload_identity" {
  value = module.workload_identity.identity
}

output "GITHUB_SECRETS" {
  value = {
    Description           = "Add the following as Github Repository Secrets"
    AZURE_TENANT_ID       = azuread_service_principal.github_workflow_sp.application_tenant_id
    AZURE_CLIENT_ID       = azuread_application.github_workflow.client_id
    AZURE_SUBSCRIPTION_ID = data.azurerm_subscription.current.subscription_id,
    APP_CONFIG_GROUP      = azurerm_resource_group.solution_rg.name,
    APP_CONFIG_NAME       = module.app_config.app_config_name
  }
}

output "HELM_CHART_VARIABLES" {
  value = {
    Description               = "The following values are passed to microservice helm chart (also stored within App Configuration)"
    SolutionTenantId          = local.tenant_id
    SolutionClientId          = local.workload_client_id
    SolutionAppConfigEndpoint = local.app_config_endpoint
    SolutionKeyVaultName      = local.key_vault_name
  }
}