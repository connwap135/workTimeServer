#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/runtime:3.1-buster-slim AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["TimeWorkerService/TimeWorkerService.csproj", "TimeWorkerService/"]
RUN dotnet restore "TimeWorkerService/TimeWorkerService.csproj"
COPY . .
WORKDIR "/src/TimeWorkerService"
RUN dotnet build "TimeWorkerService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TimeWorkerService.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
#ENTRYPOINT ["dotnet", "TimeWorkerService.dll"]


EXPOSE 39169/udp

CMD ["/bin/bash","-c","export PYTHONPATH=/app/publish &&  dotnet TimeWorkerService.dll"]