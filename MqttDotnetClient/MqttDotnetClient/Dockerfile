FROM microsoft/dotnet:2.2-sdk AS build
WORKDIR /src

COPY . .
WORKDIR /src/MqttDotnetClient
RUN dotnet publish MqttDotnetClient.csproj -c Release -o /app

FROM microsoft/dotnet:2.2-aspnetcore-runtime AS final

ENV RUNNING_IN_CONTAINER=true

COPY ./Certificates/*.pem /etc/ssl/certs/
RUN update-ca-certificates

WORKDIR /app
COPY --from=build /app .
ENTRYPOINT ["dotnet", "MqttDotnetClient.dll"]
