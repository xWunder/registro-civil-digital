# Guía técnica y audiovisual para Moreno

## Alcance real de la entrega

Se integró el trabajo de archivos de origin/feature/osito-archivos (a2e4840) en la aplicación: cabecera, índice ordenado por DNI y número, búsqueda secuencial, búsqueda binaria y acceso directo. Se agregaron repositorio con CRUD, diario de recuperación, origen visible en formularios, preparación de 100 000 actas y comparación de tiempos. Se conserva el flujo SQL y la interfaz/paginación del equipo.

No presentar como terminado lo que aún debe producir Moreno: documento final (máximo 4 páginas según la consigna), capturas y video de 3 minutos. Tampoco afirmar que se probaron 5 millones o que existe un árbol B implementado: **el archivo utiliza un índice plano ordenado con búsqueda binaria**, no B-tree.

## Matriz de requisitos

| Requisito | Implementación/evidencia |
|---|---|
| Cabecera y registros | Firma RCDIG001, versión 1, cantidad, 64 bytes; registros de 512 bytes |
| Longitud fija o variable | Se eligió longitud fija; justificar acceso directo y costo de espacio |
| Acceso directo/indexación simple | Dos .idx ordenados; posición lógica -> 64 + posición × 512 |
| Insertar | Agrega al final; valida claves y actualiza cabecera/índices |
| Buscar | DNI o número; comparación secuencial e indexada en /archivos |
| Modificar | Reescritura del mismo bloque; mantiene ID y número, reconstruye índices |
| Listar | Paginación directa, tamaño seleccionable; no carga 100 000 filas al navegador |
| Anulación | Estado 0, sin eliminar físicamente ni liberar claves |
| 100 000 simulados | Generación reproducible, nombres ficticios, pruebas aisladas |
| Rendimiento | Calentamiento + 5 repeticiones, mediana/mínimo/máximo, CSV descargable |
| Escalabilidad | Análisis teórico a 5 millones; no prueba experimental a esa escala |
| SQL | Flujo conservado, scripts y comparador; verificación real pendiente en sesión normal |

## Organización lógica y física

Una acta tiene ID, número único, DNI opcional único, apellidos, nombres, fecha de nacimiento, sexo, ubigeo, lugar, fechas de registro/modificación y estado. Número inmutable; DNI nulo permitido en múltiples actas. No se permite editar una anulada ni duplicar claves de actas anuladas.

Cabecera (64 bytes): firma UTF-8 de 8 bytes, versión Int32 de 4, cantidad Int64 de 8, reserva de 44. Enteros en little-endian mediante BinaryWriter.

Registro (desplazamientos relativos al inicio del registro):

| Campo | Offset | Bytes |
|---|---:|---:|
| ID Int64 | 0 | 8 |
| Número, 20 unidades UTF-16LE | 8 | 40 |
| DNI, 8 unidades UTF-16LE | 48 | 16 |
| Apellido paterno, 40 | 64 | 80 |
| Apellido materno, 40 | 144 | 80 |
| Nombres, 60 | 224 | 120 |
| Fecha nacimiento, DayNumber Int32 | 344 | 4 |
| Sexo, byte | 348 | 1 |
| Ubigeo, 6 | 349 | 12 |
| Lugar, 60 | 361 | 120 |
| Fecha registro, ticks Int64 | 481 | 8 |
| Fecha modificación, ticks Int64 | 489 | 8 |
| Estado, byte | 497 | 1 |
| Reserva | 498 | 14 |

Texto relleno hasta longitud fija; se rechaza exceso, no se trunca. Una posición lógica p empieza en 64 + p × 512. Las actas permanecen en orden creciente de ID; los índices están ordenados por clave con comparación ordinal.

Índice DNI: clave UTF-16LE 16 bytes + posición Int64 8 = 24 bytes. Índice número: 40 + 8 = 48 bytes. Los índices no incluyen cabecera. Los DNI nulos no tienen entrada.

