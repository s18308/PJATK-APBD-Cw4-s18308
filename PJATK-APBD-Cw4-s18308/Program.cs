using PJATK_APBD_Cw4_s18308.Infrastructure;
using PJATK_APBD_Cw4_s18308.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(opt => opt.SuppressMapClientErrors = true);
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddScoped<IDbService, DbService>();

builder.Services.AddDbContext<DatabaseContext>(opt =>
{
    opt.UseSqlServer(
        builder.Configuration.GetConnectionString("Default"),
        // Zmieniłem lokalizację tabeli z migracjami ze względu na istnienie już takiej tabeli w moim głównym chemacie
        x => x.MigrationsHistoryTable("EFCore_Migrations", builder.Configuration["DB:DefaultSchema"])
    );
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();