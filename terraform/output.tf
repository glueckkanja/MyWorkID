output "update_commands" {
  value = "az webapp deploy -g '${azurerm_resource_group.main.name}' -n '${azurerm_linux_web_app.backend.name}' --src-path binaries.zip \naz webapp restart --resource-group '${azurerm_resource_group.main.name}' --name '${azurerm_linux_web_app.backend.name}'"
}
