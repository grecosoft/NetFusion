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

# Configure the Kubernetes Dashboard Web Interface:
# https://www.replex.io/blog/how-to-install-access-and-add-heapster-metrics-to-the-kubernetes-dashboard
kubectl apply -f https://raw.githubusercontent.com/kubernetes/dashboard/v2.0.0/aio/deploy/recommended.yaml
kubectl create serviceaccount dashboard-admin-sa
kubectl create clusterrolebinding dashboard-admin-sa --clusterrole=cluster-admin --serviceaccount=default:dashboard-admin-sa
token_name=`(kubectl get secrets | grep -oP '(^dashboard-admin-sa-token[^\s]+)')`
token_value=`(kubectl describe secret $token_name)`

echo ""
echo "-------------------------------------------------------"
echo "COMPLETE THE FOLLOWING TO VIEW THE KUBERNETES DASHBOARD"
echo "-------------------------------------------------------"
echo "RUN:    kubectl proxy"
echo "OPEN:   http://localhost:8001/api/v1/namespaces/kubernetes-dashboard/services/https:kubernetes-dashboard:/proxy/."
echo "TOKEN:  "$token_value



