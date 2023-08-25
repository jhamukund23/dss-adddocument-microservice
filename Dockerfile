FROM humana-idm-dss-docker-virtual.jfrog.io/dotnet/runtime:6.0
COPY App/ App/

# Add Humana Root and CA certs to container image...
COPY certs/ca /usr/local/share/ca-certificates/ca
COPY certs/root /usr/local/share/ca-certificates/root
RUN update-ca-certificates

WORKDIR /App
ENTRYPOINT ["dotnet", "dss-adddocument-microservice.dll"]
