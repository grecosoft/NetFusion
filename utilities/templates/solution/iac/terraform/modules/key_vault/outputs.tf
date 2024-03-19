output "key_vault_id" {
  value = azurerm_key_vault.kv.id
}

output "key_vault_name" {
  value = azurerm_key_vault.kv.name
}

output "key_vault_key_id" {
  value = azurerm_key_vault_key.key_vault_key.id
}