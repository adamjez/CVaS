#!/bin/bash
cwd=$(pwd)
dir="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"
web="$dir/../src/CVaS.Web"
dal="$dir/../src/CVaS.DAL"

# We have to change directory for dotnet ef to be available
cd $web
echo "dotnet ef --project $dal migrations add $1"
dotnet ef --project $dal migrations add $1