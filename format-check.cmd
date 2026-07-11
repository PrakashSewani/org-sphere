@echo off
echo Checking formatting...
dotnet format --no-restore --severity info --verify-no-changes
if %ERRORLEVEL% NEQ 0 (
    echo Formatting issues found. Run format.cmd to fix.
    exit /b 1
)
echo Format check passed.
