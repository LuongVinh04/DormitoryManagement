@echo off
echo Starting Backend and Frontend...

:: Start Backend
echo Starting Backend...
start "Backend - .NET" cmd /c "cd /d %~dp0backend\DormitoryManagement && dotnet run --urls http://127.0.0.1:5101"

:: Start Frontend
echo Starting Frontend...
start "Frontend - Vite" cmd /c "cd /d %~dp0frontend && npm run dev"

echo Both services have been started in new windows.
