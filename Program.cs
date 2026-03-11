using Azure;
//using Azure.AI.OpenAI;
using DocumentQA.Data;
using DocumentQA.Factories;
using DocumentQA.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
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
        policy.WithOrigins("https://react-client-theta.vercel.app")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();

    });
});

builder.Services.AddControllers();

// Core RAG pipeline
builder.Services.AddSingleton<ITextChunker, TextChunker>();
builder.Services.AddScoped<IRetrievalService, RetrievalService>();
builder.Services.AddScoped<IRagService, RagService>();

// LLM
builder.Services.AddSingleton<GroqLlmService>();
builder.Services.AddSingleton<AzureOpenAiLlmService>();
builder.Services.AddSingleton<OpenAiLmService>();
builder.Services.AddHttpClient<OllamaLmService>();
builder.Services.AddSingleton<LlmServiceFactory>();
// Embedding
builder.Services.AddSingleton<IEmbeddingService, AzureOpenAIEmbeddingService>();
builder.Services.AddSingleton<GroqEmbeddingService>();

builder.Services.AddSingleton<ILlmService>(sp =>
{
    var factory = sp.GetRequiredService<LlmServiceFactory>();
    return factory.Create();
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