Archivos auxiliares: escritura.lock coordina procesos; operacion.json conserva la cabecera y el bloque previo; indices.pendientes obliga a reconstruir índices. Las escrituras usan diario antes de modificar y los lectores del repositorio comparten el bloqueo. Esto es recuperación académica ante interrupciones, no una garantía transaccional equivalente a SQL ni protección ante pérdida física del disco.

## Complejidad y crecimiento

- Búsqueda secuencial: O(n); puede ganar cuando el registro está al inicio.
- Búsqueda binaria del índice: O(log n), luego acceso directo O(1).
- Página de k actas: O(k), acceso directo por posición.
- Inserción en datos: escritura de un bloque de 512 bytes y cabecera. **Operación completa O(n log n)** por reconstruir índices.
- Edición/anulación: localización + escritura de un bloque, también reconstruye índices.
- Construcción de índices: O(n log n), memoria O(n); listas en memoria, no ordenamiento externo.
- 100 000 actas: 51 200 064 bytes de datos + hasta 7 200 000 bytes de índices.
- 5 millones: 2 560 000 064 bytes de datos + hasta 360 000 000 de índices, sin auxiliares ni memoria de objetos.
- Para esa escala convendrían índices con actualización incremental, páginas/B+ tree u ordenamiento externo y medición real. No prometer tiempos extrapolados.

## Medición reproducible

Usar el mismo equipo y conjunto de datos; anotar fecha, sistema, runtime y cantidad. En /archivos probar DNI 90100000 (inicio), 90149999 (medio), 90199999 (final), 00000000 (ausente), y ACT-2026-199999. Descargar CSV después de cada consulta.

La comparación hace un calentamiento excluido y cinco búsquedas por método. Informa medianas, no tiempo de renderizado. Caché caliente y orden fijo de métodos limitan la comparación; no son mediciones de disco frío. SQL incluye apertura de conexión + consulta. Si SQL no está disponible, mostrarlo como no medido, nunca como 0 ms.

Para equivalencia exacta SQL/archivo, importar SQL en una carpeta nueva vacía. Los generadores independientes pueden tener diferentes IDs y marcas de tiempo aunque las claves coincidan. La aplicación advierte si difiere el registro completo. No editar un origen y afirmar que el otro se sincronizó.

## Propuesta de documento de máximo 4 páginas

1. Problema, alcance y modelo lógico; breve matriz de funciones.
2. Organización física: tabla de tamaños, cabecera, fórmula e índices.
3. Algoritmos y resultados reales con entorno y metodología.
4. Complejidad, escalabilidad, límites, conclusiones y enlace al repositorio/video.

## Guion de 3 minutos

- 0:00–0:20: objetivo y elección de longitud fija.
- 0:20–0:45: /archivos con 100 000 registros; explicar 64/512 bytes y dos índices.
- 0:45–1:10: consultar archivo, paginar y buscar DNI/número.
- 1:10–1:45: registrar una acta ficticia con número DEMO-VIDEO-01 y DNI 88001234 (usar otra clave si ya existe), editar lugar y anular con confirmación.
- 1:45–2:25: comparar inicio/final/ausente, mostrar CSV y explicar por qué varía el costo.
- 2:25–2:45: acceso directo y O(log n), distinguir datos de índices.
- 2:45–3:00: límite de reconstrucción y propuesta a 5 millones; cierre.

Antes de grabar: preparar datos, probar la secuencia, ocultar notificaciones personales y cerrar ventanas ajenas. No grabar contraseñas ni información real. Si se muestra SQL, completar primero las pruebas pendientes y registrar sus resultados reales.

## Qué falta comprobar fuera del entorno aislado

En la sesión normal del equipo: iniciar SQL Server, ejecutar 06_VerificarIntegracion.sql, buscar una clave conocida y hacer alta/edición/anulación de un registro ficticio reservado para prueba. Importar a un archivo vacío y comprobar el mensaje de equivalencia. La prueba automatizada no sustituye esa verificación de SQL.
