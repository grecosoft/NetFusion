#!/bin/bash

#==========================================================================
# Microservice Integration: RabbitMQ
#==========================================================================

kubectl apply -f "https://github.com/rabbitmq/cluster-operator/releases/latest/download/cluster-operator.yml"
kubectl apply -f rabbitmq.yaml -n [nf:kube-namespace]
