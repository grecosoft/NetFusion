#!/bin/bash

#========================================================================
# Create a local Kubernetes cluster and Docker registry.  
#========================================================================
# The local build creates a Docker image for the Microserivce and pushes 
# it to the local image registery and pulled by the cluster when deployed
# (deploy-microservice.sh) 

#!/bin/sh
set -o errexit

# create registry container unless it already exists
reg_name='[nf:local-cluster-name]-registry'
reg_port='[nf:local-registry-port]'
if [ "$(docker inspect -f '{{.State.Running}}' "${reg_name}" 2>/dev/null || true)" != 'true' ]; then
  docker run \
    -d --restart=always -p "127.0.0.1:${reg_port}:5000" --name "${reg_name}" \
    registry:2
fi

# create a cluster with the local registry enabled in containerd
cat <<EOF | kind create cluster --config=-
kind: Cluster
apiVersion: kind.x-k8s.io/v1alpha4
name: [nf:local-cluster-name]
nodes:
- role: control-plane
  kubeadmConfigPatches:
  - |
    kind: InitConfiguration
    nodeRegistration:
      kubeletExtraArgs:
        node-labels: "ingress-ready=true"
  extraPortMappings:
  - containerPort: 80
    hostPort: 80
    protocol: TCP
  - containerPort: 443
    hostPort: 443
    protocol: TCP
- role: worker
- role: worker
containerdConfigPatches:
- |-
  [plugins."io.containerd.grpc.v1.cri".registry.mirrors."localhost:${reg_port}"]
    endpoint = ["http://${reg_name}:5000"]
EOF


# connect the registry to the cluster network if not already connected
if [ "$(docker inspect -f='{{json .NetworkSettings.Networks.kind}}' "${reg_name}")" = 'null' ]; then
  docker network connect "kind" "${reg_name}"
fi

cat <<EOF | kubectl apply -f -
apiVersion: v1
kind: ConfigMap
metadata:
  name: local-registry-hosting
  namespace: kube-public
data:
  localRegistryHosting.v1: |
    host: "localhost:${reg_port}"
    help: "https://kind.sigs.k8s.io/docs/user/local-registry/"
EOF


#========================================================================
# Deploy NgInx Ingress Controller used to expose services external to
# the cluster:
#========================================================================
kubectl apply -f https://raw.githubusercontent.com/kubernetes/ingress-nginx/main/deploy/static/provider/kind/deploy.yaml


#========================================================================
# Create a Kubernetes namespace for the Microservice solution:
#========================================================================
kubectl create namespace [nf:kube-namespace]


#========================================================================
# Install SEQ for central Microservice logging:
#========================================================================
helm repo add datalust https://helm.datalust.co
helm repo update
helm install seq datalust/seq -n [nf:kube-namespace]

#========================================================================
# Configure the Kubernetes Dashboard Web Interface:
#========================================================================
# https://www.replex.io/blog/how-to-install-access-and-add-heapster-metrics-to-the-kubernetes-dashboard
kubectl apply -f https://raw.githubusercontent.com/kubernetes/dashboard/v2.0.0/aio/deploy/recommended.yaml
kubectl create serviceaccount dashboard-admin-sa
kubectl create clusterrolebinding dashboard-admin-sa --clusterrole=cluster-admin --serviceaccount=default:dashboard-admin-sa


