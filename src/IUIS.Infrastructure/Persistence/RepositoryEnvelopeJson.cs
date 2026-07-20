using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace IUIS.Infrastructure.Persistence
{
    public sealed class RepositoryEnvelopeJsonConverterFactory : JsonConverterFactory
    {
        public override bool CanConvert(Type typeToConvert)
        {
            return typeToConvert.IsGenericType
                && typeToConvert.GetGenericTypeDefinition() == typeof(RepositoryEnvelope<>);
        }

        public override JsonConverter CreateConverter(
            Type typeToConvert,
            JsonSerializerOptions options)
        {
            var recordType = typeToConvert.GetGenericArguments()[0];
            return (JsonConverter)Activator.CreateInstance(
                typeof(RepositoryEnvelopeJsonConverter<>).MakeGenericType(recordType));
        }
    }

    public sealed class RepositoryEnvelopeJsonConverter<T> : JsonConverter<RepositoryEnvelope<T>>
    {
        public override RepositoryEnvelope<T> Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
                throw new JsonException("Repository envelope must be a JSON object.");

            string canonicalName = null;
            string legacyName = null;
            var schemaVersion = 0;
            var revision = -1L;
            DateTime? updatedAtUtc = null;
            string updatedByUserId = null;
            List<T> records = null;

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject) break;
                if (reader.TokenType != JsonTokenType.PropertyName)
                    throw new JsonException("Repository envelope property name is invalid.");

                var propertyName = reader.GetString();
                if (!reader.Read()) throw new JsonException("Repository envelope value is missing.");

                switch (propertyName)
                {
                    case "repositoryName":
                        canonicalName = reader.TokenType == JsonTokenType.Null
                            ? null
                            : reader.GetString();
                        break;
                    case "repository":
                        legacyName = reader.TokenType == JsonTokenType.Null
                            ? null
                            : reader.GetString();
                        break;
                    case "schemaVersion":
                        schemaVersion = reader.GetInt32();
                        break;
                    case "revision":
                        revision = reader.GetInt64();
                        break;
                    case "updatedAtUtc":
                        updatedAtUtc = reader.GetDateTime();
                        break;
                    case "updatedByUserId":
                        updatedByUserId = reader.TokenType == JsonTokenType.Null
                            ? null
                            : reader.GetString();
                        break;
                    case "records":
                        records = JsonSerializer.Deserialize<List<T>>(ref reader, options);
                        break;
                    default:
                        using (JsonDocument.ParseValue(ref reader)) { }
                        break;
                }
            }

            if (!string.IsNullOrWhiteSpace(canonicalName)
                && !string.IsNullOrWhiteSpace(legacyName)
                && !string.Equals(canonicalName, legacyName, StringComparison.OrdinalIgnoreCase))
            {
                throw new JsonException("Canonical and legacy repository names conflict.");
            }

            return new RepositoryEnvelope<T>
            {
                RepositoryName = string.IsNullOrWhiteSpace(canonicalName)
                    ? legacyName
                    : canonicalName,
                SchemaVersion = schemaVersion,
                Revision = revision,
                UpdatedAtUtc = updatedAtUtc ?? default(DateTime),
                UpdatedByUserId = updatedByUserId,
                Records = records
            };
        }

        public override void Write(
            Utf8JsonWriter writer,
            RepositoryEnvelope<T> value,
            JsonSerializerOptions options)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            writer.WriteStartObject();
            writer.WriteString("repositoryName", value.RepositoryName);
            writer.WriteNumber("schemaVersion", value.SchemaVersion);
            writer.WriteNumber("revision", value.Revision);
            writer.WriteString("updatedAtUtc", value.UpdatedAtUtc);
            if (value.UpdatedByUserId == null)
                writer.WriteNull("updatedByUserId");
            else
                writer.WriteString("updatedByUserId", value.UpdatedByUserId);
            writer.WritePropertyName("records");
            JsonSerializer.Serialize(writer, value.Records, options);
            writer.WriteEndObject();
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
