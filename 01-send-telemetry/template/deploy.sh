#!/bin/bash


# END OF CONFIGURATION. DO NOT CHANGE ANYTHING BELOW THIS LINE
source ../../files/common.sh
ERRCOUNT=0
OKCOUNT=0

function upload() 
{
    filename=$1
    model=$(cat $filename | $JQ -r '.model')

    echo "Uploading ${model} from file ${filename}..."
    body=$(cat "$filename")
    resp=$($CURL -s -X POST "${TRURL}/api/v1.0/ModelDefinitions/${model}/types" -H "accept: application/json" -H "Content-Type: application/json-patch+json" -d "${body}")
    echo $resp
    error=$(echo ${resp} | $JQ -r '.error // ""')

    if [ -z "${error}" ]; then
        success "Upload OK"
        let "OKCOUNT++"
    elif [[ "${error}" = "Entity already exists in database" ]]; then
    	warning "File $filename already uploaded, skipping."
	let "OKCOUNT++"
    else
        error "Upload Failed: ${error}"
        let "ERRCOUNT++"
    fi

}

function uploadBatch()
{
    fileList="${1}"
    for fileName in ${fileList}; do
      upload "$fileName"
    done
}

function deploy() 
{
    regex="${1}"
    fileList=$(ls ${PATTERN} 2>/dev/null)
    count=$(echo "$fileList" |wc -w)
    if [ $count -eq 0 ]; then
      fatal "Error: no files found using pattern ${PATTERN}"
    fi
    echo "Uploading ${count} files to $TRURL..."
    uploadBatch "${fileList}"
}

if [ "$1" == "-g" ] && [ $# -ge 2 ]; then
   PATTERN=${3:-*.json}
   TRURL=https://$(getRegistryUrl $2)
   echo "Registry URL: $TRURL"
else
    if [ "$1" == "-d" ] && [ "$DEF_RES_GROUP" != "none" ]; then
        echo "USING DEFAULT RG: $DEF_RES_GROUP"
        TRURL=https://$(getRegistryUrl $DEF_RES_GROUP)
        echo "Registry URL: $TRURL"
    else
    TRURL=$1
    fi
    PATTERN=${2:-*.json}
fi

if [ -z "$TRURL" ]; then
   error "Error: type registry URL not specified"
   echo "Usage:"
   echo "  `basename $0` <typeRegistryUrl | -g resourceGroup> [pattern] | -d ( use DEF_RES_GROUP from common.sh)"
   echo "    typeRegistryUrl  - an URL to the Ability Type Registry service"
   echo "    resourceGroup    - Azure resource group name"
   echo "    pattern          - file search pattern (default: *.json)"
   echo ""
   echo "  example: `basename $0` https://abitydefregxxxxxxxxx.azurewebsites.net"
   echo ""
   exit 1
fi

deploy "*"


echo "Finished. Success: ${OKCOUNT}, Failed: ${ERRCOUNT}"

echo "Copy edge.env to files/ability/conf..."
cp -f edge.env ../../files/ability/conf

