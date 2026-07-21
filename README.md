# RestauranteVentas — Dominio + Application (CQRS)

Implementación del núcleo de negocio y de la capa de aplicación para la gestión de ventas de un restaurante, construida con DDD (Domain-Driven Design), CQRS y .NET 10.

## Estado actual

- `RestauranteVentas.Dominio` concentra reglas de negocio puras (sin infraestructura).
- `RestauranteVentas.Aplicacion` orquesta casos de uso con Commands, Queries y Handlers.
- El mapeo de entidades a DTOs se realiza de forma manual.
- Se incluyen pruebas unitarias de dominio y pruebas unitarias de handlers con mocks.
- Se incluyen archivos `.http` para probar todos los endpoints definidos.

## Características principales

- Gestión del agregado `Venta` y sus detalles.
- Productos de menú activos o inactivos.
- Value Objects inmutables para dinero, cantidades, mesas y nombres de producto.
- Estados de venta: `Abierta`, `Pagada` y `Cancelada`.
- Conservación de nombre y precio históricos en cada detalle de venta.
- Eventos de dominio al crear, pagar y cancelar una venta.
- Resultados explícitos (`Resultado`, `Resultado<T>`, `ResultadoAplicacion<T>`) para manejar reglas y validaciones.
- Interfaces de repositorio asíncronas y unidad de trabajo para persistencia desacoplada.

## Reglas de negocio

- Una venta puede crearse sin cliente, mesa o ambos.
- La moneda permitida es USD y los montos deben ser mayores que cero.
- Una venta necesita al menos un detalle para poder pagarse.
- Solo una venta abierta puede agregar, cambiar o eliminar detalles.
- Una venta pagada o cancelada no puede modificarse.
- Un producto inactivo no puede agregarse a una venta.
- El total se calcula a partir de los detalles y nunca se asigna manualmente.
- El pago se realiza una única vez mediante efectivo, tarjeta o transferencia.

La descripción completa del modelo de dominio está disponible en [docs/modelo-dominio.md](docs/modelo-dominio.md).

## CQRS en Application

### Commands implementados

- `CrearProductoMenuComando`
- `CrearVentaComando`
- `AgregarProductoVentaComando`
- `CambiarCantidadDetalleVentaComando`
- `EliminarDetalleVentaComando`
- `PagarVentaComando`
- `CancelarVentaComando`

### Queries implementadas

- `ObtenerProductoMenuPorIdConsulta`
- `ObtenerVentaPorIdConsulta`

### Contratos de aplicación

- Interfaces base CQRS: `IComando`, `IConsulta`, `IComandoHandler`, `IConsultaHandler`.
- Orquestación transaccional: `IUnidadDeTrabajo`.
- Dependencias temporales/identidad desacopladas: `IReloj`, `IGeneradorIdentidad`.
- DTOs de salida y mapeadores manuales.

## Estructura

```text
src/
├── RestauranteVentas.Dominio/
│   ├── Abstracciones/   # Entidad, eventos, errores y resultados
│   ├── Compartido/      # Value Objects
│   ├── Productos/       # ProductoMenu y su repositorio
│   └── Ventas/          # Agregado Venta, detalles y eventos
└── RestauranteVentas.Aplicacion/
    ├── Abstracciones/   # Contratos CQRS, resultado de app y unidad de trabajo
    ├── Dtos/            # Contratos de salida
    ├── Mapeadores/      # Mapeo manual Dominio -> DTO
    ├── Productos/       # Commands/Queries/Handlers de productos
    └── Ventas/          # Commands/Queries/Handlers de ventas

tests/
├── RestauranteVentas.Dominio.Tests/
│   ├── Compartido/
│   ├── Productos/
│   └── Ventas/
└── RestauranteVentas.Aplicacion.Tests/
    ├── Helpers/
    ├── Productos/
    └── Ventas/

docs/
├── modelo-dominio.md
└── http/
    ├── productos.http
    └── ventas.http
```

## Endpoints documentados (.http)

- `POST /api/productos`
- `GET /api/productos/{productoId}`
- `POST /api/ventas`
- `GET /api/ventas/{ventaId}`
- `POST /api/ventas/{ventaId}/detalles`
- `PUT /api/ventas/{ventaId}/detalles/{detalleId}`
- `DELETE /api/ventas/{ventaId}/detalles/{detalleId}`
- `POST /api/ventas/{ventaId}/pagar`
- `POST /api/ventas/{ventaId}/cancelar`

## Tecnologías

- .NET 10
- C#
- xUnit
- Moq

## Ejecutar las pruebas

Desde la raíz del repositorio:

```bash
dotnet test RestauranteVentas.slnx
```

Estado actual: **53 pruebas unitarias** (dominio + aplicación), sin base de datos ni servicios externos.

## Principios de arquitectura

- `RestauranteVentas.Dominio` se mantiene puro y sin dependencias externas.
- `RestauranteVentas.Aplicacion` depende del dominio para ejecutar casos de uso.
- Las capas futuras de Infrastructure y API deben depender de Application/Dominio, nunca al revés.
