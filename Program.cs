
using Azure;
using Azure.AI.OpenAI;

using DocumentQA.Data;
using DocumentQA.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using OpenAI.Chat;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

if (builder.Environment.IsProduction())
{
    var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
    builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
}

builder.Services.AddCors(options =>
{
    options.AddPolicy("DevCors", policy =>
    {
        policy.SetIsOriginAllowed(origin =>
            origin.StartsWith("http://localhost"))
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });

    options.AddPolicy("ProdCors", policy =>
    {
        policy.WithOrigins("https://react-client-theta.vercel.app", "http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

builder.Services.AddControllers();

builder.Services.AddSingleton<ITextChunker, TextChunker>();
builder.Services.AddScoped<IRetrievalService, RetrievalService>();
builder.Services.AddScoped<IRagService, RagService>();
builder.Services.AddSingleton<IEmbeddingService, AzureOpenAIEmbeddingService>();
builder.Services.AddSingleton<ILlmService, AzureOpenAiLlmService>();
builder.Services.AddScoped<IOcrService>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();

    var endpoint = config["AzureOcr:Endpoint"];
    var apiKey = config["AzureOcr:ApiKey"];

    return new OcrService(endpoint, apiKey);
});

builder.Services.AddSingleton<ChatClient>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();

    var endpoint = config["AzureOpenAI:Endpoint"];
    var key = config["AzureOpenAI:ApiKey"];
    var deployment = config["AzureOpenAI:ChatDeployment"];

    var client = new AzureOpenAIClient(
        new Uri(endpoint),
        new AzureKeyCredential(key)
    );

    return client.GetChatClient(deployment);
});

// PDF extraction
builder.Services.AddScoped<IPdfTextExtractor, PdfTextExtractor>();

// SQL Server vector storage
builder.Services.AddDbContext<VectorDbContext>(options =>
{
    options.UseSqlite(builder.Configuration.GetConnectionString("SQLite"));
});

builder.Services.AddScoped<IChunkStore, ChunkStore>();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// JWT authentication
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
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,

        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])),

        // Required to make expired tokens immediately invalid
        ClockSkew = TimeSpan.Zero
    };
});

var app = builder.Build();

// Create SQLite DB + run migrations on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<VectorDbContext>();
    db.Database.Migrate();
}

// 1. CORS FIRST — before ANYTHING else
if (builder.Environment.IsDevelopment())
{
    app.UseCors("DevCors");
}
else
{
    app.UseCors("ProdCors");
}

// 2. Then HTTPS redirection
app.UseHttpsRedirection();

// 3. Then Swagger (optional)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();

// 4. Then authorization
app.UseAuthentication();
app.UseAuthorization();

// 5. Then controllers
app.MapControllers();

app.Run();