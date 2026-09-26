namespace TimeClockSystem.Domain;

/// <summary>
/// Valida que una coordenada de marcaje esté dentro del geofence de un centro de trabajo (FR-006).
/// </summary>
public static class GeofenceValidator
{
    private const double RadioTierraMetros = 6_371_000;

    public static bool EstaDentroDelGeofence(CentroTrabajo centro, double latitud, double longitud)
    {
        var distancia = DistanciaHaversineMetros(centro.Latitud, centro.Longitud, latitud, longitud);
        return distancia <= centro.RadioMetros;
    }

    /// <summary>
    /// Distancia en metros entre dos coordenadas geográficas (fórmula de Haversine).
    /// </summary>
    public static double DistanciaHaversineMetros(double lat1, double lon1, double lat2, double lon2)
    {
        var dLat = GradosARadianes(lat2 - lat1);
        var dLon = GradosARadianes(lon2 - lon1);

        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(GradosARadianes(lat1)) * Math.Cos(GradosARadianes(lat2)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        return RadioTierraMetros * c;
    }

    private static double GradosARadianes(double grados) => grados * Math.PI / 180;
}
