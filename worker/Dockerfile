FROM microsoft/dotnet:1.1-sdk-msbuild

WORKDIR /app

ADD . /app

RUN dotnet restore \
    && dotnet publish -c Release -o out 

ENV AZURE_STORAGE_ACCOUNT account
ENV AZURE_STORAGE_ACCESS_KEY key

ENTRYPOINT ["dotnet", "out/worker.dll"]
