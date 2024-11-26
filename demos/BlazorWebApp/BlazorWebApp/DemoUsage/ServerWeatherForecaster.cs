using BlazorWebApp.CgScript;
using Catglobe.CgScript.Runtime;

namespace BlazorWebApp.DemoUsage;

internal interface IWeatherForecaster
{
   Task<IEnumerable<WeatherForecast>> GetWeatherForecastAsync();
}

internal sealed class ServerWeatherForecaster(ICgScriptApiClient cgScriptClient) : IWeatherForecaster
{
   public async Task<IEnumerable<WeatherForecast>> GetWeatherForecastAsync()
   {
      var response = await cgScriptClient.Execute("WeatherForecast", new("Ho Chi Minh City", 3), WeatherForecastSerializer.Default.WeatherForecastParameters, WeatherForecastSerializer.Default.WeatherForecastArray);
      return response.GetValueOrThrowError();
   }
}


internal interface IPublicWeatherForecaster
{
   Task<IEnumerable<WeatherForecast>> GetWeatherForecastAsync();
}

internal sealed class ServerPublicWeatherForecaster(ICgScriptApiClient cgScriptClient) : IPublicWeatherForecaster
{
   public async Task<IEnumerable<WeatherForecast>> GetWeatherForecastAsync()
   {
      var response = await cgScriptClient.Execute("ThePublicWeatherForecast", new("Ho Chi Minh City", 3), WeatherForecastSerializer.Default.WeatherForecastParameters, WeatherForecastSerializer.Default.WeatherForecastArray);
      return response.GetValueOrThrowError();
   }
}