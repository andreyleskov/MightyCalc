# Generate api classes for C# from OpenAPI specification 
FROM mcr.microsoft.com/dotnet/core/sdk:2.2 as api-gen-env

RUN apt-get update 
RUN apt-get install curl -y
RUN apt-get install zip -y
RUN mkdir /usr/bin/MightyCalc 
WORKDIR /usr/bin/MightyCalc
#download latest Swag toolchain
RUN curl -L http://rsuter.com/Projects/NSwagStudio/archive.php --output ./NSwag.zip
RUN unzip ./NSwag.zip -d NSwag > /dev/null
RUN rm ./NSwag.zip > /dev/null

COPY . .
#Generating API controller
RUN dotnet ./NSwag/NetCore22/dotnet-nswag.dll swagger2cscontroller /input:"./MightyCalcAPI.yaml" /classname:Api /namespace:MightyCalc.API /UseLiquidTemplates:true /ControllerBaseClass:Microsoft.AspNetCore.Mvc.ControllerBase /AspNetNamespace:"Microsoft.AspNetCore.Mvc" /output:"/MightyCalc.API/MightyCalc.API/Controllers/MightyCalcController.cs" /ResponseArrayType:"System.Collections.Generic.IReadOnlyCollection"  /ArrayBaseType:"System.Collections.Generic.IReadonlyCollection" /ArrayInstanceType:"System.Collections.Generic.List"
#Generating API client in C#
RUN dotnet ./NSwag/NetCore22/dotnet-nswag.dll swagger2csclient /input:"./MightyCalcAPI.yaml" /classname:MightyCalcClient /namespace:MightyCalc.Client /UseLiquidTemplates:true /output:"/MightyCalc.API/MightyCalc.Client/Client.cs" /GenerateClientInterfaces:true /InjectHttpClient:true /generateDataAnnotations:false /exceptionClass:MightyCalcException /generateOptionalParameters:true /ArrayBaseType:"System.Collections.Generic.IReadonlyCollection" /responseArrayType:"System.Collections.Generic.IReadOnlyCollection" /arrayType:"System.Collections.Generic.IReadOnlyCollection"
RUN rm -rf ./NSwag

#build the project and get binaries for production 
FROM microsoft/dotnet:3.0-sdk as build-env

COPY --from=api-gen-env /usr/bin/MightyCalc /usr/bin/MightyCalc
WORKDIR /usr/bin/MightyCalc
RUN dotnet build -c Release -v quite
RUN mkdir -p /swagger
RUN cp ./MightyCalcAPI.yaml /swagger/MightyCalcAPI.yaml #for tests
COPY ./integration_tests.sh ./integration_tests.sh
RUN chmod +x ./integration_tests.sh
ENTRYPOINT [ "bash" ]
CMD ["./integration_tests.sh"]

FROM microsoft/dotnet:3.0-sdk as publish-env
WORKDIR /usr/bin/MightyCalc
RUN ls
RUN dotnet publish ./MightyCalc.API/MightyCalc.API.csproj -c Release --no-build -o publish
FROM microsoft/dotnet:3.0-aspnetcore-runtime as runtime
COPY --from=publish-env /usr/bin/MightyCalc/publish /usr/bin/MightyCalc  
WORKDIR /usr/bin/MightyCalc
EXPOSE 80
ENTRYPOINT ["dotnet", "MightyCalc.API.dll"]