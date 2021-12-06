@echo off
@setlocal
pushd
set ERROR_CODE=0
src\Modzy.CLI\bin\Debug\net6.0\Modzy.CLI.exe %*

:end
popd
exit /B %ERROR_CODE%