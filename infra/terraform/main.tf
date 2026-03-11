terraform {
  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "~> 3.80"
    }
  }
  backend "azurerm" {
    resource_group_name  = "rg-carrental-tfstate"
    storage_account_name = "carrentaltfstate"
    container_name       = "tfstate"
    key                  = "prod.terraform.tfstate"
  }
}

provider "azurerm" {
  features {}
}

variable "location" { default = "West Europe" }
variable "environment" { default = "prod" }
variable "project" { default = "driveshare" }

locals {
  name_prefix = "${var.project}-${var.environment}"
  tags = {
    Project     = var.project
    Environment = var.environment
    ManagedBy   = "Terraform"
  }
}

# ── Resource Group ─────────────────────────────────────────
resource "azurerm_resource_group" "main" {
  name     = "rg-${local.name_prefix}"
  location = var.location
  tags     = local.tags
}

# ── Container Apps Environment ─────────────────────────────
resource "azurerm_log_analytics_workspace" "main" {
  name                = "law-${local.name_prefix}"
  resource_group_name = azurerm_resource_group.main.name
  location            = var.location
  sku                 = "PerGB2018"
  tags                = local.tags
}

resource "azurerm_container_app_environment" "main" {
  name                       = "cae-${local.name_prefix}"
  resource_group_name        = azurerm_resource_group.main.name
  location                   = var.location
  log_analytics_workspace_id = azurerm_log_analytics_workspace.main.id
  tags                       = local.tags
}

# ── SQL Server ─────────────────────────────────────────────
resource "azurerm_mssql_server" "main" {
  name                         = "sql-${local.name_prefix}"
  resource_group_name          = azurerm_resource_group.main.name
  location                     = var.location
  version                      = "12.0"
  administrator_login          = "sqladmin"
  administrator_login_password = var.sql_admin_password
  tags                         = local.tags
}

variable "sql_admin_password" { sensitive = true }

resource "azurerm_mssql_database" "users" {
  name      = "CarRental_Users"
  server_id = azurerm_mssql_server.main.id
  sku_name  = "S1"
}

resource "azurerm_mssql_database" "bookings" {
  name      = "CarRental_Bookings"
  server_id = azurerm_mssql_server.main.id
  sku_name  = "S1"
}

resource "azurerm_mssql_database" "payments" {
  name      = "CarRental_Payments"
  server_id = azurerm_mssql_server.main.id
  sku_name  = "S1"
}

resource "azurerm_mssql_database" "reviews" {
  name      = "CarRental_Reviews"
  server_id = azurerm_mssql_server.main.id
  sku_name  = "S1"
}

# ── Cosmos DB ──────────────────────────────────────────────
resource "azurerm_cosmosdb_account" "main" {
  name                = "cosmos-${local.name_prefix}"
  resource_group_name = azurerm_resource_group.main.name
  location            = var.location
  offer_type          = "Standard"
  kind                = "GlobalDocumentDB"
  tags                = local.tags

  consistency_policy {
    consistency_level = "Session"
  }

  geo_location {
    location          = var.location
    failover_priority = 0
  }
}

resource "azurerm_cosmosdb_sql_database" "listings" {
  name                = "CarRentalListings"
  resource_group_name = azurerm_resource_group.main.name
  account_name        = azurerm_cosmosdb_account.main.name
}

resource "azurerm_cosmosdb_sql_container" "listings_container" {
  name                = "Listings"
  resource_group_name = azurerm_resource_group.main.name
  account_name        = azurerm_cosmosdb_account.main.name
  database_name       = azurerm_cosmosdb_sql_database.listings.name
  partition_key_path  = "/id"
  throughput          = 400
}

# ── Service Bus ────────────────────────────────────────────
resource "azurerm_servicebus_namespace" "main" {
  name                = "sb-${local.name_prefix}"
  resource_group_name = azurerm_resource_group.main.name
  location            = var.location
  sku                 = "Standard"
  tags                = local.tags
}

resource "azurerm_servicebus_topic" "booking_confirmed" {
  name         = "booking-confirmed"
  namespace_id = azurerm_servicebus_namespace.main.id
}

resource "azurerm_servicebus_topic" "booking_cancelled" {
  name         = "booking-cancelled"
  namespace_id = azurerm_servicebus_namespace.main.id
}

# ── Storage Account ────────────────────────────────────────
resource "azurerm_storage_account" "main" {
  name                     = "st${replace(local.name_prefix, "-", "")}001"
  resource_group_name      = azurerm_resource_group.main.name
  location                 = var.location
  account_tier             = "Standard"
  account_replication_type = "LRS"
  tags                     = local.tags
}

# ── Outputs ────────────────────────────────────────────────
output "sql_server_fqdn" { value = azurerm_mssql_server.main.fully_qualified_domain_name }
output "cosmos_endpoint" { value = azurerm_cosmosdb_account.main.endpoint }
output "servicebus_namespace" { value = azurerm_servicebus_namespace.main.name }
output "storage_account_name" { value = azurerm_storage_account.main.name }
output "container_app_environment_id" { value = azurerm_container_app_environment.main.id }
