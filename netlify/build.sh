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

## Run Unit Test
dotnet test

## publish project to known location for subsequent deployment by Netlify
dotnet publish -c Release -o release