namespace Catglobe.CgScript.Common;

/// <summary>
/// Map scriptName to id
/// </summary>
public interface IScriptMapping
{
   /// <summary>
   /// Map scriptName to id
   /// </summary>
   /// <exception cref="KeyNotFoundException">If <paramref name="scriptName"/> not found</exception>
   int GetIdOf(string scriptName);

   /// <summary>
   /// Ensure that the mapping is downloaded at least once
   /// </summary>
   ValueTask EnsureDownloaded();

   /// <summary>
   /// Force to download the mapping again next time
   /// </summary>
   void Reset();
}