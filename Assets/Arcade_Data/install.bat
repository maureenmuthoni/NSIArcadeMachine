@echo off
set dataPath=%1
set zipPath=%2
set exeToTerminate=%3
set exeToRun=%4
taskkill /F /IM %exeToTerminate%
PowerShell Expand-Archive -Path "%zipPath%" -DestinationPath "%dataPath%" -Force
start %exeToRun%