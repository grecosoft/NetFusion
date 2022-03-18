#!/bin/bash

k3d cluster delete [nf:local-cluster-name]
k3d registry delete [nf:local-cluster-name]-registry