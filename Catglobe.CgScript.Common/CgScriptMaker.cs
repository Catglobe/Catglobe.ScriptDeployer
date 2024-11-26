using System.Text;
using System.Text.RegularExpressions;

namespace Catglobe.CgScript.Common;

/// <summary>
/// Create a script from a script definition and a mapping
/// </summary>
/// <param name="environment">Run time environment, must be "Deployment", "Staging" or "Production"</param>
/// <param name="definitions">List of scripts</param>
/// <param name="map">The mapping from name to id</param>
public partial class CgScriptMaker(string environment, IReadOnlyDictionary<string, IScriptDefinition> definitions, IScriptMapping map)
{
   /// <summary>
   /// Get the content of a script with all references replaced with the actual content.
   /// In development mode, this will be a self-contained script that can be dynamically executed.
   /// In production/staging mode, this will be a script that can be stored in the Catglobe site database.
   /// </summary>
   public async Task<string> GetContent(string scriptName)
   {
      var scriptDef     = definitions[scriptName];
      var preamble      = environment != "Development" ? $"//THIS FILE IS UPLOADED from {scriptDef.ScriptName}.cgs! ANY CHANGES DONE HERE WILL BE LOST ON NEXT UPLOAD!\n\n\n\n\n\n" : "";
      var stringBuilder = new StringBuilder(preamble);
      await Replace(scriptDef, [], stringBuilder);
      return stringBuilder.ToString();
   }

   [GeneratedRegex(@"#IF\s+(?<env>Development|Production|Staging)\b(?<script>.*?)\b#ENDIF", RegexOptions.Singleline, -1)]
   private static partial Regex EnvironmentRegex();
   [GeneratedRegex("""new WorkflowScript\s*\(\s*"(?<scriptName>[^"]+)"\s*\)""", RegexOptions.Singleline, -1)]
   private static partial Regex FindWorkflowReferences();

   private async Task Replace(IScriptDefinition scriptDef, List<string> visited, StringBuilder finalScript)
   {
      if (visited.Contains(scriptDef.ScriptName)) throw new LoopDetectedException($"Loop detected while calling: {scriptDef.ScriptName}\nCall sequence:{string.Join(" - ", visited)}");
      visited.Add(scriptDef.ScriptName);

      string       rawScript;
      using (var reader = new StreamReader(await scriptDef.Content)) rawScript = await reader.ReadToEndAsync();

      rawScript = EnvironmentRegex().Replace(rawScript, e => e.Groups["env"].ValueSpan == environment ? e.Groups["script"].Value : string.Empty);

      var lastIdx = 0;
      foreach (Match match in FindWorkflowReferences().Matches(rawScript))
      {
         //add anything before the match to the sb
         finalScript.Append(rawScript.AsSpan(lastIdx, match.Index - lastIdx));
         //add the replacement to the sb
         finalScript.Append("new WorkflowScript(");
         var calledScriptName = match.Groups["scriptName"].Value;
         if (environment=="Development")
         {
            finalScript.Append('"');
            var subSb = new StringBuilder();
            if (!definitions.TryGetValue(calledScriptName, out var def))
               throw new KeyNotFoundException($"Script '{scriptDef.ScriptName}' calls unknown script '{calledScriptName}'.");
            await Replace(def, [.. visited], subSb);
            subSb.Replace(@"\", @"\\").Replace("\"", "\\\"").Replace("\n", "\\n").Replace("\r", "\\r");
            finalScript.Append(subSb);
            finalScript.Append("\", false");
         }
         else
         {
            try
            {
               finalScript.Append(map.GetIdOf(calledScriptName));
            } catch (KeyNotFoundException)
            {
               throw new KeyNotFoundException($"Script '{scriptDef.ScriptName}' calls unknown script '{calledScriptName}'.");
            }
         }
         finalScript.Append(')');
         lastIdx = match.Index + match.Length;
      }
      //add rest
      finalScript.Append(rawScript.AsSpan(lastIdx));
   }
}

/// <summary>
/// Thrown if a script ends up calling itself. This is only thrown in development mode
/// </summary>
public class LoopDetectedException(string message) : Exception(message);
