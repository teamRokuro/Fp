DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" >/dev/null 2>&1 && pwd )"
dotnet publish $DIR -c Release -f netcoreapp3.1 -r linux-x64 -o $DIR/publish -v m /p:IlcDisableReflection=true