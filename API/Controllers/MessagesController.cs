using System.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    public class MessagesController:BaseApiController
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        public MessagesController(IUnitOfWork unitOfWork,IMapper mapper)
        {
            _mapper = mapper;
            _unitOfWork=unitOfWork;
        }

        [HttpPost]
        public async Task<ActionResult<MessageDto>>CreateMessage(CreateMessageDto createMessageDto)
        {
            var username = User.GetUsername();

            if(username==createMessageDto.RecipientUsername.ToLower())
                return BadRequest("You can not send message to your self");

            var sender =await _unitOfWork.UserRepository.GetUserByUsernameAsync(username);
            var recipient=await _unitOfWork.UserRepository.GetUserByUsernameAsync(createMessageDto.RecipientUsername);

            if(recipient is null) return NotFound();    

            var message=new Message
            {
                Sender = sender,
                Recipient = recipient,
                SenderUsername = sender.UserName,
                RecipientUsername = recipient.UserName,
                Content = createMessageDto.Content
            };

            _unitOfWork.MessageRepository.AddMessage(message);

            if(await _unitOfWork.Complete())
                return Ok(_mapper.Map<MessageDto>(message));

            return BadRequest("Failed to send Message..");  
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<MessageDto>>> GetMessagesForUser([FromQuery]
        MessageParams messageParams)
        {
           messageParams.Username=User.GetUsername();
           var messages=await _unitOfWork.MessageRepository.GetMessagesForUser(messageParams);
           //add pagination header
           Response.AddPaginationHeader(new PaginationHeader(messages.CurrentPage
            ,messages.PageSize,messages.TotalCount,messages.TotalPages));     

            return messages;
        }



       [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteMessage(int id)
        {
            var username = User.GetUsername();

            var message = await _unitOfWork.MessageRepository.GetMessage(id);

            if (message.SenderUsername != username && message.RecipientUsername != username) 
                return Unauthorized();

            if (message.SenderUsername == username) message.SenderDeleted = true;
            if (message.RecipientUsername == username) message.RecipientDeleted = true;

            if (message.SenderDeleted && message.RecipientDeleted) 
            {
                _unitOfWork.MessageRepository.DeleteMessage(message);
            }

            if (await _unitOfWork.Complete()) return Ok();

            return BadRequest("Problem deleting the message");

        }
    }
}