#!/bin/bash

envsubst < appsettings.Docker.json > appsettings.json
rm appsettings.Docker.json

echo "Here's the appsettings.json we're using inside Docker ..."
cat appsettings.json

dotnet IdentityServer.dll