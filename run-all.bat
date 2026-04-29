@echo off
setlocal

set ROOT=%~dp0
set BACKEND_PORT=5101
set FRONTEND_PORT=5173

echo ==================================================
echo Dormitory Management - Run all
echo Root: %ROOT%
echo ==================================================

echo.
echo Freeing development ports if they are already in use...
powershell -NoProfile -ExecutionPolicy Bypass -Command ^
  "$ports=@(%BACKEND_PORT%,%FRONTEND_PORT%); foreach($port in $ports){ Get-NetTCPConnection -LocalPort $port -State Listen -ErrorAction SilentlyContinue | ForEach-Object { $owner=$_.OwningProcess; $proc=Get-Process -Id $owner -ErrorAction SilentlyContinue; if($proc){ Write-Host ('Stopping PID {0} ({1}) on port {2}' -f $owner,$proc.ProcessName,$port); Stop-Process -Id $owner -Force } } }"

echo.
echo Starting backend on http://127.0.0.1:%BACKEND_PORT% ...
start "Backend - .NET" cmd /k "cd /d ""%ROOT%backend\DormitoryManagement"" && dotnet run --urls http://127.0.0.1:%BACKEND_PORT%"

echo Starting frontend on http://127.0.0.1:%FRONTEND_PORT% ...
start "Frontend - Vite" cmd /k "cd /d ""%ROOT%frontend"" && npm run dev -- --host 127.0.0.1 --port %FRONTEND_PORT%"

echo.
echo Both services are starting in separate windows.
echo If a window reports an error, it will stay open so you can read the log.
echo Frontend: http://127.0.0.1:%FRONTEND_PORT%
echo Backend:  http://127.0.0.1:%BACKEND_PORT%
echo.
pause
