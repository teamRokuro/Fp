#!/bin/bash
DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" >/dev/null 2>&1 && pwd )"
dotnet publish "$DIR" -f net5.0 -c Release -r linux-x64 -o "$DIR/publish" -v m /p:IlcGenerateCompleteTypeMetadata=false /p:IlcGenerateStackTraceData=false /p:IlcOptimizationPreference=Speed