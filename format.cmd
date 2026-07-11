@echo off
echo Running dotnet format...
dotnet format --no-restore --severity info
if %ERRORLEVEL% NEQ 0 (
    echo Formatting issues found and fixed.
) else (
    echo No formatting issues found.
)
