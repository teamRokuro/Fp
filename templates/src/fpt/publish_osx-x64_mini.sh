#!/bin/bash
DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" >/dev/null 2>&1 && pwd )"
dotnet publish "$DIR" -f netcoreapp3.1 -c Release -r osx-x64 -o "$DIR/publish" -v m /p:IlcDisableReflection=true /p:IlcInvariantGlobalization=true /p:RootAllApplicationAssemblies=false /p:IlcGenerateCompleteTypeMetadata=false /p:IlcGenerateStackTraceData=false /p:IlcOptimizationPreference=Speed
strip $DIR/publish/${DIR##*/}