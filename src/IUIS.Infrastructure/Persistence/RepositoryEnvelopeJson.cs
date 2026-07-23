using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace IUIS.Infrastructure.Persistence
{
    public sealed class RepositoryEnvelopeJsonConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType.IsGenericType
                && objectType.GetGenericTypeDefinition() == typeof(RepositoryEnvelope<>);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var recordType = objectType.GetGenericArguments()[0];
            var envelopeType = typeof(RepositoryEnvelope<>).MakeGenericType(recordType);
            var envelope = Activator.CreateInstance(envelopeType);

            if (reader.TokenType == JsonToken.Null) return null;

            JObject jo = JObject.Load(reader);
            
            string canonicalName = jo.Value<string>("repositoryName");
            string legacyName = jo.Value<string>("repository");
            int schemaVersion = jo.Value<int>("schemaVersion");
            long revision = jo.Value<long>("revision");
            DateTime updatedAtUtc = jo.Value<DateTime?>("updatedAtUtc") ?? default(DateTime);
            string updatedByUserId = jo.Value<string>("updatedByUserId");
            JArray recordsArray = jo["records"] as JArray;
            var records = recordsArray?.ToObject(typeof(List<>).MakeGenericType(recordType), serializer);

            if (!string.IsNullOrWhiteSpace(canonicalName)
                && !string.IsNullOrWhiteSpace(legacyName)
                && !string.Equals(canonicalName, legacyName, StringComparison.OrdinalIgnoreCase))
            {
                throw new JsonException("Canonical and legacy repository names conflict.");
            }

            var nameProperty = envelopeType.GetProperty("RepositoryName");
            var schemaProperty = envelopeType.GetProperty("SchemaVersion");
            var revisionProperty = envelopeType.GetProperty("Revision");
            var updatedAtProperty = envelopeType.GetProperty("UpdatedAtUtc");
            var updatedByProperty = envelopeType.GetProperty("UpdatedByUserId");
            var recordsProperty = envelopeType.GetProperty("Records");

            nameProperty.SetValue(envelope, string.IsNullOrWhiteSpace(canonicalName) ? legacyName : canonicalName);
            schemaProperty.SetValue(envelope, schemaVersion);
            revisionProperty.SetValue(envelope, revision);
            updatedAtProperty.SetValue(envelope, updatedAtUtc);
            updatedByProperty.SetValue(envelope, updatedByUserId);
            recordsProperty.SetValue(envelope, records);

            return envelope;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value == null)
            {
                writer.WriteNull();
                return;
            }

            var envelopeType = value.GetType();
            var nameProperty = envelopeType.GetProperty("RepositoryName");
            var schemaProperty = envelopeType.GetProperty("SchemaVersion");
            var revisionProperty = envelopeType.GetProperty("Revision");
            var updatedAtProperty = envelopeType.GetProperty("UpdatedAtUtc");
            var updatedByProperty = envelopeType.GetProperty("UpdatedByUserId");
            var recordsProperty = envelopeType.GetProperty("Records");

            var name = (string)nameProperty.GetValue(value);
            var schema = (int)schemaProperty.GetValue(value);
            var revision = (long)revisionProperty.GetValue(value);
            var updatedAt = (DateTime)updatedAtProperty.GetValue(value);
            var updatedBy = (string)updatedByProperty.GetValue(value);
            var records = recordsProperty.GetValue(value);

            JObject jo = new JObject
            {
                ["repositoryName"] = name,
                ["schemaVersion"] = schema,
                ["revision"] = revision,
                ["updatedAtUtc"] = updatedAt.ToString("o"),
                ["updatedByUserId"] = updatedBy != null ? (JToken)updatedBy : JValue.CreateNull(),
                ["records"] = records != null ? JToken.FromObject(records, serializer) : JValue.CreateNull()
            };

            jo.WriteTo(writer);
        }
    }

    public sealed class RepositoryManifestRecordJsonConverter : JsonConverter<RepositoryManifestRecord>
    {
        public override RepositoryManifestRecord Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
                throw new JsonException("Repository manifest record must be an object.");

            string canonicalName = null;
            string legacyName = null;
            var record = new RepositoryManifestRecord();
            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject) break;
                var propertyName = reader.GetString();
                if (!reader.Read()) throw new JsonException("Repository manifest value is missing.");
                switch (propertyName)
                {
                    case "repositoryName": canonicalName = reader.GetString(); break;
                    case "repository": legacyName = reader.GetString(); break;
                    case "fileName": record.FileName = reader.GetString(); break;
                    case "schemaVersion": record.SchemaVersion = reader.GetInt32(); break;
                    case "revision": record.Revision = reader.GetInt64(); break;
                    case "sha256": record.Sha256 = reader.GetString(); break;
                    case "verifiedAtUtc": record.VerifiedAtUtc = reader.GetDateTime(); break;
                    default:
                        using (JsonDocument.ParseValue(ref reader)) { }
                        break;
                }
            }

            if (!string.IsNullOrWhiteSpace(canonicalName)
                && !string.IsNullOrWhiteSpace(legacyName)
                && !string.Equals(canonicalName, legacyName, StringComparison.OrdinalIgnoreCase))
                throw new JsonException("Repository manifest names conflict.");
            record.RepositoryName = string.IsNullOrWhiteSpace(canonicalName)
                ? legacyName
                : canonicalName;
            return record;
        }

        public override void Write(
            Utf8JsonWriter writer,
            RepositoryManifestRecord value,
            JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WriteString("repositoryName", value.RepositoryName);
            writer.WriteString("fileName", value.FileName);
            writer.WriteNumber("schemaVersion", value.SchemaVersion);
            writer.WriteNumber("revision", value.Revision);
            writer.WriteString("sha256", value.Sha256);
            writer.WriteString("verifiedAtUtc", value.VerifiedAtUtc);
            writer.WriteEndObject();
        }
    }

    public static class RepositoryEnvelopeJson
    {
        public static RepositoryEnvelope<T> Deserialize<T>(
            string json,
            JsonSerializerOptions options)
        {
            if (string.IsNullOrWhiteSpace(json))
                throw new InvalidDataException("Repository JSON is required.");
            try
            {
                return JsonSerializer.Deserialize<RepositoryEnvelope<T>>(json, options);
            }
            catch (JsonException exception)
            {
                throw new InvalidDataException("Repository envelope JSON is invalid.", exception);
            }
        }

        public static string Serialize<T>(
            RepositoryEnvelope<T> envelope,
            JsonSerializerOptions options)
        {
            if (envelope == null) throw new ArgumentNullException(nameof(envelope));
            return JsonSerializer.Serialize(envelope, options);
        }

        public static bool IsLegacy(string json)
        {
            using (var document = JsonDocument.Parse(json))
            {
                var root = document.RootElement;
                return root.TryGetProperty("repository", out _)
                    || root.TryGetProperty("createdAtUtc", out _)
                    || !root.TryGetProperty("repositoryName", out _);
            }
        }

        public static string CanonicalizeRaw(
            string json,
            ProductionRepositoryDescriptor descriptor,
            JsonSerializerOptions options)
        {
            if (descriptor == null) throw new ArgumentNullException(nameof(descriptor));
            var envelope = Deserialize<JsonElement>(json, options);
            Validate(descriptor, envelope);
            envelope.RepositoryName = descriptor.Name;
            return Serialize(envelope, options);
        }

        public static void Validate<T>(
            ProductionRepositoryDescriptor descriptor,
            RepositoryEnvelope<T> envelope)
        {
            if (envelope == null) throw new InvalidDataException("Repository envelope is missing.");
            if (!string.Equals(
                envelope.RepositoryName,
                descriptor.Name,
                StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("Repository envelope name does not match its catalog entry.");
            if (envelope.SchemaVersion != descriptor.SchemaVersion)
                throw new InvalidDataException("Repository schema version is unsupported.");
            if (envelope.Revision < 0)
                throw new InvalidDataException("Repository revision cannot be negative.");
            if (envelope.UpdatedAtUtc.Kind != DateTimeKind.Utc)
                throw new InvalidDataException("Repository updated timestamp must be UTC.");
            if (string.IsNullOrWhiteSpace(envelope.UpdatedByUserId))
                throw new InvalidDataException("Repository update actor is required.");
            if (envelope.Records == null)
                throw new InvalidDataException("Repository records collection is required.");
        }
    }
}
