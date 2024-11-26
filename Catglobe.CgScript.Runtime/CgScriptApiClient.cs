using System.Diagnostics.CodeAnalysis;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using Catglobe.CgScript.Common;

namespace Catglobe.CgScript.Runtime;

/// <summary>
/// Client to execute scripts on the server.
/// </summary>
public interface ICgScriptApiClient
{
   /// <summary>
   /// Execute a script on the server.
   /// </summary>
   /// <param name="scriptName">Name of script to run</param>
   /// <param name="parameter">The parameter for the script</param>
   /// <param name="callJsonTypeInfo">Source generator parser for the parameter</param>
   /// <param name="resultJsonTypeInfo">Source generator parser for the result</param>
   /// <param name="cancellationToken">Token to monitor for cancellation requests</param>
   /// <returns>A results object that contains the message from the server</returns>
   public Task<ScriptResult<TR>> Execute<TP, TR>(string scriptName, TP parameter, JsonTypeInfo<TP> callJsonTypeInfo, JsonTypeInfo<TR> resultJsonTypeInfo, CancellationToken cancellationToken = default);

   /// <summary>
   /// Execute a script on the server.
   /// </summary>
   /// <param name="scriptName">Name of script to run</param>
   /// <param name="resultJsonTypeInfo">Source generator parser for the result</param>
   /// <param name="cancellationToken">Token to monitor for cancellation requests</param>
   /// <returns>A results object that contains the message from the server</returns>
   public Task<ScriptResult<TR>> Execute<TR>(string scriptName, JsonTypeInfo<TR> resultJsonTypeInfo, CancellationToken cancellationToken = default);

   /// <summary>
   /// Execute a script on the server.
   /// </summary>
   /// <remarks>
   /// Not recommended to use. Prefer a model that uses <see cref="Execute{TP,TR}(string,TP,JsonTypeInfo{TP},JsonTypeInfo{TR},CancellationToken)"/>.
   /// </remarks>
   /// <param name="scriptName">Name of script to run</param>
   /// <param name="parameter">The parameter for the script</param>
   /// <param name="options">Options that will tell us how to serialize parameters and deserialize result. Can be null</param>
   /// <param name="cancellationToken">Token to monitor for cancellation requests</param>
   /// <returns>A results object that contains the message from the server</returns>
   [RequiresUnreferencedCode("JSON")]
   public Task<ScriptResult<TR>> Execute<TP, TR>(string scriptName, TP parameter, JsonSerializerOptions? options = null, CancellationToken cancellationToken = default);

   /// <summary>
   /// Execute a script on the server.
   /// </summary>
   /// <remarks>
   /// Not recommended to use. Prefer a model that uses <see cref="Execute{TP,TR}(string,TP,JsonTypeInfo{TP},JsonTypeInfo{TR},CancellationToken)"/>.
   /// </remarks>
   /// <param name="scriptName">Name of script to run</param>
   /// <param name="parameters">The parameters for the script</param>
   /// <param name="options">Options that will tell us how to serialize parameters and deserialize result. Can be null</param>
   /// <param name="cancellationToken">Token to monitor for cancellation requests</param>
   /// <returns>A results object that contains the message from the server</returns>
   [RequiresUnreferencedCode("JSON")]
   public Task<ScriptResult<TR>> Execute<TR>(string scriptName, IReadOnlyCollection<object> parameters, JsonSerializerOptions? options = null, CancellationToken cancellationToken = default);

   /// <summary>
   /// Execute a script on the server.
   /// </summary>
   /// <remarks>
   /// Not recommended to use. Prefer a model that uses <see cref="Execute{TP,TR}(string,TP,JsonTypeInfo{TP},JsonTypeInfo{TR},CancellationToken)"/>.
   /// </remarks>
   /// <param name="scriptName">Name of script to run</param>
   /// <param name="options">Options that will tell us how to serialize parameters and deserialize result. Can be null</param>
   /// <param name="cancellationToken">Token to monitor for cancellation requests</param>
   /// <returns>A results object that contains the message from the server</returns>
   [RequiresUnreferencedCode("JSON")]
   public Task<ScriptResult<TR>> Execute<TR>(string scriptName, JsonSerializerOptions? options = null, CancellationToken cancellationToken = default);

}

