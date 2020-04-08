dotnet publish %~dp0 -c Release -f netcoreapp3.1 -r win-x86 -o %~dp0\publish -v m /p:IlcOptimizationPreference=Speed
/p:IlcDisableReflection=true