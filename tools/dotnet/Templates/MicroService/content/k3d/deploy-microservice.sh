#!/bin/bash

./build-docker-image.sh

kubectl create configmap [nf:microservice-name].settings -n [nf:kube-namespace] \
--from-file=../src/Solution.Context.WebApi/appsettings.json

kubectl create secret generic [nf:microservice-name].secrets -n [nf:kube-namespace]


kubectl apply -f ../deploy/deployment.yaml
