namespace Catglobe.CgScript.Deployment;

/// <summary>
/// Options for deployment
/// </summary>
public class DeploymentOptions
{
   /// <summary>
   /// The site to deploy to
   /// </summary>
   public Uri Authority { get; set; } = null!;
   /// <summary>
   /// The OAuth client id
   /// </summary>
   public string ClientId { get; set; } = null!;
   /// <summary>
   /// The OAuth client secret
   /// </summary>
   public string ClientSecret { get; set; } = null!;
   /// <summary>
   /// The folder to deploy to
   /// </summary>
   public int FolderResourceId { get; set; }
   /// <summary>
   /// The folder to search for scripts in. Used by <see cref="FilesFromDirectoryScriptProvider"/>.
   /// </summary>
   public string ScriptFolder { get; set; } = null!;
}
