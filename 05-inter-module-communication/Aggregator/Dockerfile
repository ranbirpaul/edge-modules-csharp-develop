FROM abbability.azurecr.io/edge/dotnet
ARG EXE_DIR=.
COPY ${EXE_DIR}/bin/Release/netcoreapp2.1/publish/* /cst-csharp-aggregator/
ENTRYPOINT ["dotnet", "/cst-csharp-aggregator/ABB.Ability.IotEdge.CST.Modules.CSharp.InterModuleCommunication.Aggregator.dll"]
