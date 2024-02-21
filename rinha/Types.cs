public record struct Transacao(int Valor, string Tipo, string Descricao)
{
    public readonly bool Valido => 
        (Tipo == "c" || Tipo == "d") &&
        !string.IsNullOrEmpty(Descricao) &&
        Descricao.Length <= 10 &&
        Valor > 0;
}

public record struct TransacaoResponse(int Saldo,int Limite);
public record struct Extrato(Saldo Saldo, IEnumerable<UltimasTransacoes> Ultimas_transacoes);
public record struct Saldo(int Total, DateTime Data_extrato, int Limite);
public record struct UltimasTransacoes(int Valor, string Tipo, string Descricao, DateTime Realizada_em);

[JsonSerializable(typeof(UltimasTransacoes))]
[JsonSerializable(typeof(Saldo))]
[JsonSerializable(typeof(Extrato))]
[JsonSerializable(typeof(Transacao))]
[JsonSerializable(typeof(TransacaoResponse))]
[JsonSerializable(typeof(DateTime))]
[JsonSerializable(typeof(int))]
internal partial class AppJsonSerializerContext : JsonSerializerContext { }