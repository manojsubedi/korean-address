using AddressSearch.Models;
using AddressSearch.Services;

var builder = WebApplication.CreateBuilder(args);

// Configure AddressApi settings from appsettings.json
builder.Services.Configure<AddressApiConfig>(builder.Configuration.GetSection("AddressApi"));

// Register HttpClient + AddressService
builder.Services.AddHttpClient<AddressService>();

var app = builder.Build();

// Serve static files from wwwroot
app.UseStaticFiles();

// === API Endpoints ===

// GET /api/address?keyword=...&page=1&count=10&language=kr&confmKey=...
app.MapGet("/api/address", async (
    string keyword,
    int page,
    int count,
    string language,
    string? confmKey,
    AddressService addressService) =>
{
    if (string.IsNullOrWhiteSpace(keyword))
        return Results.BadRequest(new { error = "keyword is required" });

    try
    {
        var result = await addressService.SearchAsync(keyword, page, count, language, confmKey);
        return Results.Ok(result);
    }
    catch (InvalidOperationException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
    catch (Exception ex)
    {
        return Results.Problem(ex.Message);
    }
});

// GET /api/config - check if keys are configured
app.MapGet("/api/config", (IConfiguration config) =>
{
    var krKey = config["AddressApi:KrKey"] ?? "";
    var enKey = config["AddressApi:EnKey"] ?? "";
    return Results.Ok(new
    {
        kr_key_set = !string.IsNullOrEmpty(krKey),
        en_key_set = !string.IsNullOrEmpty(enKey)
    });
});

// Fallback: serve index.html for root
app.MapFallbackToFile("index.html");

app.Run();
