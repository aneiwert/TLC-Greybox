using System;
using Newtonsoft.Json;
using Unity.Cloud.CommonEmbedded;

namespace Unity.Cloud.AssetsEmbedded
{
    class LibraryIdConverter : JsonConverter<AssetLibraryId>
    {
        public override void WriteJson(JsonWriter writer, AssetLibraryId value, JsonSerializer serializer)
        {
            writer.WriteValue(value.ToString());
        }

        public override AssetLibraryId ReadJson(JsonReader reader, Type objectType, AssetLibraryId existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            return reader.TokenType == JsonToken.String ? new AssetLibraryId(reader.Value?.ToString()) : existingValue;
        }
    }
}
