#!/bin/bash

dashboard_token_name=`(kubectl get secrets | grep -oP '(^dashboard-admin-sa-token[^\s]+)')`
dashboard_token_value=`(kubectl describe secret $token_name)`

echo ""
echo "-------------------------------------------------------"
echo "COMPLETE THE FOLLOWING TO VIEW THE KUBERNETES DASHBOARD"
echo "-------------------------------------------------------"
echo "RUN:    kubectl proxy"
echo "OPEN:   http://localhost:8001/api/v1/namespaces/kubernetes-dashboard/services/https:kubernetes-dashboard:/proxy/."
echo "TOKEN:  "$dashboard_token_value


echo ""
echo "-------------------------------------------------------"
echo "COMPLETE THE FOLLOWING TO VIEW THE SEQ DASHBOARD"
echo "-------------------------------------------------------"
echo "RUN:    kubectl port-forward \"service/seq\" -n [nf:kube-namespace] 8081:80"
echo "OPEN:   http://localhost:8081"

rabbit_username="$(kubectl get secret microservice-integration-default-user -n [nf:kube-namespace] -o jsonpath='{.data.username}' | base64 --decode)"
rabbit_password="$(kubectl get secret microservice-integration-default-user -n [nf:kube-namespace] -o jsonpath='{.data.password}' | base64 --decode)"

echo ""
echo "-------------------------------------------------------"
echo "COMPLETE THE FOLLOWING TO VIEW THE RABBITMQ DASHBOARD"
echo "-------------------------------------------------------"
echo "RUN:    kubectl port-forward \"service/microservice-integration\" -n [nf:kube-namespace] 15672"
echo "OPEN:   http://localhost:15672"
echo "USER NAME:  "$rabbit_username
echo "PASSWORD:   "$rabbit_password

