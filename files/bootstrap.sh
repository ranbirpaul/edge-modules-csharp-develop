#!/bin/bash

ABILITYDIR=/var/ability
ENVFILE=${1:-ability/conf/edge.env}
VAGRANTHOME=$PWD
UPGRADESYSTEM=0

echo "ABB Ability Edge provisioning"
echo 'debconf debconf/frontend select Noninteractive' | debconf-set-selections

if [ $UPGRADESYSTEM -ne 0 ]; then
    echo "Upgrading a Host OS"

    # Update apt-get
    apt-get update -y

    # Update Ubuntu
    apt-get -y upgrade
    apt-get -y dist-upgrade

    # Install recommended extra packages
    apt-get install -y \
        apt-transport-https \
        ca-certificates \
        curl \
        unzip \
        jq

    # Add Docker’s official GPG key
    curl -fsSL https://download.docker.com/linux/ubuntu/gpg | sudo apt-key add -

    # Set up the stable repo
    add-apt-repository \
    "deb [arch=amd64] https://download.docker.com/linux/ubuntu \
    $(lsb_release -cs) \
    stable"

    # Update the packages
    apt-get update

    # Install docker-ce
    apt-get install -y docker-ce
fi

# Access docker w/o sudo
usermod -aG docker $USER

# Edge configuration
set -o allexport
. $ENVFILE
set +o allexport

echo "We are in $VAGRANTHOME"
. $VAGRANTHOME/ability-install.sh

echo "Ability Edge bootstrap complete."
echo "You can vagrant ssh now into your machine."
echo ""

exit 0
