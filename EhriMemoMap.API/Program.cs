using EhriMemoMap.API.Services;
using EhriMemoMap.Data;
using EhriMemoMap.Services;
using EhriMemoMap.Shared;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.IO.Converters;

var builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new GeoJsonConverterFactory());
});


builder.Services.AddScoped<MapLogicService>();
builder.Services.AddScoped<SolrService>();
builder.Services.AddHttpClient();

// you must set the MEMOMAP_DB environment variable to the connection string
// in windows you can do this by running the following command in a command prompt: 
// setx MEMOMAP_DB "Host=[host];Port=[port];Database=[db_name];User ID=[user_id];Password=[password]"
// in linux you can do this by setting variable in service file 
// Environment=MEMOMAP_DB='Host=[host];Port=[port];Database=[db_name];User ID=[user_id];Password=[password]'
var db = builder.Configuration["MEMOMAP_DB"];
builder.Services.AddDbContext<MemogisContext>(options => options.UseNpgsql(builder.Configuration["MEMOMAP_DB"]));

var cors = "_myAllowSpecificOrigins";

builder.Services.AddCors(options =>
{
    options.AddPolicy(cors, builder =>
    {
        builder.AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader();
    });
});

var app = builder.Build();
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

app.UseCors(cors);

var proxyClient = new HttpClient();

app.MapGet("/", () => "Hello World...");

app.MapGet("/getdistrictstatistics", (bool total, DateTime? timeLinePoint, string city, MapLogicService service, HttpContext context) =>
{
    context.Response.Headers.Append("Cache-Control", "no-cache");
    var parameters = new DistrictStatisticsParameters
    {
        City = city,
        Total = total,
        TimeLinePoint = timeLinePoint
    };
    return service.GetDistrictStatistics(parameters);
});

app.MapPost("/getheatmap", (MapObjectParameters parameters, MapLogicService service) => service.GetHeatmap(parameters));

app.MapPost("/getmapobjects", (MapObjectParameters parameters, MapLogicService service) => service.GetMapObjects(parameters));

app.MapGet("/getwelcomedialogstatistics", (string city, MapLogicService service) => service.GetWelcomeDialogStatistics(city));

app.MapGet("/getvictimlonginfo", (string city, long id, MapLogicService service) => service.GetVictimLongInfo(city, id));

app.MapPost("/getplaces", (PlacesParameters parameters, MapLogicService service) => service.GetPlaces(parameters));

app.MapPost("/getsolrplaces", (SolrQueryParameters parameters, SolrService service) => service.SolrExecuteDocument(parameters));

app.MapGet("/wmsProxy", async (HttpContext context) =>
{
    var url = "https://geodata.ehri-project.eu/geoserver/ehri/wms?" + context.Request.QueryString.Value;
    var imageFileName = url.GetHashCode() + ".png";
    var imageFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Cache/" + imageFileName);

    context.Response.Headers.Append("Cache-Control", "public,max-age=1000000");

    if (File.Exists(imageFilePath))
        return Results.File(File.ReadAllBytes(imageFilePath), "image/png", imageFileName);

    var result = await proxyClient.GetByteArrayAsync(url);
    File.WriteAllBytes(imageFilePath, result);
    return Results.File(result, "image/png", imageFileName);
});

app.Run();
