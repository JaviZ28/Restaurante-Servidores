using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RestauranteVentas.Infrastructure.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class ProcesamientoSeguroOutbox : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_outbox_mensajes_pendientes_ocurrido",
                table: "outbox_mensajes");

            migrationBuilder.AddColumn<DateTime>(
                name: "bloqueado_hasta_utc",
                table: "outbox_mensajes",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "token_bloqueo",
                table: "outbox_mensajes",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_outbox_mensajes_pendientes_ocurrido",
                table: "outbox_mensajes",
                columns: new[] { "procesado_en_utc", "bloqueado_hasta_utc", "ocurrido_en_utc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_outbox_mensajes_pendientes_ocurrido",
                table: "outbox_mensajes");

            migrationBuilder.DropColumn(
                name: "bloqueado_hasta_utc",
                table: "outbox_mensajes");

            migrationBuilder.DropColumn(
                name: "token_bloqueo",
                table: "outbox_mensajes");

            migrationBuilder.CreateIndex(
                name: "IX_outbox_mensajes_pendientes_ocurrido",
                table: "outbox_mensajes",
                columns: new[] { "procesado_en_utc", "ocurrido_en_utc" });
        }
    }
}
