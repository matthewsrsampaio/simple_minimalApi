using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseInMemoryDatabase("TarefasDB")
);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//Endpoint teste
app.MapGet("/", () => "Olá Mundo");

app.MapGet("frases", async () =>
    await new HttpClient().GetStringAsync("https://ron-swanson-quotes.herokuapp.com/v2/quotes")
);

//Endpoint teste das frases engraçadas do swanson
app.MapGet("/tarefas", async (AppDbContext db) =>
{
    return await db.Tarefas.ToListAsync();
});

//Endpoint para salvar tarefas na memoria
app.MapPost("/tarefas", async(Tarefa tarefa, AppDbContext db) =>
{
    db.Tarefas.Add(tarefa);
    await db.SaveChangesAsync();
    return Results.Created($"/tarefas/{tarefa.Id}", tarefa);
});

//Ache a tarefa pelo ID
app.MapGet("tarefas/{id}", async (int id, AppDbContext db) => await db.Tarefas.FindAsync(id) is Tarefa tarefa ? Results.Ok(tarefa) : Results.NotFound());

//Ache as tarefas que foram concluidas
app.MapGet("/tarefas/concluida", async (AppDbContext db) =>
    await db.Tarefas.Where(t => t.IsConcluida).ToListAsync()
);

app.MapPut("/tarefas/{id}", async (int id, Tarefa inputTarefa, AppDbContext db) =>
{
    var tarefa = await db.Tarefas.FindAsync(id);

    if(tarefa is null) 
        return Results.NotFound();

    tarefa.Nome = inputTarefa.Nome;
    tarefa.IsConcluida = inputTarefa.IsConcluida;

    await db.SaveChangesAsync();

    return Results.NoContent();
});

app.MapDelete("/tarefas/{id}", async (int id, AppDbContext db) =>
{
    if(await db.Tarefas.FindAsync(id) is Tarefa tarefa)
    {
        db.Tarefas.Remove(tarefa);
        await db.SaveChangesAsync();
        return Results.Ok(tarefa);
    }
    return Results.NotFound();
});

app.Run();

class Tarefa
{
    public int Id { get; set; }
    public string? Nome { get; set; }
    public bool IsConcluida { get; set; }
}

class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {

    }

    public DbSet<Tarefa> Tarefas => Set<Tarefa>();
}