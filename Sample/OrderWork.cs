﻿using System.Threading.Tasks;
using Greatbone;
using static Greatbone.Modal;
using static Samp.Order;
using static Samp.User;

namespace Samp
{
    public abstract class OrderWork<V> : Work where V : OrderVarWork
    {
        protected OrderWork(WorkConfig cfg) : base(cfg)
        {
            CreateVar<V, int>((obj) => ((Order) obj).id);
        }
    }

    public class MyOrderWork : OrderWork<MyOrderVarWork>
    {
        public MyOrderWork(WorkConfig cfg) : base(cfg)
        {
        }

        public void @default(WebContext wc)
        {
            int myid = wc[-1];
            using (var dc = NewDbContext())
            {
                var arr = dc.Query<Order>("SELECT * FROM orders WHERE status BETWEEN 0 AND 4 AND uid = @1 ORDER BY id DESC", p => p.Set(myid));
                wc.GivePage(200, h =>
                {
                    h.BOARD(arr, o =>
                        {
                            h.T("<header class=\"uk-card-header\">");
                            h.T("收货：").T(o.uaddr).SP().T(o.uname).SP().T(o.utel);
                            h.T("</header>");
                            h.T("<main class=\"uk-card-body\">");
                            h.ICO_(css: "uk-width-1-6").T('/').T(o.item).T("/icon")._ICO();
                            h.CNY(o.price).T(o.qty).T(o.unit)._P();
                            h.VARTOOLPAD(css: "uk-width-1-5");
                            h.T("</main>");
                        }
                    );
                }, false, 2, title: "我的订单");
            }
        }

        [Ui("查看历史订单"), Tool(AOpen, size: 2)]
        public void old(WebContext wc, int page)
        {
            int myid = wc[-1];
            using (var dc = NewDbContext())
            {
                var arr = dc.Query<Order>("SELECT * FROM orders WHERE status >= 2 AND custid = @1 ORDER BY id DESC", p => p.Set(myid));
//                GiveBoardPage(wc, arr, false);
            }
        }
    }

    [Ui("订购"), UserAccess(CTR_MGR)]
    public class CtrOrderWork : OrderWork<CtrOrderVarWork>
    {
        public CtrOrderWork(WorkConfig cfg) : base(cfg)
        {
        }

        public void @default(WebContext wc)
        {
            wc.GivePage(200, h =>
            {
                h.TOOLBAR();
                using (var dc = NewDbContext())
                {
                    var arr = dc.Query<Order>("SELECT * FROM orders WHERE status BETWEEN 1 AND 4 ORDER BY id");
                    h.TABLE(arr, null,
                        o => h.TD(o.utel, o.uname).TD(o.item).TD_(css: "uk-text-right").T(o.qty).SP().T(o.unit)._TD().TD(Statuses[o.status])
                    );
                }
            });
        }

        [Ui("撤消", "确认要撤销此单吗？实收款项将退回给买家"), Tool(ButtonPickConfirm)]
        public async Task abort(WebContext wc)
        {
            string orgid = wc[-2];
            int orderid = wc[this];
            short rev = 0;
            decimal cash = 0;
            using (var dc = NewDbContext())
            {
                if (dc.Query1("SELECT rev, cash FROM orders WHERE id = @1 AND status = 1", p => p.Set(orderid)))
                {
                    dc.Let(out rev).Let(out cash);
                }
            }
            if (cash > 0)
            {
                string err = await ((SampService) Service).WeiXin.PostRefundAsync(orderid + "-" + rev, cash, cash);
                if (err == null) // success
                {
                    using (var dc = NewDbContext())
                    {
                        dc.Execute("UPDATE orders SET status = -1, aborted = localtimestamp WHERE id = @1 AND orgid = @2", p => p.Set(orderid).Set(orgid));
                    }
                }
            }
            wc.GiveRedirect("../");
        }
    }

    /// <summary>
    /// The order workset as the <code>supplier</code> role
    /// </summary>
    [Ui("供应"), UserAccess(CTR_SPR)]
    public class GvrOrderWork : OrderWork<GvrOrderVarWork>
    {
        public GvrOrderWork(WorkConfig cfg) : base(cfg)
        {
        }

