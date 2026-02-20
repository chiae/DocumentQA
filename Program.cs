using Azure;
//using Azure.AI.OpenAI;
using DocumentQA.Data;
using DocumentQA.Factories;
using DocumentQA.Services;
using Microsoft.EntityFrameworkCore;
using OpenAI;


var builder = WebApplication.CreateBuilder(args);
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
builder.Services.AddCors(options =>
{
    options.AddPolicy("DevCors", policy =>
    {
        policy.SetIsOriginAllowed(origin =>
            origin.StartsWith("http://localhost"))
              .AllowAnyHeader()
              .AllowAnyMethod();
    });

    options.AddPolicy("ProdCors", policy =>
    {
        policy.WithOrigins("https://react-client-theta.vercel.app/")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddControllers();

// Core RAG pipeline
builder.Services.AddSingleton<ITextChunker, TextChunker>();
builder.Services.AddScoped<IEmbeddingService, FakeEmbeddingService>();
builder.Services.AddScoped<IRetrievalService, RetrievalService>();
builder.Services.AddScoped<IRagService, RagService>();

// LLM
builder.Services.AddSingleton<AzureOpenAiLlmService>();
builder.Services.AddSingleton<OpenAiLmService>();
builder.Services.AddHttpClient<OllamaLmService>();
builder.Services.AddSingleton<LlmServiceFactory>();

builder.Services.AddSingleton<ILlmService>(sp =>
{
    var factory = sp.GetRequiredService<LlmServiceFactory>();
    return factory.Create();
});


// PDF extraction
builder.Services.AddScoped<IPdfTextExtractor, PdfTextExtractor>();

// SQL Server vector storage
builder.Services.AddDbContext<VectorDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("VectorDb")));
builder.Services.AddScoped<IChunkStore, ChunkStore>();


// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

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

// 4. Then authorization
app.UseAuthorization();

// 5. Then controllers
app.MapControllers();

app.Run();