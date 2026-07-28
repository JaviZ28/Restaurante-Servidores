# ADR 0006: Concurrencia optimista para ventas con `xmin`

- **Estado:** Aceptada e implementada
- **Fecha:** 2026-07-28

## Contexto

Dos solicitudes pueden cargar una misma venta abierta y tratar de modificarla. Sin control de versión, la última escritura puede sobrescribir una transición o detalle de la primera sin que el usuario sea informado.

## Decisión

Las entidades `Venta` usan la columna de sistema `xmin` de PostgreSQL como token de concurrencia. EF Core incorpora el valor en la condición de actualización y arroja `DbUpdateConcurrencyException` si no encuentra la versión esperada. La unidad de trabajo la traduce a `ConflictoConcurrenciaException`; la API responde `409 Conflict` mediante el manejador global de excepciones.

## Consecuencias

- No se agrega una propiedad técnica visible al agregado ni una columna artificial.
- El cliente recibe un conflicto explícito y puede recargar el recurso antes de reintentar.
- La protección cubre el agregado `Venta`; no debe presentarse como concurrencia universal de todos los recursos.
- Esto no vuelve idempotente un command HTTP: una llave de idempotencia sigue siendo una decisión independiente.
