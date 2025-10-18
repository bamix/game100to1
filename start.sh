#!/bin/bash

echo "================================="
echo "    Запуск игры \"100 к 1\""
echo "================================="
echo

echo "Переход в папку бэкенда..."
cd Backend/Game100To1.Backend

echo "Установка зависимостей .NET..."
dotnet restore

echo
echo "Переход в папку фронтенда..."
cd ../../Frontend

echo "Установка зависимостей Node.js..."
if [ -d "node_modules" ]; then
    echo "Node modules уже установлены"
else
    npm install
fi

echo
echo "Сборка фронтенда..."
npm run build

echo
echo "Переход в папку бэкенда для запуска..."
cd ../Backend/Game100To1.Backend

echo
echo "Запуск сервера..."
echo
echo "Сервер будет доступен по адресу:"
echo "http://localhost:5000"
echo
echo "Страницы:"
echo "- Главная: http://localhost:5000"
echo "- Экран игроков: http://localhost:5000/#player"
echo "- Админка: http://localhost:5000/#admin"
echo "- Swagger API: http://localhost:5000/swagger"
echo
echo "Для остановки нажмите Ctrl+C"
echo

dotnet run