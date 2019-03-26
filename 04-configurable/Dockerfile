FROM abbability.azurecr.io/edge/dotnet
ARG EXE_DIR=.
COPY ${EXE_DIR}/bin/Release/netcoreapp2.1/publish/* /cst-csharp-listen-config/
ENTRYPOINT ["dotnet", "/cst-csharp-listen-config/ABB.Ability.IotEdge.CST.Modules.CSharp.Configurable.dll"]