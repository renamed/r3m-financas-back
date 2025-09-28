using R3M.Financas.Back.Application.Interfaces;
using R3M.Financas.Back.Domain.Dtos;
using R3M.Financas.Back.Domain.Models;

namespace R3M.Financas.Back.Application.Converters;

public class PeriodoConverter : ConverterBase<PeriodoResponse, Periodo>
{
    public override Periodo Convert(PeriodoResponse request)
    {
        return new Periodo
        {
            Id = request.PeriodoId,
            Fim = request.Fim,
            Inicio = request.Inicio,
            Nome = request.Nome
        };
    }

    public override PeriodoResponse Convert(Periodo domain)
    {
        return new PeriodoResponse
        {
            PeriodoId = domain.Id,
            Fim = domain.Fim,
            Inicio = domain.Inicio,
            Nome = domain.Nome
        };
    }
}
