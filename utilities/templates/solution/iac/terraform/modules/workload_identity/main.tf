// Create an user identity and federate with the cluster:
resource "azurerm_user_assigned_identity" "solution-identity" {
  resource_group_name = var.resource_group
  location            = var.location
  name                = "${lower(var.identity_name)}-identity"
}

resource "azurerm_federated_identity_credential" "solution-federated-identity" {
  name                = "${azurerm_user_assigned_identity.solution-identity.name}-federated"
  resource_group_name = var.resource_group
  audience            = ["api://AzureADTokenExchange"]
  issuer              = var.oidc_issuer_url
  parent_id           = azurerm_user_assigned_identity.solution-identity.id
  subject             = "system:serviceaccount:${var.namespace}:${azurerm_user_assigned_identity.solution-identity.name}"
}

resource "kubernetes_service_account" "sa" {
  metadata {
    name      = azurerm_user_assigned_identity.solution-identity.name
    namespace = lower(var.namespace)
    annotations = {
      "azure.workload.identity/client-id" = azurerm_user_assigned_identity.solution-identity.client_id
    }
  }
}

