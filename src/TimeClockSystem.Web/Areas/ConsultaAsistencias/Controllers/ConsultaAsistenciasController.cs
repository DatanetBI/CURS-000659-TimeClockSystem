using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using TimeClockSystem.Web.Areas.ConsultaAsistencias.Models;
using TimeClockSystem.Web.Infrastructure.ApiClients;
using TimeClockSystem.Web.Infrastructure.Auth;

namespace TimeClockSystem.Web.Areas.ConsultaAsistencias.Controllers;

[Area("ConsultaAsistencias")]
[Authorize(Roles = Roles.Administrador)]
public class ConsultaAsistenciasController(
    ConsultaAsistenciasApiClient consultaApi, EmpleadosApiClient empleadosApi, CentrosTrabajoApiClient centrosTrabajoApi) : Controller
{
    public async Task<IActionResult> Index(int? empleadoId, int? centroTrabajoId, DateOnly? fecha)
    {
        await CargarListasAsync();

        var filas = await consultaApi.ConsultarParaAdministradorAsync(empleadoId, centroTrabajoId, fecha);

        var resultados = filas.Select(fila => new FilaConsultaViewModel
        {
            NumeroEmpleado = fila.NumeroEmpleado,
            NombreEmpleado = fila.NombreEmpleado,
            CentroTrabajo = fila.CentroTrabajo,
            Timestamp = fila.Timestamp,
            Tipo = fila.Tipo,
            Estado = fila.Estado,
            MotivoRechazo = fila.MotivoRechazo,
            Puntualidad = fila.Puntualidad,
            EsDiaFestivo = fila.EsDiaFestivo,
        }).ToList();

        return View(new ConsultaAsistenciasViewModel
        {
            Filtro = new FiltroConsultaViewModel { EmpleadoId = empleadoId, CentroTrabajoId = centroTrabajoId, Fecha = fecha },
            Resultados = resultados,
        });
    }

    private async Task CargarListasAsync()
    {
        ViewBag.Empleados = new SelectList((await empleadosApi.ListarAsync()).OrderBy(e => e.NumeroEmpleado), "Id", "NumeroEmpleado");
        ViewBag.CentrosTrabajo = new SelectList((await centrosTrabajoApi.ListarAsync()).OrderBy(c => c.Nombre), "Id", "Nombre");
    }
}
