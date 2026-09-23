using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TimeClockSystem.Web.Areas.ConsultaAsistencias.Models;
using TimeClockSystem.Web.Domain;
using TimeClockSystem.Web.Infrastructure.Data;
using TimeClockSystem.Web.Infrastructure.Identity;

namespace TimeClockSystem.Web.Areas.ConsultaAsistencias.Controllers;

[Area("ConsultaAsistencias")]
[Authorize(Roles = Roles.Administrador)]
public class ConsultaAsistenciasController(ApplicationDbContext db) : Controller
{
    public async Task<IActionResult> Index(int? empleadoId, int? centroTrabajoId, DateOnly? fecha)
    {
        await CargarListasAsync();

        var query = db.Marcas
            .Include(m => m.Empleado)
            .ThenInclude(e => e!.CentroTrabajo)
            .AsQueryable();

        if (empleadoId is not null)
        {
            query = query.Where(m => m.EmpleadoId == empleadoId);
        }
        if (centroTrabajoId is not null)
        {
            query = query.Where(m => m.Empleado!.CentroTrabajoId == centroTrabajoId);
        }
        if (fecha is not null)
        {
            var inicio = fecha.Value.ToDateTime(TimeOnly.MinValue);
            var fin = inicio.AddDays(1);
            query = query.Where(m => m.Timestamp >= inicio && m.Timestamp < fin);
        }

        var marcas = await query.OrderByDescending(m => m.Timestamp).Take(200).ToListAsync();

        var festivos = (await db.DiasFestivos.Select(f => f.Fecha).ToListAsync()).ToHashSet();

        var resultados = new List<FilaConsultaViewModel>();
        foreach (var marca in marcas)
        {
            IndicadorPuntualidad? puntualidad = null;
            if (marca.Tipo == TipoMarca.Entrada && marca.Estado == EstadoMarca.Valida)
            {
                var fechaMarca = DateOnly.FromDateTime(marca.Timestamp.ToLocalTime());
                var asignacion = await db.AsignacionesTurno
                    .Include(a => a.Turno)
                    .FirstOrDefaultAsync(a => a.EmpleadoId == marca.EmpleadoId && a.Fecha == fechaMarca);
                puntualidad = PuntualidadCalculator.Calcular(marca, asignacion?.Turno);
            }

            resultados.Add(new FilaConsultaViewModel
            {
                NumeroEmpleado = marca.Empleado?.NumeroEmpleado ?? string.Empty,
                NombreEmpleado = marca.Empleado?.Nombre ?? string.Empty,
                CentroTrabajo = marca.Empleado?.CentroTrabajo?.Nombre ?? string.Empty,
                Timestamp = marca.Timestamp,
                Tipo = marca.Tipo,
                Estado = marca.Estado,
                MotivoRechazo = marca.MotivoRechazo,
                Puntualidad = puntualidad,
                EsDiaFestivo = festivos.Contains(DateOnly.FromDateTime(marca.Timestamp.ToLocalTime())),
            });
        }

        return View(new ConsultaAsistenciasViewModel
        {
            Filtro = new FiltroConsultaViewModel { EmpleadoId = empleadoId, CentroTrabajoId = centroTrabajoId, Fecha = fecha },
            Resultados = resultados,
        });
    }

    private async Task CargarListasAsync()
    {
        ViewBag.Empleados = new SelectList(await db.Empleados.OrderBy(e => e.NumeroEmpleado).ToListAsync(), "Id", "NumeroEmpleado");
        ViewBag.CentrosTrabajo = new SelectList(await db.CentrosTrabajo.OrderBy(c => c.Nombre).ToListAsync(), "Id", "Nombre");
    }
}
