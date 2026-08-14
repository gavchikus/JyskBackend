using System.Reflection;
using System.Text;
using JyskBackend.Database;
using JyskBackend.Interfaces;
using JyskBackend.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("Default")
                       ?? "Data Source=jysk.db";

builder.Services.AddDbContext<JyskDbContext>(options => options.UseSqlite(connectionString));

builder.Services.AddScoped<IProductsService, ProductsService>();
builder.Services.AddScoped<ICategoriesService, CategoriesService>();
builder.Services.AddScoped<IVariantsService, VariantsService>();
builder.Services.AddScoped<IReviewsService, ReviewsService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IOrdersService, OrdersService>();
builder.Services.AddScoped<IRoomsService, RoomsService>();
builder.Services.AddScoped<ICollectionsService, CollectionsService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Jysk Store API Docs",
        Version = "v1",
        Description = "Повна специфікація ендпоінтів інтернет-магазину меблів для інтеграції з фронтендом"
    });

    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT заголовок авторизації за схемою Bearer. Введіть: 'Bearer {ваш_токен}' у поле нижче.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

// Ключ підпису токенів більше не лежить у коді: Development бере його з
// appsettings.Development.json, Production — лише зі змінної оточення Jwt__Key
// або user-secrets. Без ключа застосунок навмисно не стартує.
var jwtSection = builder.Configuration.GetSection("Jwt");
var jwtKey = jwtSection["Key"];
if (string.IsNullOrWhiteSpace(jwtKey) || Encoding.UTF8.GetByteCount(jwtKey) < 32)
{
    throw new InvalidOperationException(
        "Jwt:Key не налаштовано або він коротший за 32 байти. " +
        "Задайте змінну оточення Jwt__Key чи секрет користувача перед запуском.");
}

var jwtIssuer = jwtSection["Issuer"] ?? "JyskBackend";
var jwtAudience = jwtSection["Audience"] ?? "JyskFrontend";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = jwtIssuer,
        ValidateAudience = true,
        ValidAudience = jwtAudience,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        ClockSkew = TimeSpan.FromMinutes(1)
    };
});

builder.Services.AddAuthorization();

// Замість AllowAnyOrigin — явний список джерел із конфігурації.
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
                     ?? ["http://localhost:5173"];

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(allowedOrigins).AllowAnyMethod().AllowAnyHeader();
    });
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Jysk API Specification v1");
    c.RoutePrefix = "swagger";
});

app.UseCors();

// У Development фронт ходить на http-профіль (5118). Примусовий редирект на https
// ламав би CORS-preflight, тому вмикаємо його лише поза розробкою.
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<JyskDbContext>();
    // Раніше схему треба було накатувати руками — тепер міграції застосовуються на старті.
    await dbContext.Database.MigrateAsync();
    await DbInitializer.SeedAsync(dbContext, app.Configuration);
}

app.Run();
