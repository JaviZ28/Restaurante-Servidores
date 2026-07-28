# ADR 0001: Monolito modular con contextos delimitados

- **Estado:** Aceptada
- **Fecha:** 2026-07-28

## Contexto

El dominio tiene dos áreas con ciclos de vida distintos: el catálogo de menú y las comandas/cobros. El alcance actual no exige despliegue independiente, equipos separados, escalado asimétrico ni integraciones remotas entre ellas.

## Decisión

Se implementará un monolito modular. `ProductoMenu` pertenece al contexto de Catálogo; `Venta` y `DetalleVenta` pertenecen al contexto de Comandas y cobro. Los módulos se separan por lenguaje y responsabilidades, pero comparten solución, proceso y PostgreSQL.

## Consecuencias

- Se conserva un modelo claro sin costo operativo de microservicios.
- Los límites siguen siendo visibles y el código puede extraerse en el futuro si el negocio lo justifica.
- No se usa una red o cola como sustituto de un buen límite de dominio.

## Alternativas descartadas

- Microservicios: añaden despliegue, observabilidad, fallos de red y consistencia distribuida sin una necesidad demostrada.
- Un único módulo de “ventas” sin límites: mezcla catálogo y cobro, dificultando reglas e historial.
