data "azurerm_resource_group" "solution_rg" {
  name = local.solution_resource_group_name
}

data "azurerm_client_config" "current" {}

// Microserivce prefixed configurations:
resource "azurerm_app_configuration_key" "seq" {
  configuration_store_id = local.solution_app_config_id
  key                    = "${var.service_name}/logging:seqUrl"
  value                  = "http://seq:5341"
}