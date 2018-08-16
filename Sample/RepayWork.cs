﻿using System;
using System.Data;
using System.Threading.Tasks;
using Greatbone;
using static Greatbone.Modal;
using static Samp.User;

namespace Samp
{
    public abstract class RepayWork<V> : Work where V : RepayVarWork
    {
        protected RepayWork(WorkConfig cfg) : base(cfg)
        {
            CreateVar<V, int>();
        }
    }

    [Ui("个款"), UserAccess(CTR_SPR | CTR_DVR)]
    public class MyCashWork : RepayWork<MyRepayVarWork>
    {
        public MyCashWork(WorkConfig cfg) : base(cfg)
        {
        }

        public void @default(WebContext wc)
        {
            var prin = (User) wc.Principal;
            using (var dc = NewDbContext())
            {
                var arr = dc.Query<Repay>("SELECT * FROM repays WHERE status = 2 AND uid = @1 ORDER BY id DESC", p => p.Set(prin.id));
                wc.GivePage(200, h =>
                {
                    h.TOOLBAR();
                    h.TABLE(arr,
                        () => h.TH("人员").TH("期间").TH("订单").TH("金额").TH("转款"),
                        o => h.TD(Repay.Jobs[o.job], o.uname).TD_().T(o.fro).BR().T(o.till)._TD().TD(o.orders).TD(o.cash).TD(o.payer)
                    );
                });
            }
        }
    }


    [Ui("结款"), UserAccess(CTR_MGR)]
    public class CtrRepayWork : RepayWork<CtrRepayVarWork>
    {
        public CtrRepayWork(WorkConfig cfg) : base(cfg)
        {
        }

        [Ui("未转"), Tool(A, "uk-button-link")]
        public void @default(WebContext wc)
        {
            using (var dc = NewDbContext())
            {
                var arr = dc.Query<Repay>("SELECT * FROM repays WHERE status < 2 ORDER BY id DESC");
                wc.GivePage(200, h =>
                {
                    h.TOOLBAR();
                    h.TABLE(arr,
                        () => h.TH("人员").TH("期间").TH("订单").TH("金额").TH("转款"),
                        o => h.TD(Repay.Jobs[o.job], o.uname).TD_().T(o.fro).BR().T(o.till)._TD().TD(o.orders).TD(o.cash).TD(o.payer)
                    );
                });
            }
        }

        [Ui("已转"), Tool(A, "uk-button-link")]
        public void old(WebContext wc, int page)
        {
            using (var dc = NewDbContext())
            {
                var arr = dc.Query<Repay>("SELECT * FROM repays WHERE status = 2 ORDER BY id DESC LIMIT 30 OFFSET @1", p => p.Set(page * 30));
                wc.GivePage(200, h =>
                {
                    h.TOOLBAR();
                    h.TABLE(arr,
                        () => h.TH("人员").TH("期间").TH("订单").TH("金额").TH("转款"),
                        o => h.TD(Repay.Jobs[o.job], o.uname).TD_().T(o.fro).BR().T(o.till)._TD().TD(o.orders).TD(o.cash).TD(o.payer)
                    );
                });
            }
        }

        [Ui("结算", "结算并生成人员结款单"), Tool(ButtonShow)]
        public async Task calc(WebContext wc)
        {
            DateTime fro; // from date
            DateTime till; // till/before date
            if (wc.GET)
            {
                using (var dc = NewDbContext())
                {
                    fro = (DateTime) dc.Scalar("SELECT till FROM repays ORDER BY id DESC LIMIT 1");
                    wc.GivePane(200, h =>
                    {
                        h.FORM_().FIELDUL_("选择截至日期（不包含）");
                        h.LI_().DATE("起　始", nameof(fro), fro, @readonly: true)._LI();
                        h.LI_().DATE("截　至", nameof(till), DateTime.Today, max: DateTime.Today)._LI();
                        h._FIELDUL()._FORM();
                    });
                }
            }
            else
            {
                var f = await wc.ReadAsync<Form>();
                fro = f[nameof(fro)];
                till = f[nameof(till)];
                using (var dc = NewDbContext(IsolationLevel.ReadUncommitted))
                {
                    dc.Execute(@"INSERT INTO repays (orgid, fro, till, orders, total, cash) SELECT orgid, @1, @2, COUNT(*), SUM(total), SUM(total * 0.994) FROM orders WHERE status = " + Order.ORD_ENDED + " AND ended BETWEEN @1 AND @2 GROUP BY orgid", p => p.Set(fro).Set(till));
                }
                wc.GivePane(200);
            }
        }

        struct Tran
        {
            internal int id;
            internal string orgid;
            internal string mgrwx;
            internal string mgrname;
            internal decimal cash;
        }

        [Ui("转款", "按结款单转款给网点"), Tool(ButtonConfirm)]
        public async Task pay(WebContext wc)
        {
            Roll<Tran> trans = new Roll<Tran>(16);
            using (var dc = NewDbContext())
            {
                // retrieve
                if (dc.Query("SELECT r.id, r.orgid, mgrwx, mgrname, cash FROM repays AS r, orgs AS o WHERE r.orgid = o.id AND r.status = 0"))
                {
                    while (dc.Next())
                    {
                        Tran tr;
                        dc.Let(out tr.id).Let(out tr.orgid).Let(out tr.mgrwx).Let(out tr.mgrname).Let(out tr.cash);
                        trans.Add(tr);
                    }
                }
            }
            // do transfer for each
            User prin = (User) wc.Principal;
            for (int i = 0; i < trans.Count; i++)
            {
                var tr = trans[i];
                string err = await ((SampService) Service).WeiXin.PostTransferAsync(tr.id, tr.mgrwx, tr.mgrname, tr.cash, "订单结款");
                // update data records
                using (var dc = NewDbContext())
                {
                    if (err != null) // error occured
                    {
                        dc.Execute("UPDATE repays SET err = @1 WHERE id = @2", p => p.Set(err).Set(tr.id));
                    }
                    else
                    {
                        // update repay status
                        dc.Execute("UPDATE repays SET err = NULL, payer = @1, status = 1 WHERE id = @2", p => p.Set(prin.name).Set(tr.id));
                    }
                }
            }
            wc.GiveRedirect();
        }
    }
}