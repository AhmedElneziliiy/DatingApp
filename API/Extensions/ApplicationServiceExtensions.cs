using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Data;
using API.Helpers;
using API.Interfaces;
using API.Services;
using API.SignalR;
using Microsoft.EntityFrameworkCore;

namespace API.Extensions
{
    public static class ApplicationServiceExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services,IConfiguration config)
        {
            services.AddSingleton<PresenceTracker>();
            services.Configure<CloudinarySettings>(config.GetSection("CloudinarySettings"));
            services.AddScoped<ITokenService,TokenService>();
            services.AddScoped<IPhotoService,PhotoService>();
            //services.AddScoped<IUserRepository,UserRepository>();
           // services.AddScoped<ILikesRepository,LikesRepository>();
           // services.AddScoped<IMessageRepository,MessageRepository>();
            services.AddScoped<IUnitOfWork,UnitOfWork>();
            services.AddSignalR();


            services.AddScoped<LogUserActivity>();
            services.AddAutoMapper(typeof(AutoMapperProfiles).Assembly);
            
            services.AddDbContext<DataContext>(options=>{
            options.UseSqlite(config.GetConnectionString("DefaultConnection"));
            });

            return services;
        }
    }
}