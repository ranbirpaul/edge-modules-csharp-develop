#!/bin/bash

docker build . -t abb.ability.cst.edge.modules.csharp.aggregator.sample:latest -f Dockerfile
docker tag abb.ability.cst.edge.modules.csharp.aggregator.sample:latest abbability.azurecr.io/abb.ability.cst.edge.modules.csharp.aggregator.sample:latest
docker login -u abbability --password-stdin https://abbability.azurecr.io <<< "+zoc=m/j+wip2ikYoAen/I7nr0O+WbVS"
docker push abbability.azurecr.io/abb.ability.cst.edge.modules.csharp.aggregator.sample