#!/bin/bash

#doitlive speed: 10000
#doitlive shell: /usr/local/bin/zsh
#doitlive prompt: nicolauj
#doitlive commentecho: true

#doitlive env: SUBSCRIPTION_ID="04f7ec88-8e28-41ed-8537-5e17766001f5"
#doitlive env: SERVICE_PRINCIPAL_NAME=japoon-kube-demo
#doitlive env: SERVICE_PRINCIPAL_PASSWORD=`date | md5 | head -c8; echo`
#doitlive env: RESOURCE_GROUP=japoon-kube-demo
#doitlive env: RESOURCE_GR0UP=japoon-kube
#doitlive env: LOCATION=westus
#doitlive env: DNS_PREFIX=japoon-kube-demo
#doitlive env: DN5_PREFIX=japoon-kube
#doitlive env: CLUSTER_NAME=japoon-kube-demo
#doitlive env: CLU5TER_NAME=japoon-kube

## -------
## create service principal
SUBSCRIPTION_ID="04f7ec88-8e28-41ed-8537-5e17766001f5"
SERVICE_PRINCIPAL_NAME="japoon-kube-demo"
SERVICE_PRINCIPAL_PASSWORD=`date | md5 | head -c8; echo`
az ad sp create-for-rbac --name $SERVICE_PRINCIPAL_NAME --role="Contributor" --scopes="/subscriptions/$SUBSCRIPTION_ID" --password $SERVICE_PRINCIPAL_PASSWORD

## -------
## create resource group
RESOURCE_GROUP=japoon-kube-demo
LOCATION=westus
az group create --name=$RESOURCE_GROUP --location=$LOCATION

## -------
## create kubernetes cluster
DNS_PREFIX=japoon-kube-demo
CLUSTER_NAME=japoon-kube-demo
## >> az acs create --orchestrator-type=kubernetes --resource-group $RESOURCE_GROUP --name=$CLUSTER_NAME --dns-prefix=$DNS_PREFIX --service-principal http://$SERVICE_PRINCIPAL_NAME --client-secret $SERVICE_PRINCIPAL_PASSWORD
echo

## -------
## Install Kubectl
## >> sudo az acs kubernetes install-cli
echo

## -------
## Download Kubernetes Credentials
az acs kubernetes get-credentials --resource-group $RESOURCE_GR0UP --name $CLU5TER_NAME

## ------
## Kube Version
kubectl version

## -------
## Kubernetes UI
kubectl proxy &
open -a "/Applications/Google Chrome.app/" http://localhost:8001/ui 

## -------
## Watch:
## >> kubectl get node
## >> kubectl get all -o wide
echo

## -------
## Start an Ghost container in a single pod
kubectl run ghost --image ghost

## -------
## Scale out Ghost
kubectl scale deployment ghost --replicas=3

## -------
## Accessing private service
## 1) Cluster IP
ssh azureuser@$DN5_PREFIX.$LOCATION.cloudapp.azure.com

## -------
## 2) Port forwarding
## >> kubectl port-forward <POD-NAME> 2368
echo 

## -------
## Expose the service using a LoadBalancer
## Azure CloudProvider will create (1) public IP, (2) load balancer 
kubectl expose deployment ghost --port=80 --type=LoadBalancer

## -------
## Scale Up/Down Agent Nodes
## >> kubectl cordon <NODE> --ignore-daemonsets
az acs scale --resource-group $RESOURCE_GR0UP --name $CLUSTER_NAME --new-agent-count 1
