# Guía de defensa

## Demostración mínima sugerida

1. Inicie Docker y el AppHost de Aspire.
2. Espere el estado saludable de `api` y muestre `/health`.
3. Cree un producto de menú.
4. Cree una venta abierta, agregue un detalle y cambie la cantidad.
5. Pague la venta y consulte el resultado final.
6. Muestre en los logs de la API/Aspire la auditoría estructurada del evento procesado por outbox.
7. Cree otra venta y cancélela con motivo para mostrar el segundo estado terminal.
8. Ejecute las pruebas y explique que las integraciones arrancan PostgreSQL efímero desde cero.

Use [docs/http/ventas.http](http/ventas.http) y [docs/http/productos.http](http/productos.http) durante la demo.

## Preguntas frecuentes del docente

### ¿Por qué `Venta` se comporta como una comanda?

Porque se crea antes de tener líneas y antes del cobro. En estado `Abierta` representa consumo en curso; el pago es una transición de su ciclo de vida. El nombre técnico se mantiene por compatibilidad del contrato actual, pero la documentación no confunde esa semántica.

### ¿Cuál es el aggregate root y por qué?

`Venta` es el aggregate root de sus detalles: decide cuándo se agregan, cambian, eliminan, pagan o cancelan. `DetalleVenta` no se modifica directamente desde fuera. `ProductoMenu` es otro agregado porque tiene identidad y ciclo de vida propios.

### ¿Por qué `Dinero`, `Cantidad`, `NumeroMesa` y `NombreProducto` son Value Objects?

No se identifican por una clave propia; se identifican por sus valores y concentran validaciones. Así, un importe, una cantidad o una mesa inválidos no se propagan como tipos primitivos sin significado.

### ¿Cómo se conserva un precio histórico?

Al agregar un producto se copian su nombre y precio actual al detalle. El total se calcula desde esos valores copiados. Cambiar después el producto de menú no modifica lo ya consumido.

### ¿Por qué no hay microservicios?

Los módulos tienen límites claros, pero no existe una necesidad comprobada de despliegue independiente, propiedad por equipos distintos o escalamiento autónomo. Un monolito modular resuelve el alcance con menor costo operativo.

### ¿Qué significa CQRS en este proyecto?

Commands y queries expresan intenciones diferentes y tienen handlers distintos. Los comandos protegen invariantes a través de agregados; las queries devuelven DTOs. CQRS no exige dos bases de datos ni Event Sourcing.

### ¿Eventos de dominio e integración son lo mismo?

No. El evento de dominio nace en `Venta`. Al guardar, el DbContext lo convierte en un mensaje JSONB de `outbox_mensajes` dentro de la misma confirmación. `ProcesadorOutbox` reclama el mensaje con token y lease, emite una auditoría estructurada y lo marca procesado o programa reintento. La entrega es al menos una vez: `EventoId` permite que un consumidor sea idempotente. El consumidor actual es local y de auditoría; no se afirma que exista un broker externo.

### ¿Cómo se evita que dos cambios simultáneos se pisen?

`Venta` usa `xmin`, la columna de sistema de PostgreSQL, como token de concurrencia optimista. Si otra operación ya actualizó la misma venta, EF Core detecta que la versión no coincide, la unidad de trabajo traduce la excepción y la API responde `409 Conflict`. Esto no es lo mismo que idempotencia HTTP: el cliente todavía debe recargar y decidir cómo reintentar.

### ¿Qué demuestra Aspire?

Que la API y PostgreSQL se orquestan de forma reproducible, que la API espera la dependencia y publica health checks. También permite observar la auditoría del procesador outbox en los logs. Las pruebas de integración validan el arranque y un flujo persistido con PostgreSQL real, no un mock de repositorio.

### ¿Por qué se prueban tres niveles?

Las unitarias del dominio verifican invariantes rápidos y deterministas; las de Application verifican orquestación y errores; las integraciones validan el sistema completo, incluyendo AppHost, migraciones, HTTP y PostgreSQL.

## Respuestas que deben ser transparentes

No atribuya al sistema patrones que no estén demostrados. Antes de afirmar cualquiera de estas capacidades, muestre su implementación y prueba:

- Idempotencia para reintentos HTTP.
- Autenticación/autorización.
- Un broker o consumidores externos del outbox.

Una respuesta sólida reconoce el límite, explica la evolución y evita convertir una carpeta o una clase sin consumidor en una capacidad empresarial completa.
