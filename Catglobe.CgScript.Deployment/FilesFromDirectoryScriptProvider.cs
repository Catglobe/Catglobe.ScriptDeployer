using Catglobe.CgScript.Common;
using Microsoft.Extensions.Options;

namespace Catglobe.CgScript.Deployment;

/// <summary>
/// Default implementation of <see cref="IScriptDefinition"/> that fetches *.cgs scripts from a file folder.
/// Files in sub folders will get script names that include the folder name. Example: MyFolder\MyScript@123.cgs will get the scriptName MyFolder/MyScript.
/// </summary>
public class FilesFromDirectoryScriptProvider(IOptions<DeploymentOptions> options) : IScriptProvider
{
   ///<inheritdoc/>
   public Task<IReadOnlyDictionary<string, IScriptDefinition>> GetAll()
   {
      var directory = options.Value.ScriptFolder;
      return Task.FromResult<IReadOnlyDictionary<string, IScriptDefinition>>
         (Directory.EnumerateFiles(directory, "*.cgs", SearchOption.AllDirectories)
                   .Select(file => new ScriptFromFileOnDisk(file, Path.GetRelativePath(directory, file)))
                   .ToDictionary(script => script.ScriptName, IScriptDefinition (script) => script));
   }
}
