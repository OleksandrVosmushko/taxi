# build runtime image
FROM microsoft/aspnetcore:2.0.9
WORKDIR /app
COPY bin/Debug/netcoreapp2.0/publish .
COPY cre
ENTRYPOINT ["dotnet", "Taxi.dll"]