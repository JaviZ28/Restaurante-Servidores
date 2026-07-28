# ADR 0004: Outbox transaccional y auditoría local

- **Estado:** Aceptada e implementada
- **Fecha:** 2026-07-28

## Contexto

`Venta` registra eventos cuando se crea, paga o cancela. Publicar un efecto antes del commit puede comunicar un hecho que luego no se persiste; hacerlo después sin persistencia puede perder el hecho si el proceso falla entre ambas operaciones.

## Decisión

Al confirmar cambios, el `DbContext` serializa los eventos pendientes a `outbox_mensajes` dentro de la misma operación de guardado que modifica el agregado. Cada mensaje usa `EventoId` como clave, conserva tipo, payload JSONB y fechas UTC. Los eventos se limpian del agregado solo después de una persistencia satisfactoria.

Un `BackgroundService` (`ProcesadorOutbox`) reclama mensajes pendientes mediante token y lease, escribe una auditoría estructurada y marca el mensaje procesado. Ante fallo conserva el error, aumenta intentos y aplica backoff antes del reintento.

## Consecuencias

- El cambio de estado y el mensaje durable se confirman juntos.
- La entrega es al menos una vez; los consumidores deben usar `EventoId` para ser idempotentes.
- El consumidor actual produce auditoría por log. No se afirma que exista un broker ni una integración remota.
- El lease reduce el procesamiento paralelo por múltiples instancias, pero un fallo entre el efecto y la marca de procesado puede repetir la entrega, lo que es el comportamiento esperado de este modelo.

## Alternativas descartadas

- Publicar directamente desde el agregado: acoplaría el dominio a infraestructura y rompería la consistencia transaccional.
- Publicar después de `SaveChanges` sin outbox: perdería eventos si el proceso falla.
- Añadir Kafka/RabbitMQ sin consumidor de negocio: aumentaría complejidad operativa sin aportar una necesidad del alcance actual.
