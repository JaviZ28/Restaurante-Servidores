# ADR 0002: CQRS lógico sin doble base de datos

- **Estado:** Aceptada
- **Fecha:** 2026-07-28

## Contexto

Las operaciones de cambio necesitan cargar agregados para hacer cumplir invariantes. Las consultas necesitan devolver DTOs. El volumen y los requisitos actuales no justifican sincronizar una segunda base o una proyección eventual.

## Decisión

Se separan Commands, Queries y handlers en Application, manteniendo una sola base PostgreSQL. Los comandos invocan comportamiento del agregado; las queries dependen de puertos de lectura (`IVentaLectura`, `IProductoMenuLectura`) y devuelven DTOs.

## Consecuencias

- La separación de responsabilidades existe sin introducir consistencia eventual innecesaria.
- Infrastructure implementa los puertos mediante proyecciones EF Core `AsNoTracking`; no carga el agregado de escritura para un `GET`.
- El nombre CQRS no se usará para afirmar que existen dos bases de datos o Event Sourcing.

## Criterio de evolución

Las consultas actuales ya aplican proyección directa a DTO y `AsNoTracking`. Una segunda base solo se considerará con una necesidad medible de escala, disponibilidad o forma de consulta.
