# ADR 0003: Snapshots históricos de productos en detalles

- **Estado:** Aceptada
- **Fecha:** 2026-07-28

## Contexto

Un producto de menú puede cambiar de nombre, precio o estado después de haberse agregado a una venta. Consultar siempre el producto actual haría que un consumo antiguo cambie retroactivamente.

## Decisión

Cada `DetalleVenta` guarda `ProductoMenuId`, `NombreHistorico` y `PrecioUnitarioHistorico` al momento de agregarlo. La venta calcula sus subtotales y total desde esos snapshots.

## Consecuencias

- El total histórico es estable y auditable.
- No se requiere una clave foránea que fuerce la existencia futura del producto para leer el detalle histórico.
- El precio actual y la disponibilidad del catálogo continúan siendo relevantes solamente antes de agregar una nueva línea.
