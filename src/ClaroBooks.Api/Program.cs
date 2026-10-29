using System.Net.Http.Headers;
using ClaroBooks.Api.Handlers;
using ClaroBooks.Application.Interfaces;
using ClaroBooks.Domain.Entities;
using ClaroBooks.Infrastructure.Repositories;
using ClaroBooks.Infrastructure.State;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy.WithOrigins("http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddSingleton<TemporaryChanges<Book>>();
builder.Services.AddSingleton<TemporaryChanges<Author>>();

var baseUrl = builder.Configuration["FakeRestApi:BaseUrl"]
    ?? throw new InvalidOperationException(
        "Falta configurar FakeRestApi:BaseUrl.");

var baseAddress = new Uri(baseUrl, UriKind.Absolute);

void ConfigureFakeRestApi(HttpClient client)
{
    client.BaseAddress = baseAddress;
    client.Timeout = TimeSpan.FromSeconds(30);

    client.DefaultRequestHeaders.Accept.Add(
        new MediaTypeWithQualityHeaderValue("application/json"));
}

builder.Services.AddHttpClient<IBookRepository, BookRepository>(
    ConfigureFakeRestApi);

builder.Services.AddHttpClient<IAuthorRepository, AuthorRepository>(
    ConfigureFakeRestApi);

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseCors("Frontend");

app.UseAuthorization();

app.MapControllers();

app.Run();