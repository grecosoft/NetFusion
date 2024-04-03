data "terraform_remote_state" "solution" {
  backend = "azurerm"

  config = {
    resource_group_name  = "Terraform"
    storage_account_name = "terraformstate06410"
    container_name       = "[nf:solution-name]"
    key                  = "solution.tfstate"
  }
}

locals {
  solution_resource_group_name     = data.terraform_remote_state.solution.outputs.resource_group_name
  solution_workload_identity       = data.terraform_remote_state.solution.outputs.workload_identity
  solution_key_vault_id            = data.terraform_remote_state.solution.outputs.key_vault_id
  solution_app_config_id           = data.terraform_remote_state.solution.outputs.app_config_id
} 