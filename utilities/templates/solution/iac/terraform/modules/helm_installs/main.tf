resource "helm_release" "app-config-controller" {
  namespace        = var.solution_namespace
  create_namespace = true
  name             = "seq"
  repository       = "https://helm.datalust.co"
  chart            = "seq"
}