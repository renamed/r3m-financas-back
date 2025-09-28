using R3M.Financas.Back.Domain.Dtos;
using R3M.Financas.Back.Domain.Models;

namespace R3M.Financas.Back.Application.Converters;

public class InstituicaoRequestConverter : ConverterBase<InstituicaoRequest, Instituicao>
{
    public override Instituicao Convert(InstituicaoRequest dto)
    {
        return new Instituicao
        {
            DataSaldoInicial = dto.DataSaldoInicial,
            InstituicaoCredito = dto.InstituicaoCredito,
            LimiteCredito = dto.LimiteCredito,
            Nome = dto.Nome,
            SaldoAtual = dto.SaldoInicial,
            SaldoInicial = dto.SaldoInicial
        };
    }

    public override InstituicaoRequest Convert(Instituicao domain)
    {
        return new InstituicaoRequest
        {
            DataSaldoInicial = domain.DataSaldoInicial,
            InstituicaoCredito = domain.InstituicaoCredito,
            LimiteCredito = domain.LimiteCredito,
            Nome =domain.Nome,
            SaldoInicial = domain.SaldoInicial
        };
    }
}
