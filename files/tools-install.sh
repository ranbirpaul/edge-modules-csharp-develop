#!/bin/bash

#Install Azure CLI
AZ_REPO=$(lsb_release -cs)
echo "deb [arch=amd64] https://packages.microsoft.com/repos/azure-cli/ $AZ_REPO main" | \
sudo tee /etc/apt/sources.list.d/azure-cli.list
curl -L https://packages.microsoft.com/keys/microsoft.asc | sudo apt-key add -

sudo apt-get update -y
sudo apt-get install -y libssl-dev libffi-dev python-dev apt-transport-https azure-cli

#Install needed tools
sudo apt-get install -y \
	nodejs \
	jq \
	npm \