using BuildingBlocks.Extensions; 
using BuildingBlocks.Middlewares;
using Onpoint.Store.Infrastructure.Extensions; 

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// BuildingBlocks
builder.Services.AddBuildingBlocksServices();
builder.Services.AddLocalizationServices();
builder.Services.AddAutoMapperServices();
builder.Services.AddFluentValidationConfiguration();
builder.Services.AddSwaggerConfiguration();
builder.Services.AddJwtAuthentication(builder.Configuration);

//  Infrastructure 
builder.Services.AddDatabaseConfiguration(builder.Configuration); 
builder.Services.AddInfrastructureServices(); 


var app = builder.Build();

// Middlewares
app.UseGlobalExceptionHandler();
app.UseRequestLocalizationConfiguration();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();