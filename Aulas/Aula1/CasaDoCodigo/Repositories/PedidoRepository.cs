using CasaDoCodigo.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace CasaDoCodigo.Repositories
{
    public interface IPedidoRepository
    {
        Pedido GetPedido();
        void AddItem(string codigo);
    }
    public class PedidoRepository : BaseRepository<Pedido>, IPedidoRepository
    {
        IHttpContextAccessor contextAcessor;

        public PedidoRepository(ApplicationContext contexto, IHttpContextAccessor contextAcessor) : base(contexto)
        {
            this.contextAcessor = contextAcessor;
        }

        public Pedido GetPedido()
        {
            var idPedido = GetPedidoId();

            var pedido = dbSet
                .Include(x => x.Itens)
                    .ThenInclude(x => x.Produto)
                .Where(x => x.Id == idPedido)
                .SingleOrDefault();

            if (pedido == null)
            {
                pedido = new Pedido();
                dbSet.Add(pedido);
                contexto.SaveChanges();
                SetPedidoId(pedido.Id);
            }

            return pedido;
        }

        public void AddItem(string codigo)
        {
            var produto = contexto.Set<Produto>().
                            Where(x => x.Codigo == codigo)
                            .SingleOrDefault();

            if (produto == null)
            {
                throw new ArgumentException("Produto não encontrado");
            }

            var pedido = GetPedido();

            var itemPedido = contexto.Set<ItemPedido>()
                                .Where(i => i.Produto.Codigo == codigo && i.Pedido.Id == pedido.Id)
                                .SingleOrDefault();

            if (itemPedido == null)
            {
                itemPedido = new ItemPedido(pedido, produto, 1, produto.Preco);

                contexto.Set<ItemPedido>().
                    Add(itemPedido);

                contexto.SaveChanges();
            }
        }

        public int? GetPedidoId()
        {
            return contextAcessor.HttpContext.Session.GetInt32("idPedido");
        }

        public void SetPedidoId(int idPedido)
        {
            contextAcessor.HttpContext.Session.SetInt32("idPedido", idPedido);
        }
    }
}
