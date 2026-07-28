using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RestauranteVentas.Infrastructure.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class AgregarRevisionConcurrenciaVenta : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "revision",
                table: "ventas",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "revision",
                table: "ventas");
        }
    }
}
