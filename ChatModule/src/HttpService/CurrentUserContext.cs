using System;

namespace ChatModule.src.HttpService
{
    public class CurrentUserContext
    {
        public CurrentUserContext(Guid userId)
        {
            UserId = userId;
        }

        public Guid UserId { get; }
    }
}
