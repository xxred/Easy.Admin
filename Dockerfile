FROM mcr.microsoft.com/dotnet/core/sdk:3.1-stretch
RUN ln -sf /usr/share/zoneinfo/Asia/Shanghai /etc/localtime && echo 'Asia/Shanghai' >/etc/timezone # 设置时区
ARG AppKey
ARG Source
ARG ProjName=Easy.Admin
WORKDIR /src
COPY ["${ProjName}/${ProjName}.csproj", "${ProjName}/"]
RUN dotnet restore "${ProjName}/${ProjName}.csproj"
COPY . .
WORKDIR /src/${ProjName}
RUN dotnet pack "${ProjName}.csproj" -c Release -o /app

WORKDIR /app
RUN dotnet nuget push *.nupkg -k ${AppKey} -s ${Source}
