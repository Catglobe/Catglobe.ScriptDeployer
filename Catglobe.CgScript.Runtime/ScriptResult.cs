namespace Catglobe.CgScript.Runtime;

/// <summary>
/// Result of a script run
/// </summary>
/// <typeparam name="T">Return type from script</typeparam>
public class ScriptResult<T>
{
   /// <summary>
   /// Error if any
   /// </summary>
   public object? Error { get; set; }
   /// <summary>
   /// Value if any
   /// </summary>
   public T?      Value { get; set; }

   /// <summary>
   /// Get the value or throw an exception if there was an error
   /// </summary>
   /// <returns>The object the script returned</returns>
   public T GetValueOrThrowError()
   {
      if (Error is not null) throw new ScriptException(Error.ToString());
      if (Value is null) throw new ScriptException("No value or error returned.");
      return Value;
   }
}

/// <summary>
/// An error occured during the execution or parsing of the return value
/// </summary>
public class ScriptException(string? message) : Exception(message);
