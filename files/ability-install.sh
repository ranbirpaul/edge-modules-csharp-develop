#!/bin/bash

if [ -z $ENVFILE ];then
	echo "This script should only be included in bootstrap.sh. Don't run it standalone."
	exit 1
fi

# Clean after previous edge deployment
docker swarm leave -f
docker container prune -f
docker network prune -f

sudo rm -rf $MODULES_VOLUME
sudo rm -rf $MQTT_VOLUME
sudo rm -rf $MQTT_AUTH_VOLUME

# Fresh deploy
get_val () {
	regex="$1=(.*)"
	kvp=$(grep "$1=" $ENVFILE)
	if [[ $kvp =~ $regex ]]; then
		val="${BASH_REMATCH[1]}"
		char=$(echo -e "[\n\r]")
		val="${val//$char/}"
		return 0
	else
		val=""
		return 1
	fi
}

get_val IMAGEREGISTRIES
IMAGEREGISTRIES=$val

ACRHOSTURL=$(echo "$IMAGEREGISTRIES" | jq -r '.abb.serveraddress')
echo Registries: $ACRHOSTURL

urlRegex="https://(.*)"
ACRHOST=""
if [[ $ACRHOSTURL =~ $urlRegex ]]; then
	ACRHOST="${BASH_REMATCH[1]}"
else
	ACRHOST=""
fi
if [ -z $ACRHOST ];then
	echo "'serveraddress' for registry 'abb' is not defined in IMAGEREGISTRIES in $ENVFILE"
	exit 1
fi

ACRUSER=$(echo $IMAGEREGISTRIES | jq --raw-output '.abb.username')
if [ -z $ACRUSER ];then
	echo "'username' for registry 'abb' is not defined in IMAGEREGISTRIES in $ENVFILE"
	exit 1
fi

ACRPASSWORD=$(echo $IMAGEREGISTRIES |jq --raw-output '.abb.password')
if [ -z $ACRPASSWORD ];then
	echo "'password' for registry 'abb' is not defined in IMAGEREGISTRIES in $ENVFILE"
	exit 1
fi

# Users and groups 
echo "Creating users & groups..."
module_group_name=${MODULE_GROUP_NAME:-ability-moduleusers}
module_username=${MODULE_USERNAME:-ability-module}
system_username=${SYSTEM_USERNAME:-ability-system}

addgroup --quiet --system $module_group_name
module_group_id=$(getent group $module_group_name | cut -d: -f3)

# Create the system user, or ensure that the existing user has the correct primary group
adduser --quiet --system --gecos "Ability System" --no-create-home --gid $module_group_id --disabled-login \
        --disabled-password $system_username || usermod -g $module_group_id $system_username
# Create the module user, or ensure that the existing user has the correct primary group
adduser --quiet --system --gecos "Ability Module" --no-create-home --gid $module_group_id --disabled-login \
        --disabled-password $module_username || usermod -g $module_group_id $module_username

system_user_id=$(id -u $system_username)
module_user_id=$(id -u $module_username)
docker_group_id=$(getent group docker | cut -d: -f3)

echo "Creating directories..."
install -d -o $system_user_id -m 750 $ABILITYDIR
install -d -o $system_user_id -m 750 $ABILITYDIR/conf
install -o $system_user_id -m 640 $VAGRANTHOME/ability/conf/edge.env $ABILITYDIR/conf
install -d -o $system_user_id -m 750 $ABILITYDIR/certs
install -o $system_user_id -m 640 $VAGRANTHOME/ability/certs/edgedevice-cert.pem $ABILITYDIR/certs
install -o $system_user_id -m 640 $VAGRANTHOME/ability/certs/edgedevice-key.pem $ABILITYDIR/certs
install -d -o $system_user_id -m 750 $MQTT_VOLUME
install -d -o $system_user_id -m 750 $MQTT_AUTH_VOLUME
install -d -o $system_user_id -m 750 $MODULES_VOLUME

echo "Logging into repository $ACRHOST as $ACRUSER..."
docker login -u $ACRUSER --password-stdin $ACRHOST <<< "$ACRPASSWORD"

if [ -z $EDGEPROXYIMAGE ];then
	echo "EDGEPROXYIMAGE is not defined in $ENVFILE"
	exit 1
fi

docker pull $EDGEPROXYIMAGE

HOST_IP=`ip -4 addr show scope global dev docker0 | grep inet | awk '{print $2}' | cut -d / -f 1`
echo "Initializing Swarm..."
docker swarm init --advertise-addr 127.0.0.1
echo "Creating Swarm Network $EDGENETWORK_INT..."
docker network create -d overlay $EDGENETWORK_INT
echo "Creating Swarm Network $EDGENETWORK_EXT..."
docker network create -d overlay $EDGENETWORK_EXT
echo "Starting Edge..."

echo $(uuidgen) | docker secret create edge-proxy -
docker service create --network $EDGENETWORK_INT --network $EDGENETWORK_EXT --name edge-proxy --secret edge-proxy -d=false --env-file $ENVFILE -e "system_user=$system_user_id:$module_group_id" -e "module_user=$module_user_id:$module_group_id" --replicas 1 --user="$system_user_id:$module_group_id" --group $docker_group_id \
        --host hostname:$HOST_IP --label system=true --label version=0 --mount type=bind,source=/var/run/docker.sock,destination=/var/run/docker.sock \
        --mount type=bind,source=$MODULES_VOLUME,destination=/files \
        --mount type=bind,source=$MQTT_AUTH_VOLUME,destination=$MQTT_AUTH_VOLUME \
		--mount type=bind,source=/var/ability/certs,destination=/var/ability/certs $EDGEPROXYIMAGE


