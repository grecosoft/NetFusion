#!/bin/bash

./build-docker-image.sh
kubectl apply -f ../deploy/deployment.yaml

