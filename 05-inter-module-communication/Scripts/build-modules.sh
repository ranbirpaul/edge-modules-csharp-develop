#!/bin/bash

#if [ -f ./ABB.Ability.IoTEdge.ReferenceModule/Services/DeviceService.cs.new ]; then
#    echo "updating DeviceService.cs to have the new devie type"
#    cp  -f ./ABB.Ability.IoTEdge.ReferenceModule/Services/DeviceService.cs.new ./ABB.Ability.IoTEdge.ReferenceModule/Services/DeviceService.cs
#fi

./ABB.Ability.IotEdge.CST.Sample/build-module.sh

./ABB.Ability.IotEdge.CST.Aggregator.Sample/build-module.sh
