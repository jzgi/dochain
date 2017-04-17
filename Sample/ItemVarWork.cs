using System.Threading.Tasks;
using Greatbone.Core;

namespace Greatbone.Sample
{
    public abstract class ItemVarWork : Work
    {
        protected ItemVarWork(WorkContext wc) : base(wc)
        {
        }

        public void @default(ActionContext ac)
        {
            string shopid = ac[0];
            int id = ac[this];

            using (var dc = Service.NewDbContext())
            {
                dc.Sql("SELECT ").columnlst(Order.Empty)._("FROM orders WHERE id = @1 AND shopid = @2");
                if (dc.Query(p => p.Set(id).Set(shopid)))
                {
                    var order = dc.ToArray<Order>();
                }
                else
                {
                }
            }
        }

        public void _icon_(ActionContext ac)
        {
            string shopid = ac[typeof(ShopVarWork)];
            string name = ac[this];

            using (var dc = Service.NewDbContext())
            {
                if (dc.Query1("SELECT icon FROM items WHERE shopid = @1 AND name = @2", p => p.Set(shopid).Set(name)))
                {
                    var byteas = dc.GetByteAs();
                    if (byteas.Count == 0) ac.Give(204); // no content 
                    else
                    {
                        StaticContent cont = new StaticContent(byteas);
                        ac.Give(200, cont);
                    }
                }
                else ac.Give(404); // not found           
            }
        }

        public void cannel(ActionContext ac)
        {
            string shopid = ac[0];
            int orderid = ac[this];

            using (var dc = Service.NewDbContext())
            {
                dc.Sql("SELECT ").columnlst(Order.Empty)._("FROM orders WHERE id = @1 AND shopid = @2");
                if (dc.Query(p => p.Set(orderid).Set(shopid)))
                {
                    var order = dc.ToArray<Order>();
                }
                else
                {
                }
            }
        }
    }

    public class PubItemVarWork : ItemVarWork
    {
        public PubItemVarWork(WorkContext wc) : base(wc)
        {
        }

        public void add(ActionContext ac)
        {
            string shopid = ac[typeof(ShopVarWork)];
            string name = ac[this];
            if (ac.GET)
            {
                using (var dc = Service.NewDbContext())
                {
                    if (dc.Query1("SELECT price, min, step FROM items WHERE shopid = @1 AND name = @2", p => p.Set(shopid).Set(name)))
                    {
                        var price = dc.GetDecimal();
                        var min = dc.GetShort();
                        short qty = min;
                        var step = dc.GetShort();
                        string note = null;
                        ac.GiveFormPane(200, f =>
                        {
                            f.NUMBER(nameof(qty), qty, label: "数量", min: min, step: step);
                            f.TEXTAREA(nameof(note), note, label: "附加说明");
                        });
                    }
                    else ac.Give(404); // not found           
                }
            }
            else
            {
            }
        }
    }

    public class OprItemVarWork : ItemVarWork
    {
        public OprItemVarWork(WorkContext wc) : base(wc)
        {
        }

        [Ui("修改", UiMode.AnchorDialog)]
        public async Task edit(ActionContext ac)
        {
            if (ac.GET)
            {
                ac.GiveFormPane(200, Item.Empty);
            }
            else // post
            {
                var item = await ac.ReadObjectAsync<Item>();
                item.shopid = ac[typeof(ShopVarWork)];
                using (var dc = Service.NewDbContext())
                {
                    const int proj = -1 ^ Projection.BIN;
                    dc.Sql("INSERT INTO items")._(Item.Empty, proj)._VALUES_(Item.Empty, proj)._("");
                    if (dc.Execute(p => item.WriteData(p, proj)) > 0)
                    {
                        ac.Give(201); // created
                    }
                    else
                    {
                        ac.Give(500); // internal server error
                    }
                }
            }
        }

        [Ui("图片", UiMode.AnchorDialog)]
        public async Task icon(ActionContext ac)
        {
            if (ac.GET)
            {
                ac.GiveFormPane(200, Item.Empty);
            }
            else // post
            {
                var item = await ac.ReadObjectAsync<Item>();
                item.shopid = ac[typeof(ShopVarWork)];
                using (var dc = Service.NewDbContext())
                {
                    const int proj = -1 ^ Projection.BIN;
                    dc.Sql("INSERT INTO items")._(Item.Empty, proj)._VALUES_(Item.Empty, proj)._("");
                    if (dc.Execute(p => item.WriteData(p, proj)) > 0)
                    {
                        ac.Give(201); // created
                    }
                    else
                    {
                        ac.Give(500); // internal server error
                    }
                }
            }
        }
    }
}