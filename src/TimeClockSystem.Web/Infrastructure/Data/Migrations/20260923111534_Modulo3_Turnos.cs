using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TimeClockSystem.Web.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class Modulo3_Turnos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Turnos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nombre = table.Column<string>(type: "TEXT", maxLength: 80, nullable: false),
                    Tipo = table.Column<int>(type: "INTEGER", nullable: false),
                    HoraEntrada = table.Column<TimeSpan>(type: "TEXT", nullable: false),
                    HoraSalida = table.Column<TimeSpan>(type: "TEXT", nullable: false),
                    DuracionRecesoMinutos = table.Column<int>(type: "INTEGER", nullable: false),
                    ToleranciaMinutos = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Turnos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AsignacionesTurno",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    EmpleadoId = table.Column<int>(type: "INTEGER", nullable: false),
                    TurnoId = table.Column<int>(type: "INTEGER", nullable: false),
                    Fecha = table.Column<DateOnly>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AsignacionesTurno", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AsignacionesTurno_Empleados_EmpleadoId",
                        column: x => x.EmpleadoId,
                        principalTable: "Empleados",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AsignacionesTurno_Turnos_TurnoId",
                        column: x => x.TurnoId,
                        principalTable: "Turnos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AsignacionesTurno_EmpleadoId",
                table: "AsignacionesTurno",
                column: "EmpleadoId");

            migrationBuilder.CreateIndex(
                name: "IX_AsignacionesTurno_TurnoId",
                table: "AsignacionesTurno",
                column: "TurnoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AsignacionesTurno");

            migrationBuilder.DropTable(
                name: "Turnos");
        }
    }
}
