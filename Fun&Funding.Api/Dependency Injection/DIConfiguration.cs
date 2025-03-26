using Fun_Funding.Api.Exception;
using Fun_Funding.Application.Interfaces.IExternalServices;
using Fun_Funding.Application.Services.ExternalServices;
using Fun_Funding.Application;
using Fun_Funding.Domain.EmailModel;
using Fun_Funding.Infrastructure;
using Microsoft.OpenApi.Models;
using Net.payOS;
using System.Net.Mail;
using System.Security.Claims;
using Microsoft.AspNetCore.SignalR;
using Fun_Funding.Application.CustomUserIdProvider;

namespace Fun_Funding.Api.Dependency_Injection
{
    public static class DIConfiguration
    {
        public static IServiceCollection AddPresistence(this IServiceCollection service, IConfiguration configuration)
        {
            #region Authenticaton Google
            service.AddAuthentication()
                .AddGoogle(options =>
                {
                    options.ClientId = configuration["Authentication:Google:ClientId"];
                    options.ClientSecret = configuration["Authentication:Google:ClientSecret"];
                    options.Scope.Add("profile");
                    options.Events.OnCreatingTicket = (context) =>
                    {
                        var picture = context.User.GetProperty("picture").GetString();

                        context.Identity.AddClaim(new Claim("User_avatar", picture));

                        return Task.CompletedTask;
                    };

                });
            #endregion

            #region PayOs
            PayOS payOS = new PayOS(
               configuration["PayOS:PAYOS_CLIENT_ID"] ?? throw new System.Exception("Cannot find environment"),
               configuration["PayOS:PAYOS_API_KEY"] ?? throw new System.Exception("Cannot find environment"),
               configuration["PayOS:PAYOS_CHECKSUM_KEY"] ?? throw new System.Exception("Cannot find environment")
               );

            service.AddSingleton(payOS);
            #endregion

            #region CORS
            //add CORS
            service.AddCors(p => p.AddPolicy("MyCors", build =>
            {
                build.WithOrigins("http://localhost:5173")
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials();
            }));
            #endregion

            #region FluentEmail
            //email service
            var smtpSettings = configuration.GetSection("SmtpSettings").Get<SmtpSettings>();

            service.AddFluentEmail(smtpSettings.FromEmail, smtpSettings.FromName)
                .AddRazorRenderer()
                .AddSmtpSender(new SmtpClient(smtpSettings.Host)
                {
                    Port = smtpSettings.Port,
                    Credentials = new System.Net.NetworkCredential(smtpSettings.UserName, smtpSettings.Password),
                    EnableSsl = true,
                });
            #endregion

            #region Swagger Configuration
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            service.AddEndpointsApiExplorer();
            service.AddSwaggerGen();
            service.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Your API Title",
                    Version = "v1"
                });

                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = @"JWT Authorization header using the Bearer scheme. 
            Enter 'Bearer' [space] and then your token in the text input below.
            Example: 'Bearer 12345abcdef'",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });



                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            },
                            Scheme = "oauth2",
                            Name = "Bearer",
                            In = ParameterLocation.Header
                        },
                        new List<string>()
                    }
                });
            });
            #endregion

            #region CustomIdProviderSignalR
            service.AddSignalR();
            service.AddSingleton<IUserIdProvider, CustomUserIdProvider>();
            #endregion

            service.AddTransient<IEmailService, EmailService>();
            service.AddTransient<IUnitOfWork, UnitOfWork>();
            return service;

        }
    }
}
