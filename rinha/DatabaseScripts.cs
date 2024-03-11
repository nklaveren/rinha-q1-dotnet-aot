public class DatabaseScripts
{
    public const string QUERY_CREDITAR = @"SELECT saldo, erro FROM creditar(@id, @valor, @descricao)";
    public const string QUERY_DEBITAR = @"SELECT saldo, erro FROM debitar(@id, @valor, @descricao)";
    public const string QUERY_EXTRATO = @"
    SELECT valor, null as tipo, null as descricao, now() as realizada_em FROM saldos WHERE cliente_id = @id
    UNION ALL 
    SELECT * FROM (
        SELECT valor, tipo, descricao, realizada_em FROM transacoes WHERE cliente_id = @id ORDER BY id DESC LIMIT 10
    ) t";
}