# Decisiones arquitectónicas

Los ADRs (Architecture Decision Records) capturan decisiones que afectan el modelo o la arquitectura. Cada registro explica contexto, decisión, consecuencias y estado; no reemplaza las pruebas ni el código.

| ADR | Decisión | Estado |
|---|---|---|
| [0001](0001-monolito-modular-y-contextos.md) | Monolito modular con dos contextos delimitados. | Aceptada |
| [0002](0002-cqrs-sin-doble-base.md) | CQRS lógico sin doble base de datos. | Aceptada |
| [0003](0003-snapshots-historicos.md) | Snapshots de nombre y precio en el detalle. | Aceptada |
| [0004](0004-eventos-y-outbox.md) | Outbox transaccional y auditoría local con entrega al menos una vez. | Aceptada e implementada |
| [0005](0005-aspire-y-migraciones-locales.md) | Aspire y migraciones automáticas para desarrollo local. | Aceptada con límite de entorno |
| [0006](0006-concurrencia-optimista-ventas.md) | Concurrencia optimista de ventas con `xmin` de PostgreSQL. | Aceptada e implementada |
