# Contrato HTTP

## Convenciones

- La API expone recursos bajo `/api`.
- Los cuerpos de solicitud y respuesta usan JSON.
- Los identificadores son `Guid`.
- Los precios se envían como números decimales en USD; no se acepta una moneda elegida por el cliente.
- Los ejemplos ejecutables están en la carpeta [http](http/).

La API no es el lugar donde se implementan las reglas del negocio: traduce HTTP a Commands o Queries de Application y devuelve DTOs.

## Recursos disponibles

| Operación | Método y ruta | Éxito |
|---|---|---|
| Crear producto de menú | `POST /api/productos` | `201 Created` + producto |
| Listar productos de menú | `GET /api/productos` | `200 OK` + colección de productos ordenada por nombre |
| Actualizar producto | `PUT /api/productos/{productoMenuId}` | `200 OK` + producto |
| Activar o desactivar producto | `PATCH /api/productos/{productoMenuId}/estado` | `200 OK` + producto |
| Obtener producto | `GET /api/productos/{productoMenuId}` | `200 OK` + producto |
| Abrir venta | `POST /api/ventas` | `201 Created` + venta |
| Listar historial de ventas | `GET /api/ventas` | `200 OK` + colección desde la más reciente |
| Obtener venta | `GET /api/ventas/{ventaId}` | `200 OK` + venta |
| Agregar detalle | `POST /api/ventas/{ventaId}/detalles` | `200 OK` + venta |
| Cambiar cantidad | `PUT /api/ventas/{ventaId}/detalles/{detalleId}` | `200 OK` + venta |
| Eliminar detalle | `DELETE /api/ventas/{ventaId}/detalles/{detalleId}` | `200 OK` + venta |
| Pagar venta | `POST /api/ventas/{ventaId}/pagar` | `200 OK` + venta |
| Cancelar venta | `POST /api/ventas/{ventaId}/cancelar` | `200 OK` + venta |

Las mutaciones de producto solo modifican el catálogo vigente. No reescriben los snapshots de nombre y precio ya guardados en los detalles históricos.

## Solicitudes

### Crear producto

```json
{
  "nombre": "Milanesa napolitana",
  "precio": 25.50
}
```

### Actualizar producto

```json
{
  "nombre": "Milanesa napolitana especial",
  "precio": 27.00
}
```

### Cambiar disponibilidad de producto

```json
{
  "estaActivo": false
}
```

### Abrir venta

```json
{
  "clienteId": "00000000-0000-0000-0000-000000000099",
  "numeroMesa": 7
}
```

`clienteId` y `numeroMesa` son opcionales. Si se proporcionan, deben ser válidos conforme a las reglas de dominio.

### Agregar detalle

```json
{
  "productoMenuId": "<guid-del-producto>",
  "cantidad": 2
}
```

### Cambiar cantidad

```json
{
  "nuevaCantidad": 3
}
```

### Pagar venta

```json
{
  "metodoPago": "Tarjeta"
}
```

Los valores admitidos por el dominio son `Efectivo`, `Tarjeta` y `Transferencia`.

### Cancelar venta

```json
{
  "motivoCancelacion": "El cliente solicitó cancelar antes del cobro."
}
```

El motivo es obligatorio, se recorta y no puede superar 500 caracteres. La respuesta expone `fechaCancelacionUtc` y `motivoCancelacion` para que la cancelación sea auditable.

## Respuestas y errores

Las respuestas de éxito usan DTOs de salida. Un producto incluye id, nombre, precio, moneda y estado de activación. Una venta incluye id, cliente, mesa, estado, fecha de creación, fechas de pago/cancelación, método de pago, motivo de cancelación, total, moneda y detalles.

Los errores de negocio no se devuelven como strings sueltas: la API usa `ProblemDetails`. A partir de la clasificación tipada de Application, la convención es:

| Caso | Código HTTP | Ejemplos |
|---|---:|---|
| Payload o validación de entrada inválida | `400 Bad Request` | Campos requeridos, tipos o valores de entrada inválidos. |
| Recurso inexistente | `404 Not Found` | Venta, producto o detalle no encontrado. |
| Conflicto de estado o concurrencia | `409 Conflict` | Operar sobre una venta terminal o detectar una versión desactualizada. |
| Regla de negocio no satisfecha | `422 Unprocessable Entity` | Pagar sin detalles o agregar un producto inactivo. |

Un error tiene, como mínimo, las propiedades estándar de Problem Details (`type`, `title`, `status`, `detail`) y los campos de extensión `codigo` y `categoria` cuando provenga de Application. No se deben inferir códigos HTTP mediante sufijos textuales de los errores.

Ejemplo de conflicto:

```json
{
  "type": "https://httpstatuses.com/409",
  "title": "Venta.YaPagada",
  "status": 409,
  "detail": "Una venta pagada no puede modificarse ni cancelarse.",
  "codigo": "Venta.YaPagada",
  "categoria": "Conflicto"
}
```

## Salud y OpenAPI

- `GET /health`: readiness de la API, incluido `AddDbContextCheck<RestauranteVentasDbContext>` contra PostgreSQL con etiqueta `ready`.
- `GET /alive`: liveness ligero configurado por `ServiceDefaults`; no depende de que PostgreSQL esté disponible.
- `/swagger`: interfaz OpenAPI en entorno Development.

La documentación de Swagger complementa, pero no reemplaza, estas reglas ni los ejemplos reproducibles de la carpeta `docs/http`.

## Compatibilidad de contrato

Cambiar rutas, nombres de campos, semántica de estados o códigos de error es un cambio de contrato. Debe hacerse con:

1. Actualización de DTOs/endpoints.
2. Pruebas de API e integración.
3. Actualización de este documento y los `.http`.
4. Versionado de API si el contrato ya tiene consumidores externos.
