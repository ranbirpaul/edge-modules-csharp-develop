#!/bin/bash

source files/common.sh

if [ $# -ne 1 ]; then
    if [ "$DEF_RES_GROUP" == "none" ]; then

        echo "Usage: `basename $0` <instance_name>"
        echo "      -OR- set DEF_RES_GROUP in files/common.sh"
        echo "Usage: `basename $0` "
    
        exit 10
    else
	echo "Using DEF_RES_GROUP: $DEF_RES_GROUP"
        ./prepare.sh "$DEF_RES_GROUP"
	code=$?
    fi
else
    ./prepare.sh "$1"
    code=$?
fi

if [ $code -ne 0 ]; then
   fatal "Prepare step failed. Aborting."
fi

cd files
sudo -H ./bootstrap.sh ability/conf/edge.env
