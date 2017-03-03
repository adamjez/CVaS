FROM microsoft/dotnet:1.0.3-runtime
ENTRYPOINT ["dotnet", "CVaS.AlgServer.dll"]
ARG source=.
WORKDIR /app
COPY $source .
ENV NETCORE_ENVIRONMENT=Development

