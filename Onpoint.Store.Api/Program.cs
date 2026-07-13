using BuildingBlocks.Extensions;
using Onpoint.Store.Application.Extensions;
using Onpoint.Store.Infrastructure.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddBuildingBlocksServices();
builder.Services.AddLocalizationServices();
builder.Services.AddSwaggerConfiguration();

// Infrastructure 
builder.Services.AddDatabaseConfiguration(builder.Configuration);
builder.Services.AddInfrastructureServices();

builder.Services.AddJwtAuthentication(builder.Configuration);

builder.Services.AddApplicationServices(builder.Configuration);

var cloudinaryConfig = builder.Configuration.GetSection("CloudinarySettings");
var cloudinaryAccount = new CloudinaryDotNet.Account(
    cloudinaryConfig["CloudName"],
    cloudinaryConfig["ApiKey"],
    cloudinaryConfig["ApiSecret"]
);
builder.Services.AddSingleton(new CloudinaryDotNet.Cloudinary(cloudinaryAccount));

var app = builder.Build();
await app.MigrateDatabaseAsync();

app.UseGlobalExceptionHandler();
app.UseRequestLocalizationConfiguration();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseCors("AllowAll");
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();