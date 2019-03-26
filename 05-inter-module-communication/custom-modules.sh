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

    if [ -f ./$folder_name/deploy.sh ]; then
        echo "$folder_name already exists"
    else
        mkdir ./$folder_name

        em="abbability.azurecr.io/abb.ability.cst.edge.modules.image:latest"
        em_new="abbability.azurecr.io/abb.ability.cst.edge.modules.csharp.$custom_name.$myhost:latest"
		eam_new="abbability.azurecr.io/abb.ability.cst.edge.modules.csharp.aggregator.$custom_name.$myhost:latest"

        cat $template/01-abb.ability.configuration.edge.modules.csharp.aggregator.sample.json | sed s/[.]sample/.$custom_name/g | sed "s|$em|$eam_new|g" > ./$folder_name/01-abb.ability.configuration.edge.modules.csharp.aggregator.$custom_name.json
        cat $template/02-abb.ability.configuration.edge.modules.csharp.sample.json | sed s/[.]sample/.$custom_name/g | sed "s|$em|$em_new|g" > ./$folder_name/02-abb.ability.configuration.edge.modules.csharp.$custom_name.json
        cat $template/03-abb.ability.configuration.edge.sample.json | sed s/[.]sample/.$custom_name/g | sed "s|$eb|$eb_new|g" | sed "s|$ep|$ep_new|g" > ./$folder_name/03-abb.ability.configuration.edge.$custom_name.json
        cat $template/04-abb.ability.device.edge.modules.csharp.aggregator.sample.json | sed s/[.]sample/.$custom_name/g > ./$folder_name/04-abb.ability.device.edge.modules.csharp.aggregator.$custom_name.json
        cat $template/05-abb.ability.device.edge.modules.csharp.sample.json | sed s/[.]sample/.$custom_name/g > ./$folder_name/05-abb.ability.device.edge.modules.csharp.$custom_name.json
        cat $template/06-abb.ability.device.edge.sample.json | sed s/[.]sample/.$custom_name/g > ./$folder_name/06-abb.ability.device.edge.$custom_name.json

        cat ./Telemetry/docker-debug-sample.sh | sed s/[.]sample/.$custom_name.$myhost/g > ./Telemetry/docker-debug-$custom_name.sh
        chmod +x ./Telemetry/docker-debug-$custom_name.sh
        cat ./Telemetry/docker-sample.sh | sed s/[.]sample/.$custom_name.$myhost/g > ./Telemetry/docker-$custom_name.sh
        chmod +x ./Telemetry/docker-$custom_name.sh

        cat ./Aggregator/docker-debug-sample.sh | sed s/[.]sample/.$custom_name.$myhost/g > ./Aggregator/docker-debug-$custom_name.sh
        chmod +x ./Aggregator/docker-debug-$custom_name.sh
        cat ./Aggregator/docker-sample.sh | sed s/[.]sample/.$custom_name.$myhost/g > ./Aggregator/docker-$custom_name.sh
        chmod +x ./Aggregator/docker-$custom_name.sh
        
    fi
else
    echo "provide a custom name"
fi

echo "Done"
