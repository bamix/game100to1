@echo off
echo ======================================
echo    Запуск ТОЛЬКО бэкенда (разработка)
echo ======================================
echo.

cd Backend\Game100To1.Backend

echo Установка зависимостей...
dotnet restore

echo.
echo Запуск API сервера...
echo.
echo Бэкенд будет доступен по адресу:
echo http://localhost:5000
echo.
echo Swagger UI (документация API):
echo http://localhost:5000/swagger
echo.
echo API endpoints:
echo - GET  /api/game/state
echo - POST /api/game/start
echo - POST /api/game/start-round
echo - и другие...
echo.
echo SignalR Hub: /gamehub
echo.

dotnet run