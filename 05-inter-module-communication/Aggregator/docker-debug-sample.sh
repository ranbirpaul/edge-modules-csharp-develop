#!/bin/bash

# If building images within the ABB firewall, the country-specific proxy addresses need to be specified in the line below
# Example: docker build . -t abb.ability.cst.edge.modules.csharp.sample:latest -f Dockerfile.debug --build-arg http_proxy=http://access401.cws.sco.cisco.com:8080/ --build-arg ftp-proxy=http://access401.cws.sco.cisco.com:8080/
docker build . -t abb.ability.cst.edge.modules.csharp.aggregator.sample:latest -f Dockerfile.debug
docker tag abb.ability.cst.edge.modules.csharp.aggregator.sample:latest abbability.azurecr.io/abb.ability.cst.edge.modules.csharp.aggregator.sample:latest
docker login -u abbability --password-stdin https://abbability.azurecr.io <<< "+zoc=m/j+wip2ikYoAen/I7nr0O+WbVS"
docker push abbability.azurecr.io/abb.ability.cst.edge.modules.csharp.aggregator.sample