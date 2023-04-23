using API.DTOs;
using API.Entities;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    public class MessageRepository : IMessageRepository
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;
        public MessageRepository(DataContext context,IMapper mapper)
        {
            _context=context;
            _mapper=mapper;
        }
        public void AddGroup(Group group)
        {
            _context.Groups.Add(group);
        }
        public async Task<Connection> GetConnection(string connectionId)
        {
            return await _context.Connections.FindAsync(connectionId);
        }

        public void RemoveConnection(Connection connection)
        {
            _context.Connections.Remove(connection);
        }
        public async Task<Group> GetMessageGroup(string groupName)
        {
            return await _context.Groups
                .Include(x => x.Connections)
                .FirstOrDefaultAsync(x => x.Name == groupName);
        }

        public async Task<Group> GetGroupForConnection(string connectionId)
        {
            return await _context.Groups
                .Include(x => x.Connections)
                .Where(x => x.Connections.Any(c => c.ConnectionId == connectionId))
                .FirstOrDefaultAsync();
        }
        public void AddMessage(Message message)
        {
            _context.Messages.Add(message);
        }

        public void DeleteMessage(Message message)
        {
            _context.Messages.Remove(message);
        }

        public async Task<Message> GetMessage(int id)
        {
            return await _context.Messages
            .Include(u=>u.Sender)
            .Include(u=>u.Recipient)
            .SingleOrDefaultAsync(x=> x.Id == id);
        }

        //getting unread messages for user
        public async Task<PagedList<MessageDto>> GetMessagesForUser(MessageParams messageParams)
        {
            var query = _context.Messages
            .OrderByDescending(m=>m.MessageSent)
            .ProjectTo<MessageDto>(_mapper.ConfigurationProvider)
            .AsQueryable();
            
            query = messageParams.Container switch{

                "Inbox"=>query.Where(u=>u.RecipientUsername==messageParams.Username
                         && u.RecipientDeleted ==false),

                "Outbox"=>query.Where(u=>u.SenderUsername==messageParams.Username 
                        && u.SenderDeleted==false),

                _ => query.Where(u=>u.RecipientUsername==messageParams.Username 
                                && u.RecipientDeleted==false && u.DateRead == null )
            };

            //var messages = query.ProjectTo<MessageDto>(_mapper.ConfigurationProvider);

            return await PagedList<MessageDto>
                    .CreateAsync(query,messageParams.PageNumber,messageParams.PageSize);    
        }

        //get chat between two members
        public async Task<IEnumerable<MessageDto>> GetMessageThread(string currentUserName, string recipientUserName)
        {
            //get conversation for both users
             var messages = await _context.Messages
             // .Include(u => u.Sender).ThenInclude(p => p.Photos)  we do not need them now cause we use ProjectTo()
            //.Include(u => u.Recipient).ThenInclude(p => p.Photos)
                .Where(
                   m => m.RecipientUsername == currentUserName && m.RecipientDeleted == false &&
                    m.SenderUsername == recipientUserName ||
                    m.RecipientUsername == recipientUserName && m.SenderDeleted == false &&
                    m.SenderUsername == currentUserName 
                )
                .OrderBy(m => m.MessageSent)
                .ProjectTo<MessageDto>(_mapper.ConfigurationProvider)
                .ToListAsync();

            var unreadMessages = messages.Where(m => m.DateRead == null 
                && m.RecipientUsername == currentUserName).ToList();

            if (unreadMessages.Any())
            {
                foreach (var message in unreadMessages)
                {
                    message.DateRead = DateTime.UtcNow;
                }

                //await _context.SaveChangesAsync();  
                //we will do that step in the hup and remove the end point from the controller
            }

            //return _mapper.Map<IEnumerable<MessageDto>>(messages); 
            //that return all data for users i need not that so we used project to 
            return messages;
        }

    }
}