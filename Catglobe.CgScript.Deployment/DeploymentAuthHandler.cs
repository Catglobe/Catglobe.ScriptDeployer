using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;

namespace Catglobe.CgScript.Deployment;

internal partial class DeploymentAuthHandler(IOptions<DeploymentOptions> options) : DelegatingHandler
{
   private string? _accessToken;

   protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
   {
      if (_accessToken is null) await AcquireToken(cancellationToken);
      request.Headers.Authorization = new("Bearer", _accessToken);
      return await base.SendAsync(request, cancellationToken);
   }

   private async Task AcquireToken(CancellationToken cancellationToken)
   {
      var o          = options.Value;
      var httpClient = new HttpClient();
      var requestData = new Dictionary<string, string> {
         {"grant_type", "client_credentials"},
         {"client_id", o.ClientId},
         {"client_secret", o.ClientSecret},
         // ReSharper disable once StringLiteralTypo
         {"scope", "scriptdeployment:w"},
      };

      var requestMessage = new HttpRequestMessage(HttpMethod.Post, "/connect/token") {Content = new FormUrlEncodedContent(requestData), Headers = {Accept = {new("application/json")}}};

      var response = await httpClient.SendAsync(requestMessage, cancellationToken);
      response.EnsureSuccessStatusCode();

      var tokenResponse = await response.Content.ReadFromJsonAsync(Serializer.Default.TokenResponse, cancellationToken) ?? throw new IOException("Failed to obtain authorization");

      _accessToken = tokenResponse.AccessToken;
   }

   private class TokenResponse
   {
      [JsonPropertyName("access_token")] public string AccessToken { get; set; } = null!;
      [JsonPropertyName("expires_in")]   public int    ExpiresIn   { get; set; }
   }

   [JsonSerializable(typeof(TokenResponse))]
   [JsonSourceGenerationOptions(JsonSerializerDefaults.Web)]
   private partial class Serializer : JsonSerializerContext;
}
