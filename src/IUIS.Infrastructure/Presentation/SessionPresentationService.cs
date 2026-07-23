using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using IUIS.Application.Abstractions.Security;
using IUIS.Application.Authorization;
using IUIS.Application.Security;
using IUIS.Infrastructure.Persistence;
using IUIS.Infrastructure.Security;

namespace IUIS.Infrastructure.Presentation
{
    public sealed class SessionPresentationService : ISessionPresentationService
    {
        private readonly JsonAuthorizationPrincipalProvider _principalProvider;
        private readonly PermissionResolver _permissionResolver;
        private readonly ProductionRepositoryCatalog _catalog;
        private readonly JsonInfrastructureOptions _options;
        private readonly JsonSerializerOptions _json = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public SessionPresentationService(
            JsonAuthorizationPrincipalProvider principalProvider,
            PermissionResolver permissionResolver,
            ProductionRepositoryCatalog catalog,
            JsonInfrastructureOptions options)
        {
            _principalProvider = principalProvider ?? throw new ArgumentNullException(nameof(principalProvider));
            _permissionResolver = permissionResolver ?? throw new ArgumentNullException(nameof(permissionResolver));
            _catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

        public bool ValidateSession(
            SessionCredential credential,
            out UserSession session,
            out EffectiveAccessSnapshot access,
            out string reason)
        {
            session = null;
            access = null;
            reason = null;

            if (credential == null)
            {
                reason = "Your session is no longer valid.";
                return false;
            }

            try
            {
                var utcNow = DateTime.UtcNow;
                var principal = _principalProvider.Load(
                    credential.SessionId,
                    credential.SessionToken,
                    utcNow);

                var account = ReadUserAccount(principal.UserId);
                if (account == null)
                {
                    reason = "Your session is no longer valid.";
                    return false;
                }

                session = new UserSession(
                    credential.SessionId,
                    credential.SessionToken,
                    principal.UserId,
                    account.LoginId,
                    principal.PersonRecordId,
                    principal.PrimaryRole,
                    principal.ApplicationKind,
                    principal.SessionPurpose);

                access = new EffectiveAccessSnapshot(
                    principal,
                    _permissionResolver.ResolveEffectivePermissions(principal));

                return true;
            }
            catch
            {
                reason = "Your session has expired or is no longer valid.";
                return false;
            }
        }

        public void Revoke(SessionCredential credential, string reason)
        {
            if (credential == null)
                return;

            var path = _catalog.ResolvePath(_options.DataRoot, "sessions");
            using (CrossProcessFileLock.Acquire(path, _options.LockTimeout))
            {
                var envelope = RepositoryEnvelopeJson.Deserialize<PersistedSessionRecord>(
                    File.ReadAllText(path),
                    _json);
                var session = envelope.Records.SingleOrDefault(
                    item => string.Equals(item.Id, credential.SessionId, StringComparison.Ordinal));
                if (session == null || !string.Equals(session.Status, "Active", StringComparison.OrdinalIgnoreCase))
                    return;

                session.Status = "Revoked";
                session.RevokedAtUtc = DateTime.UtcNow;
                envelope.Revision = checked(envelope.Revision + 1);
                envelope.UpdatedAtUtc = DateTime.UtcNow;
                envelope.UpdatedByUserId = session.UserId;

                new AtomicFileWriter().WriteUtf8(
                    path,
                    RepositoryEnvelopeJson.Serialize(envelope, _json));
            }
        }

        public void Touch(SessionCredential credential)
        {
            if (credential == null)
                return;

            var path = _catalog.ResolvePath(_options.DataRoot, "sessions");
            using (CrossProcessFileLock.Acquire(path, _options.LockTimeout))
            {
                var envelope = RepositoryEnvelopeJson.Deserialize<PersistedSessionRecord>(
                    File.ReadAllText(path),
                    _json);
                var session = envelope.Records.SingleOrDefault(
                    item => string.Equals(item.Id, credential.SessionId, StringComparison.Ordinal));
                if (session == null || !string.Equals(session.Status, "Active", StringComparison.OrdinalIgnoreCase))
                    return;

                var utcNow = DateTime.UtcNow;
                session.LastActivityAtUtc = utcNow;
                session.InactivityExpiresAtUtc = utcNow.AddMinutes(
                    session.Purpose == "FirstLoginPasswordChange" ? 10 : 15);
                envelope.Revision = checked(envelope.Revision + 1);
                envelope.UpdatedAtUtc = utcNow;
                envelope.UpdatedByUserId = session.UserId;

                new AtomicFileWriter().WriteUtf8(
                    path,
                    RepositoryEnvelopeJson.Serialize(envelope, _json));
            }
        }

        private PersistedUserAccount ReadUserAccount(string userId)
        {
            var path = _catalog.ResolvePath(_options.DataRoot, "users");
            var envelope = RepositoryEnvelopeJson.Deserialize<PersistedUserAccount>(
                File.ReadAllText(path),
                _json);
            return envelope.Records.SingleOrDefault(
                item => string.Equals(item.Id, userId, StringComparison.Ordinal));
        }
    }
}
