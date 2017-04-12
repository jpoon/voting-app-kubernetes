# Voting-App-Kubernetes

Inspired by [example-voting-app](https://github.com/docker/example-voting-app)

## Architecture

* vote: Python web app which allows you to vote between two options. The vote is saved onto an Azure Storage queue
* worker: .NET core worker consumes votes from the Azure Storage queue and stores them in an Azure Storage Table
* result: node.js web app which reads the Azure Storage Table and displays the results of the voting in real-time

## Deployment

### Create Azure Secret

Base64 encode Azure Storage Account Name and Key:

```
$ echo -n "[azure-storage-account-name]" | base64
$ echo -n "[azure-storage-account-key]" | base64
```

Create Kubernetes Secret

> secret-azurestorage.yaml:

```
apiVersion: v1
kind: Secret
metadata:
  name: azure-storage
type: Opaque
data:
  account-name: [azure-storage-account-name | base64]
  access-key: [azure-storage-account-key | base64]
```

```
$ kubectl create -f secret-azurestorage.yaml
```

### Deploy to Kubernetes

```
kubectl create -f vote.yaml
kubectl create -f worker.yaml
kubectl create -f result.yaml
```

*Note*: You will need to build, tag, and deploy the Docker images to a registry and update the Kubernetes manifests with the location of the images.