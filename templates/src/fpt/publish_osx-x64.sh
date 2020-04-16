#!/bin/bash
DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" >/dev/null 2>&1 && pwd )"
dotnet publish "$DIR" -f netcoreapp3.1 -c Release -r osx-x64 -o "$DIR/publish" -v m /p:IlcGenerateCompleteTypeMetadata=false /p:IlcGenerateStackTraceData=false /p:IlcOptimizationPreference=Speed