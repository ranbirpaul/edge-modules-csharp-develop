#!/bin/bash

OS_WIN=0
OS_LINUX=1

# Tools path
AZ=az
CURL=curl
UUIDGEN=uuidgen
JQ=jq
VAGRANT=vagrant
NODE=node
NPM=npm
SED=sed
DEF_RES_GROUP=none

# Dirs path
BASE_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )/.." && pwd )"
TYPES_DIR="$BASE_DIR/types"
WIN_DIR="$BASE_DIR/windows-helpers"

# Colors
GREEN='\033[0;32m'                                                                                                                          RED='\033[0;31m'                                                                                                                            WARN='\033[0;93m'                                                                                                                           NC='\033[0m' # No Color

if [ "$(uname -o)" == "Msys" ]; then
  OS_WIN=1
  OS_LINUX=0
  AZ=az.cmd
else
  OS_WIN=0
  OS_LINUX=1
fi

if [ $OS_WIN ]; then
  PATH="$PATH:$WIN_DIR"
fi

function getRegistryUrl()
{
  getPlatformUrl "$1" "AbiTyDefReg"
}

function getPlatformUrl()
{
  rgName=$1
  grep=$2
  name=$($AZ webapp list -g "${rgName}" -o json | $JQ -r ".[] | select(.name | contains(\"$grep\")).defaultHostName")
  echo $name
}

function error()
{
  echo -e $RED$@$NC
}

function fatal() 
{
  error $@
  exit 1
}

function warning()
{
  echo -e $WARN$@$NC
}

function success()
{
  echo -e $GREEN$@$NC
}

