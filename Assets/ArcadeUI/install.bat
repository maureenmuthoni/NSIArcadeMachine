@echo off
set dataPath=%1
set zipPath=%2
set exeToTerminate=%3
taskkill /F /IM %exeToTerminate%
PowerShell Expand-Archive -Path "%zipPath%" -DestinationPath "%dataPath%" -Force
start %exeToTerminate%
pause