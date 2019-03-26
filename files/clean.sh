#!/bin/bash


ABILITYDIR=/var/ability
ENVFILE=${1:-ability/conf/edge.env}
VAGRANTHOME=$PWD

echo "ABB Ability Edge cleanup"
echo 'debconf debconf/frontend select Noninteractive' | debconf-set-selections

# Edge configuration

set -o allexport
. $ENVFILE
set +o allexport

echo "We are in $VAGRANTHOME"

# Clean after previous edge deployment

docker swarm leave -f
docker container prune -f
docker network prune -f

sudo rm -rf $MODULES_VOLUME
sudo rm -rf $MQTT_VOLUME
sudo rm -rf $MQTT_AUTH_VOLUME

echo "Ability Edge cleanup complete."
echo ""

exit 0


