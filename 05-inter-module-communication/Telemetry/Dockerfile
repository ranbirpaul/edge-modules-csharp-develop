FROM abbability.azurecr.io/edge/dotnet
ARG EXE_DIR=.
COPY ${EXE_DIR}/bin/Release/netcoreapp2.1/publish/* /cst-csharp-telemetry/
ENTRYPOINT ["dotnet", "/cst-csharp-telemetry/ABB.Ability.IotEdge.CST.Modules.CSharp.InterModuleCommunication.Telemetry.dll"]