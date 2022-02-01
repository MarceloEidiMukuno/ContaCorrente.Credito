using System;
using System.ComponentModel.DataAnnotations;
using ContaCorrente.ApiCredito.Enums;
using ContaCorrente.ApiCredito.Models;

namespace ContaCorrente.ApiCredito.ViewModels
{
    public class CreateCreditoViewModel
    {

        [Required(ErrorMessage = "O campo agência é obrigatorio")]
        public string Agencia { get; set; }

        [Required(ErrorMessage = "O campo conta é obrigatorio")]
        public string Conta { get; set; }
        [Required(ErrorMessage = "O campo valor é obrigatorio")]
        public decimal Valor { get; set; }

        [Required(ErrorMessage = "O campo descrição é obrigatorio")]
        public string Descricao { get; set; }

        [Url(ErrorMessage = "O campo Webhook não possui é um endereço válido")]
        public string? Webhook { get; set; }

        public Transacao ToCreateCredito() => new(
            0,
            Agencia,
            Conta,
            Valor,
            DateTime.Now,
            Descricao,
            Convert.ToInt32(ETipoTransacao.Credito)
        );
    }
}
