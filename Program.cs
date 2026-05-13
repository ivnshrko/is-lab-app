var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddEnvironmentVariables();

var app = builder.Build();

var notes = new List<Note>();

app.MapGet("/", () => "Hello World!");

app.MapGet("/health", () =>
{
    return Results.Json(new
    {
        status = "ok",
        time = DateTime.Now
    });
});

app.MapGet("/version", () =>
{
    return Results.Json(new
    {
        name = builder.Configuration["App:Name"] ?? "IsLabApp",
        version = builder.Configuration["App:Version"] ?? "1.0"
    });
});

app.MapGet("/api/notes", () =>
{
    return Results.Json(notes);
});

app.MapGet("/api/notes/{id}", (int id) =>
{
    var note = notes.FirstOrDefault(n => n.Id == id);

    return note is null
        ? Results.NotFound()
        : Results.Json(note);
});

app.MapPost("/api/notes", (Note note) =>
{
    if (string.IsNullOrWhiteSpace(note.Title))
    {
        return Results.BadRequest("Title is required");
    }

    note.Id = notes.Count + 1;
    note.CreatedAt = DateTime.Now;

    notes.Add(note);

    return Results.Ok(note);
});

app.MapDelete("/api/notes/{id}", (int id) =>
{
    var note = notes.FirstOrDefault(n => n.Id == id);

    if (note is null)
    {
        return Results.NotFound();
    }

    notes.Remove(note);

    return Results.Ok();
});
app.MapGet("/db/ping", () =>
{
    try
    {
        var connectionString = builder.Configuration.GetConnectionString("Mssql");

        return Results.Json(new
        {
            status = "error",
            message = "SQL Server not connected yet",
            connectionString = connectionString
        });
    }
    catch (Exception ex)
    {
        return Results.Json(new
        {
            status = "error",
            message = ex.Message
        });
    }
});
app.Run();

class Note
{
    public int Id { get; set; }

    public string Title { get; set; } = "";

    public string Text { get; set; } = "";

    public DateTime CreatedAt { get; set; }
}