# Operación local, migraciones y pruebas

## Requisitos

- SDK de .NET 10 instalado.
- Docker Desktop en ejecución.
- Acceso a NuGet configurado por `nuget.config`.
- CLI de Aspire para iniciar el AppHost de forma interactiva o no interactiva.

## Inicio local con Aspire

Desde la raíz del repositorio:

```powershell
aspire start --apphost RestauranteVentas.AppHost/RestauranteVentas.AppHost.csproj --non-interactive
aspire wait api --non-interactive
```

Aspire levanta PostgreSQL, expone pgAdmin y arranca la API. La URL final de la API depende de los puertos asignados por Aspire; use la URL mostrada por el dashboard/CLI en los archivos `.http`. `/health` comprueba readiness, incluida la conexión del `RestauranteVentasDbContext` a PostgreSQL; `/alive` es el liveness ligero.

En entorno `Development`, la API aplica las migraciones pendientes al iniciar. Esto facilita el desarrollo y la demostración local. En un despliegue de producción se debe usar un paso de migración explícito, auditable y coordinado con el despliegue; no se debe convertir el inicio de cada instancia en el mecanismo de migración.

Para detener los recursos:

```powershell
aspire stop --non-interactive
```

## Pruebas

Ejecute toda la solución:

```powershell
dotnet restore RestauranteVentas.slnx
dotnet test RestauranteVentas.slnx --configuration Release
```

La verificación final registrada el 28 de julio de 2026 produjo el siguiente resultado:

| Grupo | Casos | Alcance |
|---|---:|---|
| Dominio | 62 | Value Objects, producto, reglas de `Venta` y payloads de eventos. |
| Application | 22 | Commands, queries, clasificación de errores y catálogo mediante dobles de prueba. |
| Arquitectura | 4 | Dirección de dependencias entre Dominio, Application, Infrastructure y API. |
| Integración | 7 | AppHost, health check, PostgreSQL, migraciones, ProblemDetails, catálogo, outbox y flujos de venta. |
| **Total** | **95** | **Suite completa** |

Las pruebas de integración usan `Aspire.Hosting.Testing` y construyen el AppHost con `UseVolumes=false`. Así se usa una instancia PostgreSQL temporal en lugar del volumen normal de desarrollo. Docker debe estar iniciado para que estas pruebas puedan completar.

La ejecución final también confirmó:

- Compilación de la solución: 0 advertencias y 0 errores.
- EF Core: sin cambios de modelo pendientes respecto de las migraciones.
- Ciclo de integración: el AppHost no queda activo al terminar.

## Integración continua

El workflow [`.github/workflows/ci.yml`](../.github/workflows/ci.yml) restaura dependencias, compila la solución en `Release`, comprueba Docker, ejecuta las pruebas y recoge cobertura en formato Cobertura. Los archivos `coverage.cobertura.xml` se publican como artefacto del workflow. La verificación de Docker es intencional: las integraciones levantan recursos reales mediante Aspire, no sustitutos en memoria.

## Verificación desde una base limpia

La suite de integración ya verifica que el entorno temporal pueda iniciar desde cero y aplicar la migración de la API. Para validar además el entorno persistente de desarrollo, siga este protocolo:

1. Detenga Aspire.
2. Identifique el volumen local antes de borrarlo.
3. Elimine solo el volumen de este proyecto si los datos son desechables.
4. Levante Aspire y espere a que `api` esté saludable.
5. Ejecute las pruebas y un flujo HTTP de creación, pago y consulta.

Comandos de inspección seguros:

```powershell
docker volume ls --filter "name=restauranteventas-postgres-data"
docker ps -a --filter "volume=restauranteventas-postgres-data"
```

Si se confirma que no hay información que deba conservarse, la limpieza es:

```powershell
docker volume rm restauranteventas-postgres-data
```

> **Advertencia:** `docker volume rm` elimina de forma irreversible la información local almacenada en esa base. Detenga antes los recursos que usen el volumen. No ejecute el comando contra un volumen cuyo contenido deba recuperarse.

La validación registrada para este repositorio eliminó el volumen `restauranteventas-postgres-data`, inició PostgreSQL limpio, aplicó las migraciones y aprobó 95/95 casos.

## Migraciones de EF Core

Restaure las herramientas locales y configure una cadena de conexión de desarrollo para inspeccionar cambios del modelo:

```powershell
dotnet tool restore
$env:RESTAURANTEVENTAS_CONNECTION_STRING = "Host=localhost;Port=5432;Database=restauranteventas;Username=postgres;Password=<password>"
dotnet tool run dotnet-ef migrations has-pending-model-changes --project src/RestauranteVentas.Infrastructure --context RestauranteVentasDbContext
```

Para crear una migración, el cambio debe estar aprobado y probado primero:

```powershell
dotnet tool run dotnet-ef migrations add <NombreDescriptivo> --project src/RestauranteVentas.Infrastructure --startup-project src/RestauranteVentas.Api --context RestauranteVentasDbContext
```

Revise la migración generada y repita las pruebas de integración. Nunca modifique una migración que ya haya sido aplicada en un entorno compartido; cree una nueva migración correctiva.

## Diagnóstico rápido

| Síntoma | Comprobación inicial | Acción sugerida |
|---|---|---|
| Las pruebas de integración exceden el tiempo | `docker info` | Inicie Docker Desktop y vuelva a ejecutar la suite. |
| Aspire no encuentra la API | `aspire wait api --non-interactive` | Revise el estado y los logs en el dashboard de Aspire. |
| La API no inicia por conexión | Estado del recurso `restauranteventas` | Espere el health check de PostgreSQL; el AppHost usa `WaitFor`. |
| Hay datos inesperados en desarrollo | Inspeccione el volumen del proyecto | Decida si necesita conservarlos antes de eliminar el volumen. |
| El modelo no coincide con migraciones | Comando `has-pending-model-changes` | Cree y revise una migración nueva. |

## Criterio de evidencia para la defensa

Una ejecución válida debe poder mostrar:

1. Docker disponible.
2. AppHost iniciado desde cero.
3. API saludable en `/health`.
4. Creación de producto y venta, detalle, pago o cancelación.
5. Consulta del estado final persistido.
6. Pruebas unitarias e integración correctas.

No basta con que la aplicación se vea en el dashboard: la evidencia debe incluir la persistencia y los resultados de prueba.
