# https://learn.microsoft.com/en-us/azure/azure-app-configuration/quickstart-azure-kubernetes-service
# https://learn.microsoft.com/en-us/azure/azure-app-configuration/reference-kubernetes-provider?tabs=default

data "azurerm_client_config" "current" {}

resource "azurerm_role_assignment" "appconf_dataowner" {
  scope                = var.resource_group_id
  role_definition_name = "App Configuration Data Owner"
  principal_id         = data.azurerm_client_config.current.object_id
}

resource "time_sleep" "role_assignment_sleep" {
  create_duration = "60s"

  triggers = {
    role_assignment = azurerm_role_assignment.appconf_dataowner.id
  }
}

resource "azurerm_app_configuration" "appconfig" {
  name                = "${var.name}-${var.unique_postfix}"
  resource_group_name = var.resource_group
  location            = var.location
  sku                 = "standard"

  identity {
    type = "UserAssigned"
    identity_ids = [
      var.identity.id
    ]
  }

  encryption {
    key_vault_key_identifier = var.key_vault_key_id
    identity_client_id       = var.identity.client_id
  }

  depends_on = [
    azurerm_role_assignment.appconf_dataowner,
    time_sleep.role_assignment_sleep
  ]
}

# Create the federated identity credential between the managed identity, OIDC issuer, and subject:
resource "azurerm_federated_identity_credential" "federatedconfig" {
  name                = "${var.identity.name}-federatedconfig"
  resource_group_name = var.resource_group
  audience            = ["api://AzureADTokenExchange"]
  issuer              = var.cluster_oidc_issuer_url
  parent_id           = var.identity.id
  subject             = "system:serviceaccount:${var.namespace}:az-appconfig-k8s-provider"
}

# Grant the user-assigned managed identity App Configuration Data Reader role in Azure App Configuration:
resource "azurerm_role_assignment" "reader" {
  scope                = azurerm_app_configuration.appconfig.id
  role_definition_name = "App Configuration Data Reader"
  principal_id         = var.identity.principal_id
}


