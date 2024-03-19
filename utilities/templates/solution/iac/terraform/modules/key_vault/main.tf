# https://learn.microsoft.com/en-us/azure/aks/csi-secrets-store-identity-access
data "azurerm_client_config" "current" {}

resource "azurerm_key_vault" "kv" {
  name                = "${var.name}-${var.unique_postfix}"
  location            = var.location
  resource_group_name = var.resource_group
  tenant_id           = var.identity.tenant_id
  sku_name            = var.sku

  enabled_for_disk_encryption = true
  soft_delete_retention_days  = var.soft_delete_retention_days
  purge_protection_enabled    = var.purge_protection_enabled
}

resource "azurerm_key_vault_access_policy" "workload-identity" {
  key_vault_id = azurerm_key_vault.kv.id
  tenant_id    = var.identity.tenant_id
  object_id    = var.identity.principal_id

  certificate_permissions = ["Get"]
  key_permissions         = ["Get", "UnwrapKey", "WrapKey"]
  secret_permissions      = ["Get"]
  storage_permissions     = ["Get"]
}

resource "azurerm_key_vault_access_policy" "current" {
  key_vault_id = azurerm_key_vault.kv.id
  tenant_id    = data.azurerm_client_config.current.tenant_id
  object_id    = data.azurerm_client_config.current.object_id

  certificate_permissions = ["Create", "Delete", "Get", "List", "Update"]
  key_permissions         = ["Get", "Create", "Delete", "List", "Import", "Restore", "Recover", "UnwrapKey", "WrapKey", "Purge", "Encrypt", "Decrypt", "Sign", "Verify", "GetRotationPolicy"]
  secret_permissions      = ["Get", "List", "Set", "Delete"]
  storage_permissions     = ["Delete", "DeleteSAS", "Get", "GetSAS", "List", "ListSAS", "Set", "SetSAS", "Update"]
}

resource "azurerm_key_vault_key" "key_vault_key" {
  name         = "solutionKeyVaultKey"
  key_vault_id = azurerm_key_vault.kv.id
  key_type     = "RSA"
  key_size     = 2048
  key_opts = [
    "decrypt",
    "encrypt",
    "sign",
    "unwrapKey",
    "verify",
    "wrapKey"
  ]

  depends_on = [
    azurerm_key_vault_access_policy.workload-identity,
    azurerm_key_vault_access_policy.current,
  ]
}