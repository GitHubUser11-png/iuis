using System;

namespace IUIS.Application.Security
{
    public sealed class SessionCredential
    {
        public SessionCredential(string sessionId, string sessionToken)
        {
            if (string.IsNullOrWhiteSpace(sessionId))
                throw new ArgumentException("A session identifier is required.", nameof(sessionId));
            if (string.IsNullOrWhiteSpace(sessionToken))
                throw new ArgumentException("A session token is required.", nameof(sessionToken));

            SessionId = sessionId.Trim();
            SessionToken = sessionToken;
        }

        public string SessionId { get; private set; }
        public string SessionToken { get; private set; }
    }
}
