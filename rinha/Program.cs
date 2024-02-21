var builder = WebApplication.CreateSlimBuilder(args);

builder.Services.ConfigureHttpJsonOptions(o =>
    o.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default));

Dictionary<int, int> clientes = new() {
    { 1, 1000 * 100 }, { 2, 800 * 100 }, { 3, 10000 * 100 },
    { 4, 100000 * 100 }, { 5, 5000 * 100 }
};

builder.Services.AddNpgsqlSlimDataSource(Environment.GetEnvironmentVariable("CONNECTION_STRING") ?? "connection string!");

var app = builder.Build();

app.MapPost("/clientes/{id}/transacoes", async (int id, Transacao request, NpgsqlConnection connection) =>
{
    if (!clientes.TryGetValue(id, out int limite)) return Results.NotFound();
    if (!request.Valido) return Results.UnprocessableEntity();
    string script = DatabaseScripts.QUERY_CREDITAR;
    if (request.Tipo == "d")
    {
        if (request.Valor > limite)
        {
            return Results.UnprocessableEntity();
        }
        script = DatabaseScripts.QUERY_DEBITAR;
    }

    using var command = new NpgsqlCommand(script, connection);
    command.Parameters.AddWithValue("id", id);
    command.Parameters.AddWithValue("valor", request.Valor);
    command.Parameters.AddWithValue("limite", -limite);
    command.Parameters.AddWithValue("descricao", request.Descricao);

    if (connection.State == ConnectionState.Closed)
        await connection.OpenAsync();

    var reader = await command.ExecuteReaderAsync();
    await reader.ReadAsync();
    if (reader.GetBoolean(1)) return Results.UnprocessableEntity();
    return Results.Ok(new TransacaoResponse(reader.GetInt32(0), limite));

});

app.MapGet("clientes/{id}/extrato", async (int id, NpgsqlConnection connection) =>
{
    if (!clientes.TryGetValue(id, out int value)) return Results.NotFound();

    using var command = new NpgsqlCommand(DatabaseScripts.QUERY_EXTRATO, connection);
    command.Parameters.AddWithValue("id", id);

    if (connection.State == ConnectionState.Closed)
        await connection.OpenAsync();

    using var reader = await command.ExecuteReaderAsync();
    await reader.ReadAsync();
    var saldo = reader.GetInt32(0);
    var dataConsulta = reader.GetDateTime(3);
    var transacoes = new List<UltimasTransacoes>(10);
    while (await reader.ReadAsync())
    {
        transacoes.Add(new(
            reader.GetInt32(0),
            reader.GetString(1),
            reader.GetString(2),
            reader.GetDateTime(3)
        ));
    }
    return Results.Ok(new Extrato(new(saldo, dataConsulta, value), transacoes));
});

app.Run();