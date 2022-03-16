#!/bin/bash

#==========================================================================
# Builds and pushes an image for the microservice to the local registery
# that is deployed into the cluster by applying the Kubernetes deployment. 
#==========================================================================

./build-docker-image.sh

kubectl create configmap [nf:microservice-name].settings -n [nf:kube-namespace] \
--from-file=../src/Solution.Context.WebApi/appsettings.json

kubectl create secret generic [nf:microservice-name].secrets -n [nf:kube-namespace]

kubectl apply -f ../deploy/deployment.yaml
