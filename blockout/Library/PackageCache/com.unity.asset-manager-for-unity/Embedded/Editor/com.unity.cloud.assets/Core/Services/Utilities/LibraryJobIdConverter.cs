using System;
using Newtonsoft.Json;

namespace Unity.Cloud.AssetsEmbedded
{
    class LibraryJobIdConverter : JsonConverter<AssetLibraryJobId>
    {
        public override void WriteJson(JsonWriter writer, AssetLibraryJobId value, JsonSerializer serializer)
        {
            writer.WriteValue(value.ToString());
        }

        public override AssetLibraryJobId ReadJson(JsonReader reader, Type objectType, AssetLibraryJobId existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            return reader.TokenType == JsonToken.String ? new AssetLibraryJobId(reader.Value?.ToString()) : existingValue;
        }
    }
}
