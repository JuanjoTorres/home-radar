using NetTopologySuite.Geometries;

namespace HomeRadar.Domain.ValueObjects;

public static class UtmProjection
{
    public const int Wgs84Srid = 4326;
    public const int Utm31NSrid = 25831;

    private const double SemiMajorAxis = 6378137.0;
    private const double Flattening = 1 / 298.257223563;
    private const double ScaleFactor = 0.9996;
    private const double FalseEasting = 500000.0;
    private const double CentralMeridianDegrees = 3.0;

    public static Point ToUtm31N(Point wgs84)
    {
        var (easting, northing) = ToUtm31N(wgs84.Y, wgs84.X);
        return new Point(easting, northing) { SRID = Utm31NSrid };
    }

    public static (double Easting, double Northing) ToUtm31N(double latitudeDegrees, double longitudeDegrees)
    {
        var e2 = Flattening * (2 - Flattening);
        var ePrime2 = e2 / (1 - e2);
        var phi = DegreesToRadians(latitudeDegrees);
        var lambda = DegreesToRadians(longitudeDegrees - CentralMeridianDegrees);

        var n = SemiMajorAxis / Math.Sqrt(1 - e2 * Math.Sin(phi) * Math.Sin(phi));
        var t = Math.Tan(phi) * Math.Tan(phi);
        var c = ePrime2 * Math.Cos(phi) * Math.Cos(phi);
        var a = Math.Cos(phi) * lambda;

        var m = SemiMajorAxis * (
            (1 - e2 / 4 - 3 * e2 * e2 / 64 - 5 * e2 * e2 * e2 / 256) * phi
            - (3 * e2 / 8 + 3 * e2 * e2 / 32 + 45 * e2 * e2 * e2 / 1024) * Math.Sin(2 * phi)
            + (15 * e2 * e2 / 256 + 45 * e2 * e2 * e2 / 1024) * Math.Sin(4 * phi)
            - (35 * e2 * e2 * e2 / 3072) * Math.Sin(6 * phi));

        var easting = FalseEasting + ScaleFactor * n * (a + (1 - t + c) * a * a * a / 6 + (5 - 18 * t + t * t + 72 * c - 58 * ePrime2) * Math.Pow(a, 5) / 120);
        var northing = ScaleFactor * (m + n * Math.Tan(phi) * (a * a / 2 + (5 - t + 9 * c + 4 * c * c) * Math.Pow(a, 4) / 24 + (61 - 58 * t + t * t + 600 * c - 330 * ePrime2) * Math.Pow(a, 6) / 720));
        return (easting, northing);
    }

    private static double DegreesToRadians(double degrees) => degrees * Math.PI / 180.0;
}
