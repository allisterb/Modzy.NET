@echo off
@setlocal
set ERROR_CODE=0
bin\Debug\net6.0\Modzy.CLI.exe %*

:end
exit /B %ERROR_CODE%