# Ability Edge modules samples

## Type Definitions

This directory contains type definitions in sample C# projects across this repository. Mind that some types are available in multiple versions, which can be observed by looking at the files' names. Look at the README of the tutorial you are following to know which version of a given type definition to use.

# Finding Type Definition Registry URL

Before you proceed, you have to find your Type Definition Registry URL using an [Azure Portal](https://portal.azure.com) (look for a web application with name starting with `AbiTyDefReg`) or using AZ CLI as follows:
```bash
ABILITY_RESOURCE_GROUP_NAME="Ability-Platform-XXXX-Tst-rg"
az webapp list -o table -g $ABILITY_RESOURCE_GROUP_NAME | grep "AbiTyDefReg"
```

# Uploading to Type Definition Registry

To upload a type definition to the registry and make it available for your application you can use any HTTP client (like curl, wget, Postman etc.) that lets you issue a `POST` request.

## Using CURL

```bash
# Put Type definition registry URL in the environment variable
ABILITY_TYPE_DEFINITION_REGISTRY_URL="https://abitydefregxxxxxxxx.azurewebsites.net"
# File name where the type is defined
FILE_NAME="/path/to/typedefinitionfile.json"
# Model definition used for this type (use the one defined in your "model" field)
MODEL="abb.ability.device"

# Read the content of the file into $BODY variable
BODY=`cat $FILE_NAME`
# Make a POST call to Type Definition Registry
curl -X POST "$ABILITY_TYPE_DEFINITION_REGISTRY_URL/api/v1/ModelDefinitions/$MODEL/types" -H "accept: application/json" -H "Content-Type: application/json-patch+json" -d "$BODY"
 ```