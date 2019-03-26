FROM abbability.azurecr.io/edge/dotnet
ARG EXE_DIR=.
COPY ${EXE_DIR}/bin/Release/netcoreapp2.1/publish/* /cst-csharp-sendtelemetry/
ENTRYPOINT ["dotnet", "/cst-csharp-sendtelemetry/ABB.Ability.IotEdge.CST.Modules.CSharp.SendTelemetry.dll"]