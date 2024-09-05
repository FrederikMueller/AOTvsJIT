using System;
using System.Diagnostics;
using System.Linq;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var startTime = Stopwatch.GetTimestamp();

var builder = WebApplication.CreateSlimBuilder(args);

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
});

var app = builder.Build();

var lifetime = app.Services.GetRequiredService<IHostApplicationLifetime>();

lifetime.ApplicationStarted.Register(() =>
{
    Console.WriteLine(Stopwatch.GetElapsedTime(startTime).TotalMilliseconds);
});

var sampleTodos = new Todo[]
{
    new(1, "Walk the dog"),
    new(2, "Do the dishes", DateOnly.FromDateTime(DateTime.Now)),
    new(3, "Do the laundry", DateOnly.FromDateTime(DateTime.Now.AddDays(1))),
    new(4, "Clean the bathroom"),
    new(5, "Clean the car", DateOnly.FromDateTime(DateTime.Now.AddDays(2)))
};

var todosApi = app.MapGroup("/todos");
todosApi.MapGet("/", () => sampleTodos);

todosApi.MapPost("/lead", ([FromBody]LeadDTO todo) =>
{
    if (string.IsNullOrEmpty(todo.firstName) || string.IsNullOrEmpty(todo.lastName) || string.IsNullOrEmpty(todo.email) || string.IsNullOrEmpty(todo.phone) || string.IsNullOrEmpty(todo.company))
    {
        return Results.BadRequest("All fields are required");
    }
    if (!todo.email.Contains('@'))
    {
        return Results.BadRequest("Invalid email");
    }
    var lead = new Lead(GenerateId(), todo.firstName, todo.lastName, todo.email, todo.phone, todo.company, GenerateId());
    return Results.Created($"/lead/{lead.Id}", lead);
});

todosApi.MapGet("/{id}", (int id) =>
    sampleTodos.FirstOrDefault(a => a.Id == id) is { } todo
        ? Results.Ok(todo)
        : Results.NotFound());

app.Run();
return;

int GenerateId() => Random.Shared.Next(1, 1_000_000);

public record Todo(int Id, string? Title, DateOnly? DueBy = null, bool IsComplete = false);

public record LeadDTO(string firstName, string lastName, string email, string phone, string company);
public record Lead(int Id, string firstName, string lastName, string email, string phone, string company, int accountId);

[JsonSerializable(typeof(Todo[]))]
[JsonSerializable(typeof(LeadDTO))]
[JsonSerializable(typeof(Lead))]
internal partial class AppJsonSerializerContext : JsonSerializerContext;


