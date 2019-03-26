FROM abbability.azurecr.io/edge/dotnet
ARG EXE_DIR=.
COPY ${EXE_DIR}/bin/Release/netcoreapp2.1/publish/* /cst-csharp-remote-commands/
ENTRYPOINT ["dotnet", "/cst-csharp-remote-commands/ABB.Ability.IotEdge.CST.Modules.CSharp.RemoteCommands.dll"]