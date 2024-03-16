ARG SDK_VERSION=8.0
FROM mcr.microsoft.com/dotnet/sdk:${SDK_VERSION} AS build
ARG PUBLISH_CONFIGURATION=Release
ARG PUBLISH_FRAMEWORK=net8.0
WORKDIR /app
COPY . ./

RUN dotnet publish ./examples/MicroserviceExample/WorkerService -c "${PUBLISH_CONFIGURATION}" -f "${PUBLISH_FRAMEWORK}" -o /out -p:IntegrationBuild=true

FROM mcr.microsoft.com/dotnet/aspnet:${SDK_VERSION} AS runtime
WORKDIR /app
COPY --from=build /out ./

COPY ./examples/MicroserviceExample/WorkerService/entrypoint.sh .

RUN apt-get -y update
RUN apt-get -y install curl

RUN apt-get -y install zip

RUN curl -sSfL https://github.com/signalfx/splunk-otel-dotnet/releases/latest/download/splunk-otel-dotnet-install.sh -O

# Install the distribution
RUN sh ./splunk-otel-dotnet-install.sh

ENTRYPOINT ["sh", "/app/entrypoint.sh"]

CMD ["dotnet", "WorkerService.dll"]