internal class CgScriptApiClient(HttpClient httpClient, IScriptMapping map) : ICgScriptApiClient
{
   public async Task<ScriptResult<TR>> Execute<TP, TR>(string scriptName, TP parameter, JsonTypeInfo<TP> callJsonTypeInfo, JsonTypeInfo<TR> resultJsonTypeInfo, CancellationToken cancellationToken) =>
      await ParseResponse(await httpClient.PostAsJsonAsync(await GetPath(scriptName), parameter, callJsonTypeInfo, cancellationToken).ConfigureAwait(false), resultJsonTypeInfo, cancellationToken);

   public async Task<ScriptResult<TR>> Execute<TR>(string scriptName, JsonTypeInfo<TR> resultJsonTypeInfo, CancellationToken cancellationToken = default) =>
      await ParseResponse(await httpClient.PostAsync(await GetPath(scriptName, "?expandParameters=true"), null, cancellationToken).ConfigureAwait(false), resultJsonTypeInfo, cancellationToken);

   private static async Task<ScriptResult<TR>> ParseResponse<TR>(HttpResponseMessage call, JsonTypeInfo<TR> resultJsonTypeInfo, CancellationToken cancellationToken)
   {
      var jsonTypeInfo = JsonMetadataServices.CreateValueInfo<ScriptResult<TR>>(new(), new ScriptResultConverterWithTypeInfo<TR>(resultJsonTypeInfo));
      call.EnsureSuccessStatusCode();
      var result = await call.Content.ReadFromJsonAsync(jsonTypeInfo, cancellationToken).ConfigureAwait(false);
      return result ?? throw new IOException("Could not deserialize result");
   }

   [RequiresUnreferencedCode("JSON")]
   public async Task<ScriptResult<TR>> Execute<TP, TR>(string scriptName, TP parameter, JsonSerializerOptions? options, CancellationToken cancellationToken = default) =>
      await ParseResponse<TR>(await httpClient.PostAsJsonAsync(await GetPath(scriptName), parameter, options, cancellationToken).ConfigureAwait(false), options, cancellationToken);

   [RequiresUnreferencedCode("JSON")]
   public async Task<ScriptResult<TR>> Execute<TR>(string scriptName, IReadOnlyCollection<object> parameters, JsonSerializerOptions? options = null, CancellationToken cancellationToken = default) =>
      await ParseResponse<TR>(await httpClient.PostAsJsonAsync(await GetPath(scriptName, "?expandParameters=true"), parameters, options, cancellationToken).ConfigureAwait(false), options, cancellationToken);

   [RequiresUnreferencedCode("JSON")]
   public async Task<ScriptResult<TR>> Execute<TR>(string scriptName, JsonSerializerOptions? options = null, CancellationToken cancellationToken = default) =>
      await ParseResponse<TR>(await httpClient.PostAsync(await GetPath(scriptName), null, cancellationToken).ConfigureAwait(false), options, cancellationToken);

   [RequiresUnreferencedCode("JSON")]
   private static async Task<ScriptResult<TR>> ParseResponse<TR>(HttpResponseMessage call, JsonSerializerOptions? options, CancellationToken cancellationToken)
   {
      var retOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web) { Converters = { new ScriptResultConverterFactory<TR>(options) } };
      call.EnsureSuccessStatusCode();
      var result = (ScriptResult<TR>?)await call.Content.ReadFromJsonAsync(typeof(ScriptResult<TR>), retOptions, cancellationToken).ConfigureAwait(false);
      return result ?? throw new IOException("Could not deserialize result");
   }

   private async Task<string> GetPath(string scriptName, string? additionalParameters = null)
   {
      await map.EnsureDownloaded();
      return $"run/{map.GetIdOf(scriptName)}{additionalParameters ?? ""}";
   }

}
