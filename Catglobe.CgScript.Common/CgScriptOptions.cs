namespace Catglobe.CgScript.Common;

/// <summary>
/// Options for the CgScript execution
/// </summary>
public class CgScriptOptions
{
   /// <summary>
   /// Which site are we running on
   /// </summary>
   public string Site { get; set; } = null!;

   /// <summary>
   /// Which root folder are we running from
   /// </summary>
   public int FolderResourceId { get; set; }

}
