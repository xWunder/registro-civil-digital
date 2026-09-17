@echo off
setlocal
cd /d "%~dp0"
for /f "delims=" %%B in ('git branch --show-current') do set "entregaBranch=%%B"
if /I not "%entregaBranch%"=="main" (
    echo Debe estar en main. No se cambiara de rama automaticamente.
    exit /b 1
)
dotnet build RegistroCivil.Web.slnx
if errorlevel 1 exit /b 1
dotnet run --project RegistroCivil.Verificacion --no-build
if errorlevel 1 exit /b 1
git add .gitignore RegistroCivil.Web.slnx README.md PublicarIntegracion.cmd Documentacion/EntregaParaMoreno.md Documentacion/VerificacionIntegracion.md RegistroCivil.Verificacion RegistroCivil.Web/Components RegistroCivil.Web/Database RegistroCivil.Web/FileStorage RegistroCivil.Web/Models/ActaNacimiento.cs RegistroCivil.Web/Services/ActaNacimientoService.cs RegistroCivil.Web/Program.cs RegistroCivil.Web/wwwroot/app.css
if errorlevel 1 exit /b 1
git diff --cached --quiet
if errorlevel 1 (
    git commit -m "Integra almacenamiento binario, pruebas y guia de entrega"
    if errorlevel 1 exit /b 1
)
git push origin main
if errorlevel 1 (
    echo No se publico. Revise el error; no use force ni borre cambios.
    exit /b 1
)
git status --short
echo Integracion publicada en main.
endlocal
