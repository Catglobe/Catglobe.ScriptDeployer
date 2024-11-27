using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace Catglobe.CgScript.Runtime;

[RequiresUnreferencedCode("JSON")]
internal class ScriptResultConverterFactory<T>(JsonSerializerOptions? innerOptions) : JsonConverterFactory
{
   [RequiresUnreferencedCode("JSON")]
   private class ScriptResultConverter(JsonSerializerOptions? innerOptions) : JsonConverter<ScriptResult<T>>
   {
      public override ScriptResult<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
      {
         //json: {"result": T?, "error": object}
         if (reader.TokenType != JsonTokenType.StartObject) throw new JsonException();
         var result = new ScriptResult<T>();
         while (reader.Read())
         {
            if (reader.TokenType == JsonTokenType.EndObject) return result;
            if (reader.TokenType != JsonTokenType.PropertyName) throw new JsonException();
            var propName = reader.GetString();
            reader.Read();
            switch (propName)
            {
            case "error":
               {
                  result.Error = reader.TokenType switch {
                     JsonTokenType.None => null,
                     JsonTokenType.StartObject => JsonDocument.ParseValue(ref reader).RootElement,
                     JsonTokenType.StartArray => JsonDocument.ParseValue(ref reader).RootElement,
                     JsonTokenType.String => reader.GetString(),
                     JsonTokenType.Number => reader.GetDouble(),
                     JsonTokenType.True => true,
                     JsonTokenType.False => false,
                     JsonTokenType.Null => null,
                     _ => throw new JsonException()
                  };
                  break;
               }
            case "result":
               {
                  result.Value = reader.TokenType switch {
                     JsonTokenType.None => default,
                     JsonTokenType.Null => default,
                     _ => JsonSerializer.Deserialize<T>(ref reader, innerOptions),
                  };
                  break;
               }
            default: throw new JsonException();
            }
         }
         throw new JsonException();
      }
      public override void Write(Utf8JsonWriter writer, ScriptResult<T> value, JsonSerializerOptions options) => throw new NotSupportedException();
   }

   public override bool CanConvert(Type typeToConvert) => typeToConvert == typeof(ScriptResult<T>);

   public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options) => new ScriptResultConverter(innerOptions);
}
internal class ScriptResultConverterWithTypeInfo<T>(JsonTypeInfo<T> inner) : JsonConverter<ScriptResult<T>>
{
   public override ScriptResult<T> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
   {
      //json: {"result": T?, "error": object}
      if (reader.TokenType != JsonTokenType.StartObject) throw new JsonException();
      var result = new ScriptResult<T>();
      while (reader.Read())
      {
         if (reader.TokenType == JsonTokenType.EndObject) return result;
         if (reader.TokenType != JsonTokenType.PropertyName) throw new JsonException();
         if (reader.ValueTextEquals("error"u8))
         {
            reader.Read();
            result.Error = reader.TokenType switch {
               JsonTokenType.None => null,
               JsonTokenType.StartObject => JsonDocument.ParseValue(ref reader).RootElement,
               JsonTokenType.StartArray => JsonDocument.ParseValue(ref reader).RootElement,
               JsonTokenType.String => reader.GetString(),
               JsonTokenType.Number => reader.GetDouble(),
               JsonTokenType.True => true,
               JsonTokenType.False => false,
               JsonTokenType.Null => null,
               _ => throw new JsonException()
            };
         }
         else if (reader.ValueTextEquals("result"u8))
         {
            reader.Read();
            result.Value = reader.TokenType switch {
               JsonTokenType.None => default,
               JsonTokenType.Null => default,
               _ => JsonSerializer.Deserialize(ref reader, inner),
            };
         }
         else
         {
            throw new JsonException();
         }
      }
      throw new JsonException();
   }

   public override void Write(Utf8JsonWriter writer, ScriptResult<T> value, JsonSerializerOptions options) => throw new NotSupportedException();
}
