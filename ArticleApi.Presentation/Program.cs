using ArticleApi.Application.Services;
using ArticleApi.Domain.Interfaces;
using ArticleApi.Infrastructure.Data;
using ArticleApi.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// добавляем сервисы
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// БД
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ArticleDbContext>(options =>
    options.UseNpgsql(connectionString));

// Dependency Injection
// Регистрируем все репозитории
builder.Services.AddScoped<IArticleRepository, ArticleRepository>();
builder.Services.AddScoped<ISectionRepository, SectionRepository>();
builder.Services.AddScoped<ITagRepository, TagRepository>();

// Сервисы
builder.Services.AddScoped<ArticleService>();
builder.Services.AddScoped<SectionService>();
builder.Services.AddScoped<TagService>();

var app = builder.Build();

// миграции
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ArticleDbContext>();
    await context.Database.MigrateAsync();
}


// для среды dev отображаем сваггер
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();