        public void @default(WebContext wc)
        {
            var prin = (User) wc.Principal;
            var items = Obtain<Map<string, Item>>();
            wc.GivePage(200, h =>
            {
                h.TOOLBAR(@group: 1);
                using (var dc = NewDbContext())
                {
                    dc.Query("SELECT id, item, qty, unit, status FROM orders WHERE status BETWEEN 1 AND 2 AND item IN (SELECT name FROM items WHERE giverid = @1) ORDER BY item, id", p => p.Set(prin.id));
                    string curitme = null;
                    while (dc.Next())
                    {
                        dc.Let(out int id).Let(out string item).Let(out short qty).Let(out short unit).Let(out short status);

                        if (item != curitme)
                        {
                            if (curitme != null)
                            {
                                h.T("</main>");
                                h.TOOLPAD(@group: 2, css: "uk-card-footer");
                                h.T("</form>");
                            }
                            h.T("<form class=\"uk-card uk-card-default\">");
                            h.T("<header class=\"uk-card-header\">").T(item).T("（").T(unit).T("）</header>");
                            h.T("<main class=\"uk-card-body\">");
                        }

                        h.T("<label class=\"checkable\"><input type=\"checkbox\" name=\"").T(id).T("\"").T("><span").T(" class=\"uk-active\"", status == 2).T(">").T(qty).T("</span></label>");

                        curitme = item;
                    }
                    h.T("</main>");
                    h.TOOLPAD(group: 0b0110, css: "uk-card-footer uk-flex-between");
                    h.T("</form>");
                }
            }, false, 2);
        }

        [Ui("概况", group: 1), Tool(ButtonPickShow)]
        public void summary(WebContext wc)
        {
            bool range = true;
        }


        [Ui("排程", "设为排程状态", 0b0010), Tool(ButtonPickShow, css: "uk-button-secondary")]
        public void plan(WebContext wc)
        {
            bool range = true;
            if (wc.GET)
            {
                wc.GivePage(200, h =>
                {
                    h.FORM_().FIELDUL_("个别选择还是区间选择");
                    h.CHECKBOX(nameof(range), range, "选择连续区间");
                    h._FIELDUL()._FORM();
                });
            }
            else
            {
                int[] key = wc.Query[nameof(key)];
                using (var dc = NewDbContext())
                {
                    dc.Sql("UPDATE orders SET status = @1 WHERE id")._IN_(key);
                    dc.Execute();
                }
                wc.GiveRedirect();
            }
        }

        [Ui("解排", "解除排程状态", 0b0010), Tool(ButtonPickShow, css: "uk-button-secondary")]
        public async Task unplan(WebContext wc)
        {
            bool range = false;
            if (wc.GET)
            {
                wc.GivePage(200, h =>
                {
                    h.FORM_().FIELDUL_("是特定选择还是区间选择");
                    h.CHECKBOX(nameof(range), range, "选择连续区间");
                    h._FIELDUL()._FORM();
                });
            }
            else
            {
                int[] key = wc.Query[nameof(key)];
                using (var dc = NewDbContext())
                {
                    dc.Sql("UPDATE orders SET status = @1 WHERE id")._IN_(key);
                    dc.Execute();
                }
                wc.GiveRedirect();
            }
        }

        [Ui("备齐", "解除排程状态", 0b0100), Tool(ButtonPickShow, css: "uk-button-secondary")]
        public async Task ready(WebContext wc)
        {
        }
    }

    /// <summary>
    /// The order workset as the <code>deliverer</code> role
    /// </summary>
    [Ui("派送"), UserAccess(CTR_DVR)]
    public class DvrOrderWork : OrderWork<DvrOrderVarWork>
    {
        public DvrOrderWork(WorkConfig cfg) : base(cfg)
        {
        }

        [Ui("团购"), Tool(A, "uk-button-link")]
        public void @default(WebContext wc, int page)
        {
            var prin = (User) wc.Principal;
            var orgs = Obtain<Map<string, Org>>();
            wc.GivePage(200, h =>
            {
                h.TOOLBAR();
                using (var dc = NewDbContext())
                {
                    dc.Query("SELECT grpid, item, SUM(qty) AS num FROM orders WHERE status = 3 AND grpid IS NOT NULL GROUP BY grpid, item", p => p.Set(prin.id));

                    string curgrpid = null;
                    while (dc.Next())
                    {
                        dc.Let(out string grpid).Let(out string item).Let(out short num);

                        if (grpid != curgrpid)
                        {
                            if (curgrpid != null)
                            {
                                h.T("</main>");
                                h.TOOLPAD(css: "uk-card-footer");
                                h.T("</article>");
                            }
                            h.T("<article class=\"uk-card  uk-card-default\">");
                            h.T("<header class=\"uk-card-header\">").T(orgs[grpid].ToString()).T("</header>");
                            h.T("<main class=\"uk-card-body\">");
                        }

                        h.T("<li>").T(item).T("\"").T(":").T(num).T("</li>");

                        curgrpid = item;
                    }
                    h.T("</main>");
                    h.VARTOOLPAD(@group: 2, css: "uk-card-footer");
                    h.T("</article>");
                }
            }, false, 2);
        }

