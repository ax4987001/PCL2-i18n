@echo off
chcp 65001 >nul
title PCL2-i18n 本地构建脚本

echo =========================================
echo       PCL2-i18n 本地构建脚本
echo =========================================
echo.

echo [1/2] 正在清理旧的构建输出目录 (build)...
if exist "build" (
    rmdir /s /q "build"
)

echo.
echo [2/2] 正在还原依赖并编译 Release 版本...
dotnet build "Plain Craft Launcher 2.sln" -c Release -p:Platform="Any CPU"

echo.
if %ERRORLEVEL% equ 0 (
    echo =========================================
    echo 构建成功！
    echo 可执行文件位于: %~dp0build\Plain Craft Launcher 2.exe
    echo =========================================
) else (
    echo =========================================
    echo 构建失败！请检查上方输出的错误信息。
    echo =========================================
)

echo.
pause
