#!/bin/bash

# Builds and creates a Docker image for the microservice.
# The build image is then pushed to the local image registry:

docker build -t [nf:microservice-name]:latest -f ../Dockerfile ../
docker tag [nf:microservice-name]:latest localhost:[nf:local-registry-port]/[nf:microservice-name]:latest
docker push localhost:[nf:local-registry-port]/[nf:microservice-name]:latest