        [Ui("个单"), Tool(A, "uk-button-link")]
        public void lone(WebContext wc, int page)
        {
            var prin = (User) wc.Principal;
            wc.GivePage(200, h =>
            {
                h.TOOLBAR();
                using (var dc = NewDbContext())
                {
                    dc.Query("SELECT uid, uname, item, SUM(qty) AS num FROM orders WHERE status = 3 AND grpid IS NULL GROUP BY uid, uname, item", p => p.Set(prin.id));
                    int curuid = 0;
                    while (dc.Next())
                    {
                        dc.Let(out int uid).Let(out string uname).Let(out string item).Let(out short num);
                        if (uid != curuid)
                        {
                            if (curuid > 0)
                            {
                                h.T("</main>");
                                h.TOOLPAD(css: "uk-card-footer");
                                h.T("</article>");
                            }
                            h.T("<article class=\"uk-card  uk-card-default\">");
                            h.T("<header class=\"uk-card-header\">").T(uname).T("</header>");
                            h.T("<main class=\"uk-card-body\">");
                        }
                        h.T("<li>").T(item).T("\"").T(":").T(num).T("</li>");
                        curuid = uid;
                    }
                    h.T("</main>");
                    h.VARTOOLPAD(@group: 2, css: "uk-card-footer");
                    h.T("</article>");
                }
            }, false, 2);
        }
    }

    [Ui("订购")]
    public class GrpOrderWork : OrderWork<GrpOrderVarWork>
    {
        public GrpOrderWork(WorkConfig cfg) : base(cfg)
        {
        }

        [Ui("当前"), Tool(A)]
        public void @default(WebContext wc)
        {
            string grpid = wc[-1];
            wc.GivePage(200, h =>
            {
                h.TOOLBAR();
                using (var dc = NewDbContext())
                {
                    var arr = dc.Query<Order>("SELECT * FROM orders WHERE status BETWEEN 1 AND 4 AND grpid = @1 ORDER BY id", p => p.Set(grpid));
                    h.TABLE(arr, null,
                        o => h.TD(o.utel, o.uname).TD(o.item).TD_(css: "uk-text-right").T(o.qty).SP().T(o.unit)._TD().TD(Statuses[o.status])
                    );
                }
            });
        }

        [Ui("查找"), Tool(APrompt)]
        public void find(WebContext wc)
        {
            bool inner = wc.Query[nameof(inner)];
            string tel = null;
            if (inner)
            {
                wc.GivePane(200, h => { h.FORM_().FIELDUL_("手机号").TEL(null, nameof(tel), tel)._FIELDUL()._FORM(); });
            }
            else
            {
                string grpid = wc[-1];
                tel = wc.Query[nameof(tel)];
                using (var dc = NewDbContext())
                {
                    var arr = dc.Query<Order>("SELECT * FROM orders WHERE status BETWEEN 1 AND 4 AND utel = @1", p => p.Set(tel));
                    wc.GivePage(200, h =>
                    {
                        h.TOOLBAR(title: tel);
                        h.TABLE(arr, null,
                            o => h.TD(o.utel, o.uname).TD(o.item).TD_(css: "uk-text-right").T(o.qty).SP().T(o.unit)._TD().TD(Statuses[o.status])
                        );
                    });
                }
            }
        }

        [Ui("收货"), Tool(ButtonPickPrompt)]
        public async Task receive(WebContext wc)
        {
            string grpid = wc[-1];
            if (wc.GET)
            {
                int[] key = wc.Query[nameof(key)];
                wc.GivePane(200, h =>
                {
                    using (var dc = NewDbContext())
                    {
                        dc.Sql("SELECT item, SUM(qty) AS num FROM orders WHERE id")._IN_(key).T(" AND status = 3 AND grpid = @1 GROUP BY item");
                        dc.Query(p => p.SetIn(key).Set(grpid));
                        h.FORM_();

                        h.T("仅列出已送达货品");
                        h.T("<table class=\"uk-table\">");
                        while (dc.Next())
                        {
                            dc.Let(out string item).Let(out short num);
                            h.TD(item).TD(num);
                        }
                        h.T("</table>");
                        h.CHECKBOX("", false, "我确认收货", required: true);
                        h._FORM();
                    }
                });
            }
            else // POST
            {
                int[] key = (await wc.ReadAsync<Form>())[nameof(key)];
                using (var dc = NewDbContext())
                {
                    dc.Sql("UPDATE orders SET status = 4 WHERE id")._IN_(key).T(" AND status = 3 AND grpid = @1");
                    dc.Execute(p => p.SetIn(key).Set(grpid));
                }
                wc.GiveRedirect();
            }
        }

        [Ui("递货"), Tool(ButtonPickPrompt)]
        public void give(WebContext wc)
        {
        }

        [Ui("查历史"), Tool(AOpen, size: 4)]
        public void history(WebContext wc)
        {
        }
    }
}