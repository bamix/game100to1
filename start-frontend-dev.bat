@echo off
echo ========================================
echo    Запуск ТОЛЬКО фронтенда (разработка)
echo ========================================
echo.

cd Frontend

echo Установка зависимостей...
if exist node_modules (
    echo Node modules уже установлены
) else (
    npm install
)

echo.
echo Запуск dev сервера...
echo.
echo Фронтенд будет доступен по адресу:
echo http://localhost:5173
echo.
echo API проксируется на http://localhost:5000
echo (Убедитесь, что бэкенд запущен отдельно!)
echo.

npm run dev