using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RestauranteVentas.Infrastructure.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "productos_menu",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    precio_monto = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    esta_activo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_productos_menu", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "ventas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    cliente_id = table.Column<Guid>(type: "uuid", nullable: true),
                    numero_mesa = table.Column<int>(type: "integer", nullable: true),
                    estado = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    fecha_creacion_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    fecha_pago_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    metodo_pago = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ventas", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "detalles_venta",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    producto_menu_id = table.Column<Guid>(type: "uuid", nullable: false),
                    nombre_historico = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    precio_unitario_monto = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    cantidad = table.Column<int>(type: "integer", nullable: false),
                    venta_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_detalles_venta", x => x.id);
                    table.ForeignKey(
                        name: "FK_detalles_venta_ventas_venta_id",
                        column: x => x.venta_id,
                        principalTable: "ventas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_detalles_venta_producto_menu_id",
                table: "detalles_venta",
                column: "producto_menu_id");

            migrationBuilder.CreateIndex(
                name: "IX_detalles_venta_venta_id",
                table: "detalles_venta",
                column: "venta_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "detalles_venta");

            migrationBuilder.DropTable(
                name: "productos_menu");

            migrationBuilder.DropTable(
                name: "ventas");
        }
    }
}
