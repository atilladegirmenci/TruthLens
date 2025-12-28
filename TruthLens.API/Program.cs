using TruthLens.Core.Interfaces;
using TruthLens.Services;

var builder = WebApplication.CreateBuilder(args);


// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy =>
        {
            policy.AllowAnyOrigin()  
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        });
});

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<IOcrService, OcrService>();
builder.Services.AddScoped<IScraperService, ScraperService>();
builder.Services.AddScoped<IAiService, AiService>();
builder.Services.AddScoped<IGoogleVerificationService, GoogleVerificationService>();
builder.Services.AddScoped<ILLMService, LLMService>();
builder.Services.AddHttpClient();


var app = builder.Build();

// Normalde sadece Development modunda hata detayını gösteririz.
// Ama hatayı bulmak için bunu ŞİMDİ ZORLA AÇIYORUZ.
app.UseDeveloperExceptionPage();

// Swagger her zaman açık kalsın (hata ayıklama bitene kadar)
app.UseSwagger();
app.UseSwaggerUI();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("AllowAll");

app.UseAuthorization();

app.MapControllers();

app.Run();
