using ContaCorrente.ApiCredito.Models;
using Microsoft.EntityFrameworkCore;

namespace ContaCorrente.ApiCredito.Data
{

    public class TransacaoDataContext : DbContext
    {

        public TransacaoDataContext(DbContextOptions options) : base(options) { }

        public DbSet<Transacao> Transacoes { get; set; }

    }
}