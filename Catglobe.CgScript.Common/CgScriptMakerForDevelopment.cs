using System.Text;

namespace Catglobe.CgScript.Common;

/// <summary>
/// Create a script from a script definition and a mapping
/// </summary>
/// <param name="definitions">List of scripts</param>
public class CgScriptMakerForDevelopment(IReadOnlyDictionary<string, IScriptDefinition> definitions) : BaseCgScriptMaker("Development", definitions)
{
   private readonly IReadOnlyDictionary<string, IScriptDefinition> _definitions = definitions;

   ///<inheritdoc/>
   protected override string GetPreamble(IScriptDefinition scriptDef) => "";

   ///<inheritdoc/>
   protected override Task Generate(IScriptDefinition scriptDef, StringBuilder finalScript)
   {
      return ProcessScriptReferences(scriptDef, finalScript, ProcessSingleReference, new List<string>());

      async Task ProcessSingleReference(List<string> visited, string calledScriptName)
      {
         if (visited.Contains(scriptDef.ScriptName)) throw new LoopDetectedException($"Loop detected while calling: {scriptDef.ScriptName}\nCall sequence:{string.Join(" - ", visited)}");

         finalScript.Append('"');
         var subSb = new StringBuilder();
         if (!_definitions.TryGetValue(calledScriptName, out var def)) throw new KeyNotFoundException($"Script '{scriptDef.ScriptName}' calls unknown script '{calledScriptName}'.");
         //we need to add to this one, otherwise 2 consecutive calls to same script would give the loop error when there is no loop
         var subVisited = new List<string>(visited) { scriptDef.ScriptName };
         await ProcessScriptReferences(def, subSb, ProcessSingleReference, subVisited);
         subSb.Replace(@"\", @"\\").Replace("\"", "\\\"").Replace("\n", "\\n").Replace("\r", "\\r");
         finalScript.Append(subSb);
         finalScript.Append("\", false");
      }
   }
}

/// <summary>
/// Thrown if a script ends up calling itself. This is only thrown in development mode
/// </summary>
public class LoopDetectedException(string message) : Exception(message);
