using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TimeClockSystem.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class Modulo6_Auditoria : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RegistrosAuditoria",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Evento = table.Column<int>(type: "INTEGER", nullable: false),
                    UsuarioOEmpleadoId = table.Column<string>(type: "TEXT", maxLength: 256, nullable: false),
                    Detalle = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    TimestampUtc = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RegistrosAuditoria", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RegistrosAuditoria");
        }
    }
}
