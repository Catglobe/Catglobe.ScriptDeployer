namespace Catglobe.CgScript.Common;

/// <summary>
/// 
/// </summary>
public interface IScriptProvider
{
   /// <summary>
   /// Return all known scripts
   /// </summary>
   Task<IReadOnlyDictionary<string, IScriptDefinition>> GetAll();
}