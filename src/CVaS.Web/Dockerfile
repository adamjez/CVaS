FROM microsoft/aspnetcore:1.0.3
ENTRYPOINT ["dotnet", "CVaS.Web.dll"]
ARG source=.
WORKDIR /app
EXPOSE 80
COPY $source .
