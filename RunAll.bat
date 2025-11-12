@echo off
chcp 65001 >nul
echo ==============================
echo 🚀 Ejecutando procesos en paralelo
echo ==============================
start "WebApi" /MAX dotnet run --project "WebApi" --configuration Release
start "WebApp" /MAX dotnet run --project "WebApp" --configuration Release
echo ==============================
echo ✅ Todos los procesos fueron iniciados.
echo ==============================
pause