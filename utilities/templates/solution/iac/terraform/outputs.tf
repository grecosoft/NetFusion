output "GITHUB_SECRETS" {
  value = {
    Description           = "Add the following as Github Repository Secrets for Environment: ${var.solution.environment}"
    AZURE_TENANT_ID       = azuread_service_principal.github_workflow_sp.application_tenant_id
    AZURE_CLIENT_ID       = azuread_application.github_workflow.client_id
    AZURE_SUBSCRIPTION_ID = data.azurerm_subscription.current.subscription_id,
    APP_CONFIG_GROUP      = azurerm_resource_group.solution_rg.name,
    APP_CONFIG_NAME       = module.app_config.app_config_name
  }
}