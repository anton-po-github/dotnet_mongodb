
using System.Text;
using AspNetCore.Identity.MongoDbCore.Extensions;
using AspNetCore.Identity.MongoDbCore.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

public static class IdentityServicesExtensions
{
    public static IServiceCollection AddIdentityServices(this IServiceCollection services, IConfiguration config)
    {

        var dbSettings = config.GetSection(nameof(DatabaseSettings)).Get<DatabaseSettings>()!;

        var mongoDbIdentityConfig = new MongoDbIdentityConfiguration
        {
            MongoDbSettings = new MongoDbSettings
            {
                ConnectionString = dbSettings.ConnectionString,
                DatabaseName = dbSettings.DatabaseName
            },

            IdentityOptionsAction = options =>
            {
                options.Password.RequireDigit = false;
                options.Password.RequiredLength = 8;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequireLowercase = false;

                //lockout
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(10);
                options.Lockout.MaxFailedAccessAttempts = 5;

                options.User.RequireUniqueEmail = true;
            }

        };

        services.ConfigureMongoDbIdentity<IdentityUser, IdentityRole, Guid>(mongoDbIdentityConfig)
        .AddUserManager<UserManager<IdentityUser>>()
        .AddSignInManager<SignInManager<IdentityUser>>()
        .AddRoleManager<RoleManager<IdentityRole>>()
        .AddDefaultTokenProviders();

        services.AddAuthentication(o =>
        {
            o.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            o.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(opt =>
        {
            opt.RequireHttpsMetadata = true;
            opt.SaveToken = true;
            opt.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidIssuer = "https://localhost:5001", // ValidIssuer + JwtSecurityToken.issuer = the SAME important
                ValidAudience = "https://localhost:5001",// ValidAudience + JwtSecurityToken.audience = the SAME important
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("P=9tJAm[Dkq6w#bNKySvF!}Lxf~Z4r]V5`z2RpH/-($,)%Xh8M")),
                ClockSkew = TimeSpan.Zero
            };
        });

        services.AddSwaggerGen();

        services.Configure<DatabaseSettings>(
                  config.GetSection(nameof(DatabaseSettings)));

        services.AddSingleton<IDatabaseSettings>(sp =>
            sp.GetRequiredService<IOptions<DatabaseSettings>>().Value);

        services.AddScoped<EmailService>();
        services.AddScoped<BookService>();
        services.AddScoped<FileService>();

        services.AddCors();
        services.AddControllers();

        return services;

    }

}
