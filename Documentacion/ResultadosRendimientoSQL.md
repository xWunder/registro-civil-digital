 Resultados de Rendimiento SQL

1. Configuración de la prueba

Se realizaron pruebas de búsqueda sobre la tabla `dbo.ActasNacimiento`.

Cantidad de registros utilizados:

- 100 001 registros en la base de datos.
- 100 000 registros correspondientes a datos simulados.

Las búsquedas evaluadas fueron:

- Búsqueda por DNI.
- Búsqueda por Número de Acta.

Para realizar las mediciones se utilizaron:

- `SET STATISTICS IO ON`
- `SET STATISTICS TIME ON`

---

2. Búsqueda por DNI

Consulta utilizada:

```sql
SELECT *
FROM dbo.ActasNacimiento
WHERE DniInscrito = '90100000';
```
3. Búsqueda por Número de Acta

Consulta utilizada:

```sql
SELECT *
FROM dbo.ActasNacimiento
WHERE NumeroActa = 'ACT-2026-100000';
```

| Registros | Consulta                    | Índice | Tiempo de ejecución | Lecturas lógicas | Lecturas físicas |
| --------: | --------------------------- | ------ | ------------------: | ---------------: | ---------------: |
|   100 001 | Búsqueda por DNI            | Sí     |               89 ms |                5 |                5 |
|   100 001 | Búsqueda por Número de Acta | Sí     |               45 ms |                6 |                3 |




4. Observaciones

La búsqueda con índice debe requerir menos lecturas lógicas que una búsqueda secuencial sin índice.

Los tiempos pueden variar según la memoria caché, procesador, almacenamiento y carga del equipo durante la ejecución.

5. Complejidad

La búsqueda mediante un índice presenta un comportamiento aproximado de O(log n).

Una búsqueda secuencial sin índice presenta un comportamiento O(n).

6. Escalamiento a 5 millones de registros

Con 5 millones de actas, una búsqueda secuencial podría requerir revisar una gran cantidad de registros.

El uso de índices permite reducir la cantidad de registros o páginas que deben revisarse para encontrar una coincidencia.

Los índices también requieren espacio adicional y mantenimiento durante las operaciones de inserción y modificación.

7. Características del equipo

Sistema operativo: Windows
Procesador: Procesador	AMD Ryzen 5 7535HS with Radeon Graphics, 3301 Mhz, 6 procesadores principales, 12 procesadores lógicos
Memoria RAM:8.00 GB
Motor de base de datos: Microsoft SQL Server 2019
Herramienta: SQL Server Management Studio 22

8.Conclusión 

Las pruebas permiten evaluar el comportamiento de las búsquedas sobre la tabla ActasNacimiento utilizando índices.

La utilización de un índice sobre DniInscrito permite realizar búsquedas directas y evita tener que recorrer toda la tabla para localizar un DNI específico.

A medida que aumenta la cantidad de registros, la utilización adecuada de índices se vuelve más importante para mantener un buen rendimiento.

Los tiempos de ejecución y las lecturas lógic	as obtenidas mediante SET STATISTICS IO y SET STATISTICS TIME permiten comparar objetivamente el comportamiento de las consultas.