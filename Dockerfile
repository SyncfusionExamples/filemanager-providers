FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base

WORKDIR /app

ENV AZURE_ACCOUNT_NAME=""
ENV AZURE_ACCOUNT_KEY=""
ENV AZURE_BLOB_NAME=""
ENV AZURE_BLOB_PATH=""
ENV AZURE_FILE_PATH=""
ENV AWS_BUCKET_NAME=""
ENV AWS_ACCESS_KEY_ID=""
ENV AWS_SECRET_ACCESS_KEY=""
ENV AWS_BUCKET_REGION=""
ENV FILEMANAGER_PROVIDER=""

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /source
COPY ["src/Syncfusion.FileManager.Providers/Syncfusion.FileManager.Providers.csproj", "./src/Syncfusion.FileManager.Providers/"]
COPY ["src/Syncfusion.FileManager.Providers/NuGet.Config", "./src/Syncfusion.FileManager.Providers/"]
RUN dotnet restore "./src/Syncfusion.FileManager.Providers/Syncfusion.FileManager.Providers.csproj" -p:TargetFramework=net10.0
COPY . .
WORKDIR "/source/src"
RUN dotnet build "Syncfusion.FileManager.Providers/Syncfusion.FileManager.Providers.csproj" -c Release -f net10.0 --no-restore -o /app

FROM build AS publish
RUN dotnet publish "Syncfusion.FileManager.Providers/Syncfusion.FileManager.Providers.csproj" -c Release -f net10.0 --no-restore -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENV ASPNETCORE_URLS=http://+:80
EXPOSE 80
ENTRYPOINT ["dotnet", "Syncfusion.FileManager.Providers.dll"]
