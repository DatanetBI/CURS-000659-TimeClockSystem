using TimeClockSystem.Domain;

namespace TimeClockSystem.Api.Tests.Unit;

public class GeofenceValidatorTests
{
    private static readonly CentroTrabajo Centro = new()
    {
        Nombre = "Centro de prueba",
        Latitud = 19.4326,
        Longitud = -99.1332,
        RadioMetros = 150,
    };

    [Fact]
    public void EstaDentroDelGeofence_MismaCoordenada_RegresaTrue()
    {
        Assert.True(GeofenceValidator.EstaDentroDelGeofence(Centro, Centro.Latitud, Centro.Longitud));
    }

    [Fact]
    public void EstaDentroDelGeofence_CoordenadaLejana_RegresaFalse()
    {
        // ~5 km al norte, muy fuera del radio de 150 m.
        Assert.False(GeofenceValidator.EstaDentroDelGeofence(Centro, Centro.Latitud + 0.05, Centro.Longitud));
    }

    [Fact]
    public void DistanciaHaversineMetros_MismoPunto_RegresaCero()
    {
        var distancia = GeofenceValidator.DistanciaHaversineMetros(19.4326, -99.1332, 19.4326, -99.1332);
        Assert.Equal(0, distancia, precision: 3);
    }
}
