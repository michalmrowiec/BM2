﻿using System.Text;
using BM2.Application.Contracts.Persistence;
using BM2.Application.Contracts.Persistence.Base;
using BM2.Application.Contracts.Services;
using BM2.Domain.Entities;
using BM2.Domain.Entities.UserProfile;
using BM2.Infrastructure.Repositories;
using BM2.Infrastructure.Repositories.Base;
using BM2.Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Sieve.Services;

namespace BM2.Infrastructure;

public static class InfrastructureInstallation
{
    public static void AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var authenticationSettings = new AuthenticationSettings();
        configuration.GetSection("Authentication").Bind(authenticationSettings);

        services.AddAuthentication(option =>
        {
            option.DefaultAuthenticateScheme = "Bearer";
            option.DefaultScheme = "Bearer";
            option.DefaultChallengeScheme = "Bearer";
        }).AddJwtBearer(cfg =>
        {
            cfg.RequireHttpsMetadata = false;
            cfg.SaveToken = true;
            cfg.TokenValidationParameters = new TokenValidationParameters
            {
                ValidIssuer = authenticationSettings.JwtIssuer,
                ValidAudience = authenticationSettings.JwtIssuer,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(authenticationSettings.JwtKey)),
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateIssuerSigningKey = true,
                ValidateLifetime = true
            };
        });

        services.AddDbContext<BM2DbContext>(
            opt => opt.UseSqlServer(configuration.GetConnectionString("BM2DB")));

        services.AddSingleton(authenticationSettings);
        services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<ISieveProcessor, BM2SieveProcessor>();

        services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<ICurrencyRepository, CurrencyRepository>();
        services.AddScoped<IRecordStatusRepository, RecordStatusRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IAuditLoginRepository, AuditLoginRepository>();
        services.AddScoped<IWalletRepository, WalletRepository>();
        services.AddScoped<IAccountRepository, AccountRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<IWalletCategoryRelationRepository, WalletCategoryRelationRepository>();
        services.AddScoped<ITagRepository, TagRepository>();
        services.AddScoped<IWalletTagRelationRepository, WalletTagRelationRepository>();
        services.AddScoped<IRecordTagRelationRepository, RecordTagRelationRepository>();
    }
}