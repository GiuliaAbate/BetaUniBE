
using BetaUni.Models;
using BetaUni.Other;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text.Json.Serialization;

namespace BetaUni
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            //Servizio usato che ha metodi vari per controlli
            builder.Services.AddScoped<Services>();

            //Service per connettersi al db
            builder.Services.AddDbContext<IubContext>(opt =>
            opt.UseSqlServer(builder.Configuration.GetConnectionString("BetaUni")));

            //Service per ignorare eventuali cicli
            builder.Services.AddControllers().AddJsonOptions(
                jsOpt => jsOpt.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);

            //builder.Services.AddCors(options =>
            //{
            //    options.AddPolicy("MyPolicy", policy =>                      
            //    {
            //        policy.WithOrigins("http://localhost:4200")
            //              .AllowAnyHeader()
            //              .AllowAnyMethod()
            //              .AllowCredentials();
            //    });
            //});

            //Si istanzia classe che si userà per JWT
            JWTSettings jwtSettings = new();
            //si va a leggere una sezione intera
            jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JWTSettings>();
            //questa classe viene poi tirata su come classe di tipo singleton che viene istanziata una sola volta
            //e che verrà usata tramite dependency injection
            builder.Services.AddSingleton(jwtSettings);

            builder.Services.AddAuthentication(opt =>
            {
                opt.DefaultAuthenticateScheme = "StudentScheme";
                opt.DefaultChallengeScheme = "StudentScheme";
            })
                .AddJwtBearer("ProfessorScheme", opt =>
                {
                    //con questa configurazione si tira su un servizio che permette di gestire il sistema di configurazione
                    opt.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = jwtSettings.Issuer,
                        ValidAudience = jwtSettings.AudienceP,
                        //generare un tempo in secondi, questo dice che quando si va a fare la validazione 
                        //deve prendere una tolleranza di tot secondi
                        ClockSkew = TimeSpan.FromSeconds(2),
                        IssuerSigningKey = new SymmetricSecurityKey(
                            System.Text.Encoding.UTF8.GetBytes(jwtSettings.SecretKey))
                    };
                    opt.Events = new JwtBearerEvents
                    {
                        OnMessageReceived = context =>
                        {
                            var accessToken = context.Request.Cookies["access_token"];
                            if (!string.IsNullOrEmpty(accessToken))
                            {
                                context.Token = accessToken;
                            }
                            return Task.CompletedTask;
                        }
                    };
                })
                .AddJwtBearer("StudentScheme", opt =>
                {
                    opt.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = jwtSettings.Issuer,
                        ValidAudience = jwtSettings.AudienceS,
                        ClockSkew = TimeSpan.FromSeconds(2),
                        IssuerSigningKey = new SymmetricSecurityKey(
                            System.Text.Encoding.UTF8.GetBytes(jwtSettings.SecretKey))
                    };
                    opt.Events = new JwtBearerEvents
                    {
                        OnMessageReceived = context =>
                        {
                            var accessToken = context.Request.Cookies["access_token"];
                            if (!string.IsNullOrEmpty(accessToken))
                            {
                                context.Token = accessToken;
                            }
                            return Task.CompletedTask;
                        }
                    };
                })
                .AddJwtBearer("OmniScheme", opt =>
                {
                    opt.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = jwtSettings.Issuer,
                        ValidAudience = jwtSettings.AudienceS,
                        ClockSkew = TimeSpan.FromSeconds(2),
                        IssuerSigningKey = new SymmetricSecurityKey(
                            System.Text.Encoding.UTF8.GetBytes(jwtSettings.SecretKey))
                    };
                    opt.Events = new JwtBearerEvents
                    {
                        OnMessageReceived = context =>
                        {
                            var accessToken = context.Request.Cookies["access_token"];
                            if (!string.IsNullOrEmpty(accessToken))
                            {
                                context.Token = accessToken;
                            }
                            return Task.CompletedTask;
                        }
                    };
                });

            //Si aggiunge una policy per implementare entrambe le autorizzazioni
            builder.Services.AddAuthorization(options =>                    
            {
                options.AddPolicy("OmniScheme", policy =>
                    policy.AddAuthenticationSchemes("StudentScheme", "ProfessorScheme")
                          .RequireAuthenticatedUser());
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseCors(builder =>
                builder.WithOrigins("http://localhost:4200") // Cambia porta se serve
                       .AllowAnyHeader()
                       .AllowAnyMethod()
                       .AllowCredentials()
            );

            app.UseHttpsRedirection();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
