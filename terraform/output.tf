output "update_commands" {
  value = "az webapp deploy -g '${azurerm_resource_group.main.name}' -n '${azurerm_linux_web_app.backend.name}' --src-path binaries.zip --restart true && az webapp restart --resource-group '${azurerm_resource_group.main.name}' --name '${azurerm_linux_web_app.backend.name}' && curl --fail --retry 12 --retry-all-errors --retry-delay 5 'https://${azurerm_linux_web_app.backend.default_hostname}/health/'"
}
