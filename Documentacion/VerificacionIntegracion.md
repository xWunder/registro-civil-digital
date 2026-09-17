# Verificación de integración — 17/09/2026

## Resultado comprobado

- Compilación de la solución: 0 errores y 0 advertencias, tanto con UseAppHost=false como en la compilación normal final con restauración previa.
- Pruebas del módulo de archivos: **44 aprobadas**, código de salida 0.
- Entorno: Windows 10.0.26200, .NET 10.0.12, 8 procesadores lógicos.
- Se compilan las mismas fuentes de FileStorage y del modelo que utiliza la web, enlazadas en RegistroCivil.Verificacion; no un algoritmo alternativo de prueba.
- Los datos de prueba están aislados en una carpeta temporal única; no se modificó SQL.
- Hay 100 000 actas ficticias preparadas en el archivo local de la aplicación. Los compañeros generan su propia copia.

## Cobertura automatizada

Cabecera y tamaño exacto del archivo; tamaños de ambos índices; DNI y número al inicio/medio/final; clave ausente; normalización; paginación y última página; prevención de sobrescritura; duplicados; DNI no ASCII; fechas futuras; longitudes; inserción de 512 bytes conservando registros; Unicode; edición con cambio de DNI e índices; anulación y repetición; bloqueo de edición de anuladas; reserva de DNI; múltiples DNI nulos; persistencia; reconstrucción de índice faltante; recuperación de escritura interrumpida; cabecera truncada; rechazo de truncamiento de texto; equivalencia secuencial/indexada en seis consultas.

## Medición de una ejecución documentada del módulo de archivos

Conjunto de exactamente 100 000 actas separado del CRUD. Carga + índices: **3456.467 ms**. Cada consulta: 1 calentamiento excluido + 5 repeticiones; caché caliente. Valores en milisegundos:

| Clave | Sec. mediana | Sec. mín. | Sec. máx. | Índ. mediana | Índ. mín. | Índ. máx. |
|---|---:|---:|---:|---:|---:|---:|
| 90100000 | 0.5156 | 0.4647 | 0.8309 | 1.4061 | 1.2207 | 2.8025 |
| 90149999 | 148.4045 | 134.7631 | 186.8480 | 1.1491 | 1.0792 | 1.2654 |
| 90199999 | 338.1722 | 290.1095 | 382.1823 | 1.0823 | 1.0739 | 1.4066 |
| 00000000 (ausente) | 344.6467 | 296.7384 | 401.7576 | 0.5372 | 0.5085 | 0.6274 |
| ACT-2026-199999 | 264.9836 | 227.6531 | 317.6289 | 1.1932 | 1.0534 | 1.3282 |
| NO-EXISTE | 315.3308 | 271.8441 | 342.8401 | 0.6776 | 0.5160 | 0.9434 |

Interpretación: la secuencial gana al inicio y empeora al recorrer más actas. El índice evita el recorrido completo. Los tiempos dependen del equipo, carga concurrente y caché; no garantizan esos valores en otro equipo. No mezclar estos resultados con los históricos de SQL ni atribuirlos a 5 millones.

Reproducir desde la raíz:
```bat
dotnet run --project RegistroCivil.Verificacion
```

## Comprobaciones de navegador

La página /archivos carga; se generaron 100 000 registros desde la interfaz; se muestran tamaño, cabecera y registros. El comparador presenta valores reales, exportación CSV y mensaje explícito cuando SQL no está disponible. El listado mostró 1–50 de 100 000, la última página mostró 99 951–100 000, y el DNI 90149999 devolvió una única acta correcta.

La prueba web usa http://localhost:5089 con claves efímeras de desarrollo en un proceso propio, porque el entorno aislado no puede descifrar claves DPAPI de la cuenta de Windows. La opción Verification:EphemeralKeys solo funciona en Development, no está activa por defecto y no debe emplearse como configuración de producción. La ejecución normal del usuario utiliza su perfil http/https.

También se completó el CRUD desde el navegador: alta de DEMO-QA-20260917 (ID 100001, datos ficticios), búsqueda por número, edición del lugar y anulación confirmada. Se verificaron la fecha 15/01/2000 y los acentos persistidos. Esta acta queda conservada como anulada en el archivo local: hay 100 000 registros base más 1 registro de prueba, sin cambios en SQL.

Menú móvil verificado a 390 × 844: abre por clic, cierra por Enter y se cierra al navegar. Se asociaron etiquetas a campos y se agregó validación de texto/fecha durante la entrada. La comparación final en navegador mostró ambos métodos sobre 100 001 registros y un enlace CSV con los mismos valores de la tabla.

## Pendientes y límites explícitos

Se repitió finalmente el comando estándar dotnet run --project RegistroCivil.Verificacion --no-build: nuevamente 44 pruebas aprobadas, código de salida 0. La tabla anterior conserva una ejecución completa concreta, sin mezclar tiempos entre corridas.

- **SQL en vivo no validado en este entorno:** autenticación/cifrado del proceso aislado falló. No se modificó ni debilitó la configuración SQL. Ejecutar Database/06_VerificarIntegracion.sql y probar CRUD SQL en sesión normal.
- No se ejecutó la actualización 05 sobre la base existente del usuario en esta integración.
- No se probó experimentalmente con 5 millones.
- No hay sincronización automática SQL/archivo, autenticación de usuarios ni autorización por roles: aplicación académica local, no desplegar públicamente con datos reales.
- No se garantiza tolerancia a daños físicos del disco. El diario cubre la recuperación ensayada de una operación interrumpida; realizar copias de seguridad.
- La calificación depende de la rúbrica y de la sustentación, documento y video; estas pruebas no equivalen a prometer una nota.
