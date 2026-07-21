using IUIS.Domain.Identity;

namespace IUIS.Application.Security
{
    public sealed class UserSession
    {
        public UserSession(
            string sessionId,
            string sessionToken,
            string userId,
            string loginId,
            string personRecordId,
            PrimaryRole primaryRole,
            SessionApplicationKind applicationKind,
            SessionPurpose purpose)
        {
            SessionId = sessionId;
            SessionToken = sessionToken;
            UserId = userId;
            LoginId = loginId;
            PersonRecordId = personRecordId;
            PrimaryRole = primaryRole;
            ApplicationKind = applicationKind;
            Purpose = purpose;
        }

        public string SessionId { get; private set; }
        public string SessionToken { get; private set; }
        public string UserId { get; private set; }
        public string LoginId { get; private set; }
        public string PersonRecordId { get; private set; }
        public PrimaryRole PrimaryRole { get; private set; }
        public SessionApplicationKind ApplicationKind { get; private set; }
        public SessionPurpose Purpose { get; private set; }

        public SessionCredential ToCredential()
        {
            return new SessionCredential(SessionId, SessionToken);
        }
    }
}
