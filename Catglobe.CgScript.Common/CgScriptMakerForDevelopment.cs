using System.Text;

namespace Catglobe.CgScript.Common;

/// <summary>
/// Create a script from a script definition and a mapping
/// </summary>
/// <param name="definitions">List of scripts</param>
/// <param name="impersonationMapping">Mapping of impersonations to development time users. 0 maps to developer account, missing maps to original</param>
public class CgScriptMakerForDevelopment(IReadOnlyDictionary<string, IScriptDefinition> definitions, Dictionary<uint, uint>? impersonationMapping) : BaseCgScriptMaker("Development", definitions)
{
   private readonly IReadOnlyDictionary<string, IScriptDefinition> _definitions = definitions;
   private readonly string _uniqueId = Guid.NewGuid().ToString("N");

   ///<inheritdoc/>
   protected override string GetPreamble(IScriptDefinition scriptDef) => "";

   ///<inheritdoc/>
   protected override async Task Generate(IScriptDefinition scriptDef, StringBuilder finalScript)
   {
      //place to put all the called scripts
      var scriptDefs = new StringBuilder();
      var visited    = new HashSet<IScriptDefinition>() {scriptDef};
      // process current script, which is going to make it a "clean" script
      await ProcessScriptReferences(scriptDef, finalScript, ProcessSingleReference, finalScript);
      //but we need that clean script as a string script to dynamically invoke it
      var outerScriptRef = GetScriptRef(scriptDef);
      ConvertScriptToStringScript(scriptDef, outerScriptRef, finalScript);
      //the whole script was moved to scriptDefs, so clear it and then re-add all definitions
      finalScript.Clear();
      finalScript.Append(scriptDefs);
      //and finally invoke the called script as if it was called
      finalScript.AppendLine($"{outerScriptRef}.Invoke(Workflow_getParameters());");
      return;

      void ConvertScriptToStringScript(IScriptDefinition scriptDefinition, string name, StringBuilder stringBuilder)
      {
         stringBuilder.Replace(@"\", @"\\").Replace("\"", "\\\"").Replace("\n", "\\n").Replace("\r", "\\r");
         stringBuilder.Insert(0, $"WorkflowScript {name} = new WorkflowScript(\"");
         stringBuilder.AppendLine("\", false);");
         stringBuilder.AppendLine($"{name}.DynamicScriptName = \"{scriptDefinition.ScriptName}\";");
         stringBuilder.AppendLine($"Workflow_setGlobal(\"{name}\", {name});");
         if (scriptDefinition.Impersonation is { } imp)
         {
            impersonationMapping?.TryGetValue(imp, out imp);
            if (imp == 0)
               stringBuilder.AppendLine($"{name}.ImpersonatedUser = getCurrentUserUniqueId();");
            else
               stringBuilder.AppendLine($"{name}.ImpersonatedUser = {imp};");
         }
         scriptDefs.Append(stringBuilder);

      }

      async Task ProcessSingleReference(StringBuilder curScript, string calledScriptName)
      {
         if (!_definitions.TryGetValue(calledScriptName, out var def)) throw new KeyNotFoundException($"Script '{scriptDef.ScriptName}' calls unknown script '{calledScriptName}'.");

         var scriptRef = GetScriptRef(def);
         curScript.Append($"Workflow_getGlobal(\"{scriptRef}\")");

         if (!visited.Add(def))
            return;

         var subSb = new StringBuilder();
         await ProcessScriptReferences(def, subSb, ProcessSingleReference, subSb);
         ConvertScriptToStringScript(def, scriptRef, subSb);
      }

      string GetScriptRef(IScriptDefinition scriptDefinition) => scriptDefinition.ScriptName.Replace("/", "__") + "__" + _uniqueId;
   }
}

