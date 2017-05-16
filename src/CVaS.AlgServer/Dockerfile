FROM microsoft/dotnet:1.1.2-runtime
ENTRYPOINT ["dotnet", "CVaS.AlgServer.dll"]
ARG source=.
WORKDIR /app
COPY $source .
ENV NETCORE_ENVIRONMENT=Development

