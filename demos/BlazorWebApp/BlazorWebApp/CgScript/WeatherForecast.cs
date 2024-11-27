using System.Text.Json;
using System.Text.Json.Serialization;

namespace BlazorWebApp.CgScript;

internal record WeatherForecastParameters(string City, int NumberOfDays);

internal class WeatherForecast
{
   public DateOnly Date         { get; init; }
   public int      TemperatureC { get; init; }
   public string?  Summary      { get; init; }
   [JsonIgnore]
   public int      TemperatureF => 32 + (int)(TemperatureC / 0.5556);

}

[JsonSerializable(typeof(WeatherForecastParameters))]
[JsonSerializable(typeof(WeatherForecast[]))]
[JsonSourceGenerationOptions(JsonSerializerDefaults.Web)]
internal partial class WeatherForecastSerializer : JsonSerializerContext;
