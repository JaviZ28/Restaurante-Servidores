# ADR 0005: Aspire y migraciones automáticas en desarrollo local

- **Estado:** Aceptada con límite de entorno
- **Fecha:** 2026-07-28

## Contexto

El proyecto necesita poder demostrarse desde una máquina limpia sin depender de una base configurada manualmente. PostgreSQL y la API deben iniciarse en orden y exponer su estado.

## Decisión

El AppHost de Aspire declara PostgreSQL, pgAdmin y la API; la API espera a la base y aplica migraciones al inicio únicamente en entorno `Development`, para desarrollo/demostración local. Las pruebas de integración usan `Aspire.Hosting.Testing` con `UseVolumes=false` para no compartir datos persistentes.

## Consecuencias

- El entorno local es reproducible y se valida desde una base nueva.
- Docker es una dependencia explícita para las integraciones.
- En producción, las migraciones deben ocurrir en un paso controlado y no al inicio de cada réplica de API.
