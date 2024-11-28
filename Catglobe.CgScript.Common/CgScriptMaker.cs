using System.Text;

namespace Catglobe.CgScript.Common;

/// <summary>
/// Create a script from a script definition and a mapping
/// </summary>
/// <param name="environment">Run time environment, must be "Deployment", "Staging" or "Production"</param>
/// <param name="definitions">List of scripts</param>
/// <param name="map">The mapping from name to id</param>
public class CgScriptMaker(string environment, IReadOnlyDictionary<string, IScriptDefinition> definitions, IScriptMapping map) : BaseCgScriptMaker(environment, definitions)
{
   ///<inheritdoc/>
   protected override string GetPreamble(IScriptDefinition scriptDef) => $"//THIS FILE IS UPLOADED from {scriptDef.ScriptName}.cgs! ANY CHANGES DONE HERE WILL BE LOST ON NEXT UPLOAD!\n\n\n\n\n\n";

   ///<inheritdoc/>
   protected override Task Generate(IScriptDefinition scriptDef, StringBuilder finalScript) =>
      ProcessScriptReferences(scriptDef, finalScript,  (_, calledScriptName) => {
         try
         {
            finalScript.Append(map.GetIdOf(calledScriptName));
         } catch (KeyNotFoundException)
         {
            throw new KeyNotFoundException($"Script '{scriptDef.ScriptName}' calls unknown script '{calledScriptName}'.");
         }
         return Task.CompletedTask;
      }, new object());
}

