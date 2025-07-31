using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using MovieWorld.WebApi;
using MovieWorld.WebApi.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<MovieDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("SqlConnection")));

builder.Services.AddAutoMapper(typeof(MappingProfiles));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseStaticFiles(new StaticFileOptions()
{
  FileProvider = new PhysicalFileProvider(@"C:\Users\Yusuf\Desktop\Web\MovieWorld"),
  RequestPath = "/StaticFiles"
});

app.UseAuthorization();

app.MapControllers();

app.Run();
