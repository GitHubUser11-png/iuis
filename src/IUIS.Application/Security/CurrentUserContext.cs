namespace IUIS.Application.Security
{
    public sealed class CurrentUserContext
    {
        public bool IsAuthenticated
        {
            get { return Session != null; }
        }

        public UserSession Session { get; private set; }
        public EffectiveAccessSnapshot Access { get; private set; }

        public string SessionId
        {
            get { return Session == null ? null : Session.SessionId; }
        }

        public string UserId
        {
            get { return Session == null ? null : Session.UserId; }
        }

        public void SetSession(UserSession session, EffectiveAccessSnapshot access)
        {
            Session = session;
            Access = access;
        }

        public void SetSession(UserSession session)
        {
            Session = session;
        }

        public void Clear()
        {
            Session = null;
            Access = null;
        }

        public SessionCredential GetCredential()
        {
            return Session == null ? null : Session.ToCredential();
        }
    }
}
