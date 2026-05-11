FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src
COPY ["src/BelKhidmah.Web.Host/BelKhidmah.Web.Host.csproj", "src/BelKhidmah.Web.Host/"]
COPY ["src/BelKhidmah.Web.Core/BelKhidmah.Web.Core.csproj", "src/BelKhidmah.Web.Core/"]
COPY ["src/BelKhidmah.Application/BelKhidmah.Application.csproj", "src/BelKhidmah.Application/"]
COPY ["src/BelKhidmah.Core/BelKhidmah.Core.csproj", "src/BelKhidmah.Core/"]
COPY ["src/BelKhidmah.EntityFrameworkCore/BelKhidmah.EntityFrameworkCore.csproj", "src/BelKhidmah.EntityFrameworkCore/"]
WORKDIR "/src/src/BelKhidmah.Web.Host"
RUN dotnet restore 

WORKDIR /src
COPY ["src/BelKhidmah.Web.Host", "src/BelKhidmah.Web.Host"]
COPY ["src/BelKhidmah.Web.Core", "src/BelKhidmah.Web.Core"]
COPY ["src/BelKhidmah.Application", "src/BelKhidmah.Application"]
COPY ["src/BelKhidmah.Core", "src/BelKhidmah.Core"]
COPY ["src/BelKhidmah.EntityFrameworkCore", "src/BelKhidmah.EntityFrameworkCore"]
WORKDIR "/src/src/BelKhidmah.Web.Host"
RUN dotnet publish -c Release -o /publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0
EXPOSE 80
WORKDIR /app
COPY --from=build /publish .
ENTRYPOINT ["dotnet", "BelKhidmah.Web.Host.dll"]
