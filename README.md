# RestauranteVentas — Dominio

Implementación del núcleo de dominio para la gestión de ventas de un restaurante, construida con DDD (Domain-Driven Design) y .NET 10.

El proyecto concentra las reglas de negocio de ventas y no depende de infraestructura, bases de datos, HTTP, Entity Framework ni paquetes externos.

## Características principales

- Gestión del agregado `Venta` y sus detalles.
- Productos de menú activos o inactivos.
- Value Objects inmutables para dinero, cantidades, mesas y nombres de producto.
- Estados de venta: `Abierta`, `Pagada` y `Cancelada`.
- Conservación de nombre y precio históricos en cada detalle de venta.
- Eventos de dominio al crear, pagar y cancelar una venta.
- Resultados explícitos (`Resultado` y `Resultado<T>`) para reglas de negocio sin excepciones de flujo.
- Validaciones defensivas para identificadores, argumentos nulos y métodos de pago inválidos.
- Interfaces de repositorio asíncronas preparadas para una futura implementación con infraestructura.

## Reglas de negocio

- Una venta puede crearse sin cliente, mesa o ambos.
- La moneda permitida es USD y los montos deben ser mayores que cero.
- Una venta necesita al menos un detalle para poder pagarse.
- Solo una venta abierta puede agregar, cambiar o eliminar detalles.
- Una venta pagada o cancelada no puede modificarse.
- Un producto inactivo no puede agregarse a una venta.
- El total se calcula a partir de los detalles y nunca se asigna manualmente.
- El pago se realiza una única vez mediante efectivo, tarjeta o transferencia.

La descripción completa del modelo está disponible en [docs/modelo-dominio.md](docs/modelo-dominio.md).

## Estructura

```text
src/
└── RestauranteVentas.Dominio/
    ├── Abstracciones/   # Entidad, eventos, errores y resultados
    ├── Compartido/      # Value Objects
    ├── Productos/       # ProductoMenu y su repositorio
    └── Ventas/          # Agregado Venta, detalles y eventos

tests/
└── RestauranteVentas.Dominio.Tests/
    ├── Compartido/
    ├── Productos/
    └── Ventas/
```

## Tecnologías

- .NET 10
- C#
- xUnit

## Ejecutar las pruebas

Desde la raíz del repositorio:

```bash
dotnet test RestauranteVentas.slnx
```

Actualmente el proyecto cuenta con **35 pruebas unitarias** de dominio, sin mocks, bases de datos ni servicios externos.

## Principios del dominio

El proyecto `RestauranteVentas.Dominio` es una biblioteca independiente y pura. Las capas futuras —Application, Infrastructure y API— deben depender del dominio, nunca al contrario.
