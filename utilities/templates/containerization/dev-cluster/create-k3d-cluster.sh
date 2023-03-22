#!/bin/bash

# Create a local Kubernetes cluster and Docker registry.  
# The local build creates a Docker image for the Microserivce and pushes it to the local image registery
# and pulled by the cluster when deployed.  

k3d registry create [nf:local-cluster-name]-registry --port 0.0.0.0:[nf:local-registry-port]
k3d cluster create [nf:local-cluster-name] -p "8081:80@loadbalancer" -a 3 --registry-use [nf:local-cluster-name]-registry:[nf:local-registry-port]


# Deploy NgInx Ingress Controller used to expose services external to the cluster:

kubectl create deployment nginx --image=nginx
kubectl create service clusterip nginx --tcp=80:80
kubectl apply -f nginx-ingress.yaml


# Create a Kubernetes namespace for the Microservice solution:

kubectl create namespace [nf:kube-namespace]


# Install SEQ for central Microservice logging:

helm repo add datalust https://helm.datalust.co
helm repo update
helm install seq datalust/seq -n [nf:kube-namespace]