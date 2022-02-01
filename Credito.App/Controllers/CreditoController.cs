using System.Linq;
using System.Threading.Tasks;
using ContaCorrente.ApiCredito.Clients;
using ContaCorrente.ApiCredito.Data;
using ContaCorrente.ApiCredito.Enums;
using ContaCorrente.ApiCredito.Extensions;
using ContaCorrente.ApiCredito.Services;
using ContaCorrente.ApiCredito.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ContaCorrente.ApiCredito.Controllers
{
    [ApiController]
    [Route("v1/credito")]
    public class CreditoController : ControllerBase
    {
        [HttpGet("")]
        public async Task<IActionResult> GetAsync(
            [FromServices] TransacaoDataContext context,
            [FromQuery] int page = 0,
            [FromQuery] int pageSize = 25)
        {
            var credito = await context
                .Transacoes
                .AsNoTracking()
                .Where(x => x.TipoTransacao == (int)(ETipoTransacao.Credito))
                .Select(x => new ListTransacoesViewModel
                {
                    Agencia = x.Agencia,
                    Conta = x.Conta,
                    Valor = x.Valor,
                    Descricao = x.Descricao,
                    DataCriacao = x.DataCriacao,
                    TipoTransacao = ((ETipoTransacao)x.TipoTransacao).ToString()
                })
                .Skip(page * pageSize)
                .Take(pageSize)
                .OrderByDescending(x => x.DataCriacao)
                .ToListAsync();

            return Ok(new ResultViewModel<dynamic>(new
            {
                total = credito.Count,
                page,
                pageSize,
                credito
            }));
        }

        [HttpPost("")]
        public async Task<IActionResult> PostAsync(
            [FromBody] CreateCreditoViewModel model,
            [FromServices] NotificationService notificationService,
            [FromServices] MessageBusService messageBus,
            [FromServices] TransacaoDataContext context)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ResultViewModel<string>(ModelState.GetErros()));

            try
            {
                var tran = model.ToCreateCredito();
                if (!tran.Valor_isValid())
                    return BadRequest(new ResultViewModel<string>("O valor da transação não pode ser menor ou igual a zero."));

                if (!tran.Descricao_Minima())
                    return BadRequest(new ResultViewModel<string>($"A descrição deve conter no minimo {tran.Qtde_Minimia_Caracteres_Descricao()} caracteres."));

                var contaIsValid = await ContaApiClient.GetContaAsync(tran.Agencia, tran.Conta);
                if (!contaIsValid)
                    return BadRequest(new ResultViewModel<string>("Conta não localizada."));

                await context.Transacoes.AddAsync(tran);
                await context.SaveChangesAsync();

                await messageBus.SendAsync(tran, "transacao");

                var result = new ResultViewModel<dynamic>(new
                {
                    Agencia = tran.Agencia,
                    Conta = tran.Conta,
                    Valor = tran.Valor,
                    Descricao = tran.Descricao,
                    DataCriacao = tran.DataCriacao.ToString("dd/MM/yyyy HH:mm:ss"),
                    TipoTransacao = ((ETipoTransacao)tran.TipoTransacao).ToString()
                });

                if (!string.IsNullOrEmpty(model.Webhook))
                    await notificationService.NotifyAsync(model.Webhook, result);


                return Created("", result);
            }
            catch (Exception)
            {
                return StatusCode(500, new ResultViewModel<string>("Falha interna servidor"));
            }
        }

    }
}
