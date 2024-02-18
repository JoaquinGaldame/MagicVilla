using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace MagicVilla_API.Migrations
{
    /// <inheritdoc />
    public partial class InsertarTablasVilla : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Villas",
                columns: new[] { "Id", "Amenidad", "Detalle", "FechaActualizacion", "FechaCreacion", "ImagenUrl", "Nombre", "Ocupantes", "Tarifa" },
                values: new object[,]
                {
                    { 1, "", "450 kilometros", new DateTime(2024, 2, 18, 12, 45, 12, 571, DateTimeKind.Local).AddTicks(6037), new DateTime(2024, 2, 18, 12, 45, 12, 571, DateTimeKind.Local).AddTicks(6026), "", "Villa Langostura", 5, 200.0 },
                    { 2, "", "320 kilometros", new DateTime(2024, 2, 18, 12, 45, 12, 571, DateTimeKind.Local).AddTicks(6039), new DateTime(2024, 2, 18, 12, 45, 12, 571, DateTimeKind.Local).AddTicks(6038), "", "Villa Mercedes", 4, 150.0 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Villas",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Villas",
                keyColumn: "Id",
                keyValue: 2);
        }
    }
}
