using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RestauranteVentas.Infrastructure.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class FortalecerPersistenciaYOutbox : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "fecha_cancelacion_utc",
                table: "ventas",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "motivo_cancelacion",
                table: "ventas",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                table: "ventas",
                type: "xid",
                rowVersion: true,
                nullable: false,
                defaultValue: 0u);

            migrationBuilder.CreateTable(
                name: "outbox_mensajes",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    tipo_evento = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    contenido = table.Column<string>(type: "jsonb", nullable: false),
                    ocurrido_en_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    creado_en_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    procesado_en_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    intentos = table.Column<int>(type: "integer", nullable: false),
                    error_procesamiento = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_outbox_mensajes", x => x.id);
                    table.CheckConstraint("CK_outbox_mensajes_intentos_no_negativos", "\"intentos\" >= 0");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ventas_estado_fecha_creacion_utc",
                table: "ventas",
                columns: new[] { "estado", "fecha_creacion_utc" });

            migrationBuilder.AddCheckConstraint(
                name: "CK_ventas_cliente_id_no_vacio",
                table: "ventas",
                sql: "\"cliente_id\" IS NULL OR \"cliente_id\" <> '00000000-0000-0000-0000-000000000000'::uuid");

            migrationBuilder.AddCheckConstraint(
                name: "CK_ventas_estado_valido",
                table: "ventas",
                sql: "\"estado\" IN ('Abierta', 'Pagada', 'Cancelada')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_ventas_fecha_cancelacion_posterior",
                table: "ventas",
                sql: "\"fecha_cancelacion_utc\" IS NULL OR \"fecha_cancelacion_utc\" > \"fecha_creacion_utc\"");

            migrationBuilder.AddCheckConstraint(
                name: "CK_ventas_fecha_creacion_valida",
                table: "ventas",
                sql: "\"fecha_creacion_utc\" > TIMESTAMPTZ '0001-01-01 00:00:00+00'");

            migrationBuilder.AddCheckConstraint(
                name: "CK_ventas_fecha_pago_posterior",
                table: "ventas",
                sql: "\"fecha_pago_utc\" IS NULL OR \"fecha_pago_utc\" > \"fecha_creacion_utc\"");

            migrationBuilder.AddCheckConstraint(
                name: "CK_ventas_metodo_pago_valido",
                table: "ventas",
                sql: "\"metodo_pago\" IS NULL OR \"metodo_pago\" IN ('Efectivo', 'Tarjeta', 'Transferencia')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_ventas_numero_mesa_positivo",
                table: "ventas",
                sql: "\"numero_mesa\" IS NULL OR \"numero_mesa\" > 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_ventas_transicion_estado_coherente",
                table: "ventas",
                sql: "(\"estado\" = 'Abierta' AND \"fecha_pago_utc\" IS NULL AND \"metodo_pago\" IS NULL AND \"fecha_cancelacion_utc\" IS NULL AND \"motivo_cancelacion\" IS NULL) OR (\"estado\" = 'Pagada' AND \"fecha_pago_utc\" IS NOT NULL AND \"metodo_pago\" IS NOT NULL AND \"fecha_cancelacion_utc\" IS NULL AND \"motivo_cancelacion\" IS NULL) OR (\"estado\" = 'Cancelada' AND \"fecha_pago_utc\" IS NULL AND \"metodo_pago\" IS NULL AND \"fecha_cancelacion_utc\" IS NOT NULL AND length(btrim(coalesce(\"motivo_cancelacion\", ''))) > 0)");

            migrationBuilder.CreateIndex(
                name: "IX_productos_menu_activo_nombre",
                table: "productos_menu",
                columns: new[] { "esta_activo", "nombre" });

            migrationBuilder.AddCheckConstraint(
                name: "CK_productos_menu_nombre_no_vacio",
                table: "productos_menu",
                sql: "length(btrim(\"nombre\")) > 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_productos_menu_precio_positivo",
                table: "productos_menu",
                sql: "\"precio_monto\" > 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_detalles_venta_cantidad_positiva",
                table: "detalles_venta",
                sql: "\"cantidad\" > 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_detalles_venta_nombre_historico_no_vacio",
                table: "detalles_venta",
                sql: "length(btrim(\"nombre_historico\")) > 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_detalles_venta_precio_unitario_positivo",
                table: "detalles_venta",
                sql: "\"precio_unitario_monto\" > 0");

            migrationBuilder.CreateIndex(
                name: "IX_outbox_mensajes_pendientes_ocurrido",
                table: "outbox_mensajes",
                columns: new[] { "procesado_en_utc", "ocurrido_en_utc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "outbox_mensajes");

            migrationBuilder.DropIndex(
                name: "IX_ventas_estado_fecha_creacion_utc",
                table: "ventas");

            migrationBuilder.DropCheckConstraint(
                name: "CK_ventas_cliente_id_no_vacio",
                table: "ventas");

            migrationBuilder.DropCheckConstraint(
                name: "CK_ventas_estado_valido",
                table: "ventas");

            migrationBuilder.DropCheckConstraint(
                name: "CK_ventas_fecha_cancelacion_posterior",
                table: "ventas");

            migrationBuilder.DropCheckConstraint(
                name: "CK_ventas_fecha_creacion_valida",
                table: "ventas");

            migrationBuilder.DropCheckConstraint(
                name: "CK_ventas_fecha_pago_posterior",
                table: "ventas");

            migrationBuilder.DropCheckConstraint(
                name: "CK_ventas_metodo_pago_valido",
                table: "ventas");

            migrationBuilder.DropCheckConstraint(
                name: "CK_ventas_numero_mesa_positivo",
                table: "ventas");

            migrationBuilder.DropCheckConstraint(
                name: "CK_ventas_transicion_estado_coherente",
                table: "ventas");

            migrationBuilder.DropIndex(
                name: "IX_productos_menu_activo_nombre",
                table: "productos_menu");

            migrationBuilder.DropCheckConstraint(
                name: "CK_productos_menu_nombre_no_vacio",
                table: "productos_menu");

            migrationBuilder.DropCheckConstraint(
                name: "CK_productos_menu_precio_positivo",
                table: "productos_menu");

            migrationBuilder.DropCheckConstraint(
                name: "CK_detalles_venta_cantidad_positiva",
                table: "detalles_venta");

            migrationBuilder.DropCheckConstraint(
                name: "CK_detalles_venta_nombre_historico_no_vacio",
                table: "detalles_venta");

            migrationBuilder.DropCheckConstraint(
                name: "CK_detalles_venta_precio_unitario_positivo",
                table: "detalles_venta");

            migrationBuilder.DropColumn(
                name: "fecha_cancelacion_utc",
                table: "ventas");

            migrationBuilder.DropColumn(
                name: "motivo_cancelacion",
                table: "ventas");

            migrationBuilder.DropColumn(
                name: "xmin",
                table: "ventas");
        }
    }
}
