@echo off
cd %~dp0

if exist static-site rd /s /q static-site
md static-site

xcopy wwwroot static-site\notebooks\ /e /q && dotnet run build-static-site
