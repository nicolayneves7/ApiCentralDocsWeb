using ApiCentralDocsWeb.Data;
using ApiCentralDocsWeb.Interfaces;
using ApiCentralDocsWeb.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using System.Text;

namespace ApiCentralDocsWeb
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // =====================================================
            // BANCO DE DADOS
            // =====================================================

            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(
                    builder.Configuration.GetConnectionString(
                        "DefaultConnection"
                    )
                )
            );


            // =====================================================
            // HTTP CLIENT
            // Necessário para o ChatController acessar a OpenRouter
            // =====================================================

            builder.Services.AddHttpClient();


            // =====================================================
            // SERVICES
            // =====================================================

            builder.Services.AddScoped<UsuarioService>();

            builder.Services.AddScoped<
                ITokenService,
                TokenService
            >();


            // =====================================================
            // JWT
            // =====================================================

            var jwtKey =
                builder.Configuration["Jwt:Key"];

            var jwtIssuer =
                builder.Configuration["Jwt:Issuer"];

            var jwtAudience =
                builder.Configuration["Jwt:Audience"];


            if (string.IsNullOrEmpty(jwtKey))
            {
                throw new InvalidOperationException(
                    "Jwt:Key não foi configurado no appsettings.json"
                );
            }


            builder.Services
                .AddAuthentication(
                    JwtBearerDefaults.AuthenticationScheme
                )
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters =
                        new TokenValidationParameters
                        {
                            ValidateIssuerSigningKey = true,

                            IssuerSigningKey =
                                new SymmetricSecurityKey(
                                    Encoding.UTF8.GetBytes(
                                        jwtKey
                                    )
                                ),

                            ValidateIssuer = true,

                            ValidIssuer =
                                jwtIssuer,

                            ValidateAudience = true,

                            ValidAudience =
                                jwtAudience,

                            ValidateLifetime = true,

                            ClockSkew =
                                TimeSpan.Zero
                        };
                });


            // =====================================================
            // CONTROLLERS
            // =====================================================

            builder.Services.AddControllers();

            builder.Services.AddOpenApi();


            // =====================================================
            // CORS
            // =====================================================

            builder.Services.AddCors(options =>
            {
                options.AddPolicy(
                    "PermitirTudo",
                    policy =>
                    {
                        policy
                            .AllowAnyOrigin()
                            .AllowAnyHeader()
                            .AllowAnyMethod();
                    }
                );
            });


            // =====================================================
            // APLICAÇÃO
            // =====================================================

            var app =
                builder.Build();


            // =====================================================
            // SCALAR / OPENAPI
            // =====================================================

            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();

                app.MapScalarApiReference();
            }


            // =====================================================
            // HTTPS
            // Desativado temporariamente para o Android usar
            // http://10.0.2.2:5083
            // =====================================================

            // app.UseHttpsRedirection();


            // =====================================================
            // CORS
            // =====================================================

            app.UseCors(
                "PermitirTudo"
            );


            // =====================================================
            // AUTENTICAÇÃO
            // =====================================================

            app.UseAuthentication();


            // =====================================================
            // AUTORIZAÇÃO
            // =====================================================

            app.UseAuthorization();


            // =====================================================
            // CONTROLLERS
            // =====================================================

            app.MapControllers();


            // =====================================================
            // TESTE DA API
            // =====================================================

            app.MapGet(
                "/",
                () => "API CentralDocs está online!"
            );


            // =====================================================
            // INICIAR API
            // =====================================================

            app.Run();
        }
    }
}