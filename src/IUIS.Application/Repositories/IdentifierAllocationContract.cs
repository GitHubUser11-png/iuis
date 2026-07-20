namespace IUIS.Application.Repositories
{
    public interface IApplicationIdentifierAllocator
    {
        string Allocate(
            string prefix,
            int year,
            string actorUserId);
    }
}
