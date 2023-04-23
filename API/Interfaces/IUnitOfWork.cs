using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.Interfaces
{
    public interface IUnitOfWork
    {
        IUserRepository UserRepository{get;}
        IMessageRepository MessageRepository{get;}
        ILikesRepository LikesRepository{get;}
        Task<bool>Complete();
        bool HasChanges(); //use it to check if any changes done to db
    }
}