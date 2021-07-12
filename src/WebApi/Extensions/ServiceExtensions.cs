using Application.DTOs.Product.Validators;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.OpenApi.Models;
using Presentation.Common;
using Swashbuckle.AspNetCore.Swagger;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using WebApi.ActionFilters;

namespace WebApi.Extensions
{
    public static class ServiceExtensions
    {
        public static void AddAuthenticationViaOIDC(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
        {
            services.AddTransient<IClaimsTransformation, ClaimsTransformer>();
            services.AddAuthentication(config =>
            {
                config.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                config.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
            })
               .AddCookie(config =>
               {
                   config.LoginPath = "/api/account/login";
                   config.LogoutPath = "/api/account/logout";
                   config.AccessDeniedPath = "/api/account/access-denied";
               })
               .AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, config =>
               {
                   config.Authority = configuration["OidcProvider:Authority"];
                   config.ClientId = configuration["OidcProvider:ClientId"];
                   config.ClientSecret = configuration["OidcProvider:ClientSecret"];
                   config.SaveTokens = true;
                   config.ResponseType = OpenIdConnectResponseType.Code;
                   config.Scope.Add("roles");
                   config.GetClaimsFromUserInfoEndpoint = true;
                   config.UsePkce = true;

                   config.Events = new OpenIdConnectEvents()
                   {
                       OnAuthenticationFailed = c =>
                       {
                           c.HandleResponse();

                           c.Response.StatusCode = 500;
                           c.Response.ContentType = "text/plain";
                           if (environment.IsDevelopment())
                           {
                               // Debug only, in production do not share exceptions with the remote host.
                               return c.Response.WriteAsync(c.Exception.ToString());
                           }
                           return c.Response.WriteAsync("An error occurred processing your authentication.");
                       }
                   };
               });
        }

        public static void AddSwaggerExtension(this IServiceCollection services)
        {
            services.AddSwaggerGen(c =>
            {
                c.AddFluentValidationRulesScoped();

                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1",
                    Title = "Clean Architecture API",
                    Description = "API Description.",
                    Contact = new OpenApiContact
                    {
                        Name = "Contact name",
                        Email = "john.doe@mail.com",
                        Url = new Uri("https://google.com"),
                    }
                });

                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    Description = "Input your Bearer token in this format - Bearer {your token here} to access this API",
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer",
                            },
                            Scheme = "Bearer",
                            Name = "Bearer",
                            In = ParameterLocation.Header,
                        }, new List<string>()
                    },
                });

                // Add Controller's summaries descriptions
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);
            });
        }

        public static void AddControllersWithFluentValidations(this IServiceCollection services)
        {
            services.Configure<ApiBehaviorOptions>(options =>
            {
                options.SuppressModelStateInvalidFilter = true;
            });

            services.AddControllers(options =>
            {
                options.Filters.Add(typeof(ValidateModelStateAttribute));
            }).AddFluentValidation(opt =>
            {
                opt.RegisterValidatorsFromAssemblyContaining(typeof(CreateProductRequestValidator));
            });
        }

        public static void AllowAnyOrigin(this IServiceCollection services)
        {
            services.AddCors(c =>
            {
                c.AddPolicy("CORSPolicy",
                    options => options.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
            });
        }

        public static void AllowSelectedOrigins(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddCors(o => o.AddPolicy("CORSPolicy", builder =>
            {
                var origins = configuration["CorsOrigins"].Split(",");
                builder.WithOrigins(origins).AllowCredentials().AllowAnyHeader().AllowAnyMethod();
            }));
        }
    }
}
