terraform {
  required_version = ">=1.0"

  backend "azurerm" {
    resource_group_name  = "Terraform"
    storage_account_name = "terraformstate06410"
    container_name       = "[nf:solution-name]"
    key                  = "microservice.[nf:service-name].tfstate"
  }
  required_providers {

    azurerm = {
      source  = "hashicorp/azurerm"
      version = "=3.82.0"
    }
  }
}

provider "azurerm" {
  features {}
}