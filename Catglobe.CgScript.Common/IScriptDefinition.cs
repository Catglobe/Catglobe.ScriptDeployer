namespace Catglobe.CgScript.Common;

/// <summary>
/// The content and metadata of a script
/// </summary>
public interface IScriptDefinition
{
   /// <summary>
   /// The name of the script as it is known in CgScript
   /// </summary>
   public string ScriptName { get; }
   /// <summary>
   /// The actual script
   /// </summary>
   public Task<Stream> Content { get; }
   /// <summary>
   /// The resource id of the user that the script run as
   /// </summary>
   public uint? Impersonation { get; }
   /// <summary>
   /// If true, the script can be run without a user being logged in
   /// </summary>
   bool AllowExecuteWithoutLogin { get; }
}
