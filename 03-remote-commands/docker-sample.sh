#!/bin/bash

sudo docker build . -t abb.ability.cst.edge.modules.sample:latest -f Dockerfile
sudo docker tag abb.ability.cst.edge.modules.csharp.sample:latest abbability.azurecr.io/abb.ability.cst.edge.modules.csharp.sample:latest
docker login -u abbability --password-stdin https://abbability.azurecr.io <<< "+zoc=m/j+wip2ikYoAen/I7nr0O+WbVS"
sudo docker push abbability.azurecr.io/abb.ability.cst.edge.modules.csharp.sample
