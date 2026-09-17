# Registro Civil Digital — entrega integrada

Aplicación académica ASP.NET Core / Blazor (.NET 10), SQL Server y almacenamiento binario de longitud fija. Solo datos ficticios; no es un sistema habilitado para uso institucional real.

## Inicio rápido (CMD, desde la carpeta que contiene RegistroCivil.Web.slnx)

```bat
dotnet restore RegistroCivil.Web.slnx
dotnet build RegistroCivil.Web.slnx
dotnet run --project RegistroCivil.Web --launch-profile http
```

Abrir **http://localhost:5074/archivos**. Dejar la consola abierta. El navegador puede no abrirse automáticamente con dotnet run; abrir la dirección manualmente.

En un equipo nuevo, elegir **Generar 100 000 actas ficticias**. Después, **Consultar archivo** permite listar, buscar, registrar, editar y anular sin SQL Server. La generación solo funciona sobre un archivo vacío: no reemplaza trabajo existente.

Para repetir las comprobaciones aisladas:

```bat
dotnet run --project RegistroCivil.Verificacion
```

Crea carpetas temporales propias y no modifica SQL ni el archivo usado por la web. Devuelve código distinto de cero si falla una comprobación.

## Dos almacenamientos explícitos

- Las rutas normales /actas y /actas/registrar trabajan con SQL.
- Las mismas rutas con ?origen=archivo trabajan con actas.dat y sus índices.
- /archivos permite preparar el archivo, importar SQL hacia un archivo vacío y comparar búsquedas.
- **No existe sincronización automática**. Registrar/editar/anular en un origen no modifica el otro.
- Git descarga código y scripts, no las bases SQL ni los archivos de ejecución. Cada compañero debe preparar sus datos.

Los archivos locales se guardan en RegistroCivil.Web/Data/ArchivoCivil (ignorado por Git). Para otra copia de demostración, utilizar una carpeta nueva sin borrar la anterior:

```bat
dotnet run --project RegistroCivil.Web --launch-profile http -- --Archivos:Directorio=Data/DemoVideo
```

## Preparación SQL (si se desea usar el origen SQL)

Con SQL Server en ejecución y una conexión válida, abrir en SSMS los scripts de RegistroCivil.Web/Database:

1. 01_CrearBaseDatos.sql: esquema, restricciones e índice DNI.
2. 02_AsegurarDniUnico.sql: revisar sus mensajes de duplicados antes de continuar.
3. 03_GenerarDatosPrueba.sql: carga masiva ficticia; conserva números existentes.
4. 05_ActualizarNombresSimulados.sql: opcional para las bases antiguas con PersonaN. Revisar y respaldar primero; no reemplaza nombres personalizados.
5. 06_VerificarIntegracion.sql: diagnóstico de solo lectura.
6. 04_MedirRendimiento.sql: medición SQL propia del equipo.

Configurar ConnectionStrings:RegistroCivil mediante configuración local o secretos de usuario; no subir contraseñas. No reinstalar ni borrar bases para resolver errores de conexión.

**Estado de validación:** el módulo binario y la compilación están probados. La conexión SQL desde el entorno aislado de verificación no pudo autenticarse; queda pendiente repetir el flujo SQL en la sesión normal de Windows antes de afirmar que toda la integración SQL está validada.

## Publicación pendiente desde la sesión normal

El entorno de integración no pudo escribir .git/index.lock, incluso con permiso solicitado. Los cambios están en el disco, **no publicados por esta integración**. Revisar git status y ejecutar en CMD, desde la raíz, PublicarIntegracion.cmd. El script verifica main, compila, ejecuta las pruebas, agrega los archivos de esta entrega, crea el commit y hace push sin forzar. Se detiene ante errores. Cerrar la ejecución de la web antes de compilar.

Una vez publicado, cada compañero con su trabajo guardado puede actualizar main:

```bat
git status
git switch main
git pull --ff-only origin main
dotnet run --project RegistroCivil.Web --launch-profile http
```

Si git status muestra cambios propios o cualquier comando falla, detenerse y conservarlos. No usar reset --hard. Para incorporar main a una rama de trabajo propia, estando en esa rama y sin cambios pendientes: git fetch origin y git merge origin/main; resolver conflictos antes de continuar.

## Material de entrega

Leer [Entrega para Moreno](Documentacion/EntregaParaMoreno.md) y [Verificación](Documentacion/VerificacionIntegracion.md). Incluyen el formato físico, complejidad, límites, guion y evidencias. Los resultados antiguos de SQL son evidencia histórica, no mediciones nuevas de esta integración.
