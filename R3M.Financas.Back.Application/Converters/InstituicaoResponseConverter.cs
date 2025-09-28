using R3M.Financas.Back.Domain.Dtos;
using R3M.Financas.Back.Domain.Models;

namespace R3M.Financas.Back.Application.Converters;

public class InstituicaoResponseConverter : ConverterBase<InstituicaoResponse, Instituicao>
{
    public override Instituicao Convert(InstituicaoResponse dto)
    {
        return new Instituicao
        {
            Id = dto.InstituicaoId,
            InstituicaoCredito = dto.Credito,
            LimiteCredito = dto.LimiteCredito,
            Nome = dto.Nome,
            SaldoAtual = dto.Saldo,
            SaldoInicial = dto.Saldo
        };
    }

    public override InstituicaoResponse Convert(Instituicao domain)
    {
        return new InstituicaoResponse
        {
            Credito = domain.InstituicaoCredito,
            InstituicaoId = domain.Id,
            LimiteCredito = domain.LimiteCredito,
            Nome = domain.Nome,
            Saldo = domain.SaldoAtual
        };
    }
}
