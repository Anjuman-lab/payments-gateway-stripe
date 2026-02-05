using Microsoft.EntityFrameworkCore;
using Payments.API.Data;
using Payments.API.Clients;
using Payments.API.Options;
using Payments.API.Services;
using Serilog;
using Polly.Extensions.Http;
using Polly;
using System.Net;

var builder = WebApplication.CreateBuilder(args);

// -------------------- Logging --------------------
builder.Host.UseSerilog((ctx, cfg) => cfg.ReadFrom.Configuration(ctx.Configuration));

// -------------------- EF Core --------------------
builder.Services.AddDbContext<AppDbContext>(o =>
    o.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// -------------------- Options --------------------
builder.Services.Configure<CardConnectOptions>(builder.Configuration.GetSection("CardConnect"));

// -------------------- HTTP Client --------------------
builder.Services.AddHttpClient<ICardConnectClient, CardConnectClient>((sp, client) =>
{
    var opts = sp.GetRequiredService<
        Microsoft.Extensions.Options.IOptions<CardConnectOptions>>().Value;

    client.BaseAddress = new Uri(opts.BaseUrl.TrimEnd('/') + "/");
})
.AddPolicyHandler(HttpPolicyExtensions
    .HandleTransientHttpError()
    .OrResult(msg => (int)msg.StatusCode == 429 || msg.StatusCode == HttpStatusCode.BadGateway)
    .WaitAndRetryAsync(new[]
    {
        TimeSpan.FromSeconds(1),
        TimeSpan.FromSeconds(2),
        TimeSpan.FromSeconds(5)
    }));

// -------------------- Services --------------------
builder.Services.AddScoped<IPaymentService, PaymentService>();

// -------------------- Controllers --------------------
builder.Services.AddControllers();

// -------------------- CORS for React Dev --------------------
// You can define origins in appsettings.json, or use defaults
var allowedOrigins = new[] { "http://localhost:3000" };

builder.Services.AddCors(opt =>
{
    opt.AddPolicy("AllowReactApp", p => p
        .WithOrigins(allowedOrigins)
        .AllowAnyHeader()
        .AllowAnyMethod());
});

// -------------------- Swagger --------------------
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// -------------------- Build the App --------------------
var app = builder.Build();

app.UseSerilogRequestLogging();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowReactApp");   // ✅ this should match the policy name above
app.UseAuthorization();
app.MapControllers();

// Optional: simple health check
app.MapGet("/api/health", () => Results.Ok("OK"));

// -------------------- Run --------------------
app.Run();
