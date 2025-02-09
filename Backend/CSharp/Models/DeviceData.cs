namespace Models;

public class DeviceData : Common
{
    public string DeviceID { get; set; }
    public Device Device { get; set; }

    // Sensor data
    public decimal? Temperature { get; set; }  // °C
    public decimal? Humidity { get; set; }     // %
    public decimal? CO2 { get; set; }          // ppm
    public decimal? Pressure { get; set; }     // hPa (hectopascal)
    public decimal? LightLevel { get; set; }   // lux

    public decimal AverageMeasurement(decimal[] measurements)
    {
        return measurements.Average();
    }

    public decimal MedianMeasurement(decimal[] measurements)
    {
        Array.Sort(measurements);
        int middle = measurements.Length / 2;
        return measurements[middle];
    }
}