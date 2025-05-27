#!/usr/bin/env bash
set -e

## install latest .NET 9.0 release
pushd /tmp
wget https://dotnet.microsoft.com/download/dotnet/scripts/v1/dotnet-install.sh
chmod u+x /tmp/dotnet-install.sh
/tmp/dotnet-install.sh --channel 9.0
popd

## Install wasm-tools
dotnet workload install wasm-tools

## Build
dotnet build

## Install playwright
pwsh AudioCuesheetEditor.End2EndTests/bin/Debug/net9.0/playwright.ps1 install --with-deps

## Start Server for tests
dotnet run --project AudioCuesheetEditor &

SERVER_PID=$!

## Wait for server to start
echo "Waiting for server to start..."
for i in {1..30}; do
    if curl -s http://localhost:5132 > /dev/null; then
        echo "Server is up!"
        break
    fi
    sleep 1
done

## Run Unit and End2End Tests
dotnet test

# Server beenden
kill $SERVER_PID

## publish project to known location for subsequent deployment by Netlify
dotnet publish -c Release -o release