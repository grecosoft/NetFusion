# Microservice Solution

This dotnet template has created a Microservice based solution to which microservices can be added.

## Contain Source
This template adds the following:
- Helm Chart containing a template to deploy microservices to Kubernetes
- Project containing solution source shared by microservices within the solution 
- GitHub Workflow for building the Helm Chart and publishing package to ACR
- GitHub Workflow for building the shared solution source and publishing NuGets

## Adding Microservices
The netfusion-microservice template can be used to add microservices to the solution.

The microservice template adds the following:
- Microservice solution containing an WebApi, UnitTests, and supporting components
- Dockerfile for building an image for the microservice
- Initial Terraform directory containing starter script

Assuming that the solution name is HomeLink and a Management service is required, execute the following to add a new microservice to the solution:

``` bash
cd ./microservices
mkdir HomeLink.Management
cd ./HomeLink.Management

dotnet new netfusion-microservice -p 7200 -ek true 

```

# Deploying Microservice
Once the microservice has been created, execute the following to apply the netfusion-deployment template to add the following:
- Helm Template Value files used to deploy the microservice using the Microservice Helm Chart
- GitHub Workflow used to build, test, and publish the microservice's solution code
- Creates a Docker image from the published source and publishes it to ACR
- Deploys the microservice to AKS by applying the microservice helm-chart

``` bash

dotnet new netfusion-deployment \
  --aks-group kube-cluster \
  --aks-name akscluster \
  --aks-namespace refarch \
  --service-account refarch-identity \
  --port 7200 \
  --registry aksclusteracr.azurecr.io \
  --gateway refarch-gateway \
  --host refarch.com \
  --chart-version 1.0.67 \

```