@echo off
@setlocal
pushd
set ERROR_CODE=0
dotnet build src\Modzy.CLI\Modzy.CLI.csproj %*

:end
popd
exit /B %ERROR_CODE%