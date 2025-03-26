using Fun_Funding.Api.Dependency_Injection;
using Fun_Funding.Api.Exception;
using Fun_Funding.Application;
using Fun_Funding.Application.AppHub;
using Fun_Funding.Application.Interfaces.IExternalServices;
using Fun_Funding.Application.IService;
using Fun_Funding.Application.Services.ExternalServices;
using Fun_Funding.Domain.EmailModel;
using Fun_Funding.Infrastructure;
using Fun_Funding.Infrastructure.Dependency_Injection;
using Microsoft.Extensions.FileProviders;
using Microsoft.OpenApi.Models;
using Net.payOS;
using System.Net;
using System.Net.Mail;
using System.Security.Claims;

namespace Fun_Funding.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddInfrastructure(builder.Configuration);
            builder.Services.AddPresistence(builder.Configuration);
            builder.WebHost.ConfigureKestrel(options =>
            {
                options.Limits.MaxRequestBodySize = 500 * 1024 * 1024; // 500 MB
            });
            #region old_DI
            //builder.Services.AddAuthentication()
            //    .AddGoogle(options =>
            //    {
            //        options.ClientId = builder.Configuration["Authentication:Google:ClientId"];
            //        options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
            //        options.Scope.Add("profile");
            //        options.Events.OnCreatingTicket = (context) =>
            //        {
            //            var picture = context.User.GetProperty("picture").GetString();

            //            context.Identity.AddClaim(new Claim("User_avatar", picture));

            //            return Task.CompletedTask;
            //        };

            //    });

            //PayOS payOS = new PayOS(
            //    builder.Configuration["PayOS:PAYOS_CLIENT_ID"] ?? throw new System.Exception("Cannot find environment"),
            //    builder.Configuration["PayOS:PAYOS_API_KEY"] ?? throw new System.Exception("Cannot find environment"),
            //    builder.Configuration["PayOS:PAYOS_CHECKSUM_KEY"] ?? throw new System.Exception("Cannot find environment")
            //    );

            //builder.Services.AddSingleton(payOS);

            ////add CORS
            //builder.Services.AddCors(p => p.AddPolicy("MyCors", build =>
            //{
            //    build.WithOrigins("*")
            //    .AllowAnyMethod()
            //    .AllowAnyHeader();
            //}));
            //
            ////email service
            //var smtpSettings = builder.Configuration.GetSection("SmtpSettings").Get<SmtpSettings>();

            //builder.Services.AddFluentEmail(smtpSettings.FromEmail, smtpSettings.FromName)
            //    .AddRazorRenderer()
            //    .AddSmtpSender(new SmtpClient(smtpSettings.Host)
            //    {
            //        Port = smtpSettings.Port,
            //        Credentials = new System.Net.NetworkCredential(smtpSettings.UserName, smtpSettings.Password),
            //        EnableSsl = true,
            //    });
            //builder.Services.AddTransient<IEmailService, EmailService>();
            //builder.Services.AddTransient<IUnitOfWork, UnitOfWork>();

            //// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            //builder.Services.AddEndpointsApiExplorer();
            //builder.Services.AddSwaggerGen();
            //builder.Services.AddSwaggerGen(c =>
            //{
            //    c.SwaggerDoc("v1", new OpenApiInfo
            //    {
            //        Title = "Your API Title",
            //        Version = "v1"
            //    });

            //    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            //    {
            //        Description = @"JWT Authorization header using the Bearer scheme. 
            //Enter 'Bearer' [space] and then your token in the text input below.
            //Example: 'Bearer 12345abcdef'",
            //        Name = "Authorization",
            //        In = ParameterLocation.Header,
            //        Type = SecuritySchemeType.ApiKey,
            //        Scheme = "Bearer"
            //    });



            //    c.AddSecurityRequirement(new OpenApiSecurityRequirement
            //    {
            //        {
            //            new OpenApiSecurityScheme
            //            {
            //                Reference = new OpenApiReference
            //                {
            //                    Type = ReferenceType.SecurityScheme,
            //                    Id = "Bearer"
            //                },
            //                Scheme = "oauth2",
            //                Name = "Bearer",
            //                In = ParameterLocation.Header
            //            },
            //            new List<string>()
            //        }
            //    });
            //});
            #endregion

            builder.Services.AddControllers(options =>
            {
                options.Filters.Add<GlobalExceptionHandler>();
            })
            .AddNewtonsoftJson(options =>
                options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
            );
            //app
            var app = builder.Build();
            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(
        Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).FullName, "Fun&Funding.Infrastructure", "Media")),
                RequestPath = "/Media"
            });

            app.UseWebSockets(new WebSocketOptions
            {
                KeepAliveInterval = TimeSpan.FromMinutes(2)
            });

            // WebSocket mapping
            app.Map("/ws", async context =>
            {
                if (context.WebSockets.IsWebSocketRequest)
                {
                    var chatService = context.RequestServices.GetRequiredService<IChatService>();

                    if (!context.Request.Query.TryGetValue("SenderId", out var senderIdValues) ||
                        string.IsNullOrEmpty(senderIdValues.FirstOrDefault()))
                    {
                        context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                        await context.Response.WriteAsync("SenderId is required");
                        return;
                    }

                    var senderId = senderIdValues.First();

                    Console.WriteLine($"WebSocket connection request from SenderId: {senderId}");

                    // Retrieve ReceiverId from the query
                    if (!context.Request.Query.TryGetValue("ReceiverId", out var receiverIdValues) ||
                        string.IsNullOrEmpty(receiverIdValues.FirstOrDefault()))
                    {
                        context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                        await context.Response.WriteAsync("ReceiverId is required");
                        return;
                    }

                    var receiverId = receiverIdValues.First();

                    using var webSocket = await context.WebSockets.AcceptWebSocketAsync();
                    await chatService.HandleWebSocketConnectionAsync(webSocket, senderId, receiverId);
                }
                else
                {
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                }
            });

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseRouting();

            app.UseCors("MyCors");

            app.UseAuthentication();
            app.UseAuthorization();


            app.MapControllers();
            app.MapHub<NotificationHub>("/notificationHub");
            app.Run();
        }
    }
}
