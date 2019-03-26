FROM abbability.azurecr.io/edge/dotnet
# If you need to use a proxy to connect to the internet, put it uncomment the line below and put your proxy server
# ENV http_proxy http://access505.cws.sco.cisco.com:8080/
ARG EXE_DIR=.
COPY ${EXE_DIR}/bin/Release/netcoreapp2.1/publish/* /cst-csharp-files-upload/
ENTRYPOINT ["dotnet", "/cst-csharp-files-upload/ABB.Ability.IotEdge.CST.Modules.CSharp.FileStorage.dll"]