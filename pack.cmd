@echo off
set BUILD_CFG=%1
if "%BUILD_CFG%" equ "" set BUILD_CFG=Debug
if exist "package-output" del "package-output" /f /q
for /d %%D in (src\FubarCoder.RestSharp.Portable*) do (
	dotnet pack %%D -c %BUILD_CFG% -o "package-output" --no-build
	if %errorlevel% neq  0 exit /b %errorlevel%
)
