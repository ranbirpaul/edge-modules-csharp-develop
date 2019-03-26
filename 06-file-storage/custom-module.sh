#!/bin/bash

ep="abbability.azurecr.io/edge/proxy-develop:latest"
eb="abbability.azurecr.io/edge/broker-develop:latest"

ep_new="abbability.azurecr.io/edge/proxy:2.0.0"
eb_new="abbability.azurecr.io/edge/broker:1.2.0"

if [ $# -ge 1 ]; then
    template=./template
    custom_name=$( echo $1 | tr '[:upper:]' '[:lower:]' )
    folder_name="types"$custom_name
    myhost=$(hostname)
    myhost=$( echo $myhost | tr '[:upper:]' '[:lower:]' )

    echo "custom=$custom_name"
    echo "folder=$folder_name"
    echo "host=$myhost"

    # comment this out in final version
    # rm -rf ./$folder_name

    if [ -f ./$folder_name/deploy.sh ]; then
        echo "$folder_name already exists"
    else
        mkdir ./$folder_name

        em="abbability.azurecr.io/abb.ability.cst.edge.modules.image:latest"
        em_new="abbability.azurecr.io/abb.ability.cst.edge.modules.csharp.$custom_name.$myhost:latest"

    
        cat $template/01-abb.ability.configuration.edge.modules.csharp.sample.json | sed s/[.]sample/.$custom_name/g | sed "s|$em|$em_new|g" > ./$folder_name/01-abb.ability.configuration.edge.modules.csharp.$custom_name.json
        cat $template/02-abb.ability.configuration.edge.sample.json | sed s/[.]sample/.$custom_name/g | sed "s|$eb|$eb_new|g" | sed "s|$ep|$ep_new|g" > ./$folder_name/02-abb.ability.configuration.edge.$custom_name.json
        cat $template/03-abb.ability.device.edge.modules.csharp.sample.json | sed s/[.]sample/.$custom_name/g > ./$folder_name/03-abb.ability.device.edge.modules.csharp.$custom_name.json
        cat $template/04-abb.ability.device.edge.sample.json | sed s/[.]sample/.$custom_name/g > ./$folder_name/04-abb.ability.device.edge.$custom_name.json
        cp $template/deploy.sh ./$folder_name/

        cat ./docker-debug-sample.sh | sed s/[.]sample/.$custom_name.$myhost/g > ./docker-debug-$custom_name.sh
        chmod +x ./docker-debug-$custom_name.sh
        cat ./docker-sample.sh | sed s/[.]sample/.$custom_name.$myhost/g > ./docker-$custom_name.sh
        chmod +x ./docker-$custom_name.sh
        
        cat $template/edge.env | sed s/[.]sample/.$custom_name/g | sed "s|E_BROKER_REPLACE|$eb_new|g" | sed "s|E_PROXY_REPLACE|$ep_new|g" > ./$folder_name/edge.env

#	./build-module.sh
    fi
else
    echo "provide a custom name"
fi

echo "Done"
