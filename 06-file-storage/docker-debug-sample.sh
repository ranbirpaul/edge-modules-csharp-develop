#!/bin/bash

docker build . -t abb.ability.cst.edge.modules.csharp.sample:latest -f Dockerfile.debug
docker tag abb.ability.cst.edge.modules.csharp.sample:latest abbability.azurecr.io/abb.ability.cst.edge.modules.csharp.sample:latest

# rem sudo docker login abbability.azurecr.io
# IMAGEREGISTRIES={"abb":{"serveraddress":"https://abbability.azurecr.io","username":"abbability","password":"+zoc=m/j+wip2ikYoAen/I7nr0O+WbVS"}}

docker login -u abbability --password-stdin https://abbability.azurecr.io <<< "+zoc=m/j+wip2ikYoAen/I7nr0O+WbVS"

# rem sudo docker images

docker push abbability.azurecr.io/abb.ability.cst.edge.modules.csharp.sample
