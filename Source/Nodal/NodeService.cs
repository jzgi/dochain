using System;
using System.Threading.Tasks;
using SkyChain.Web;
using static SkyChain.Nodal.NodeUtility;

namespace SkyChain.Nodal
{
    /// <summary>
    /// A web service that realizes inter-peer communication. 
    /// </summary>
    public class NodeService : WebService, INodeService
    {
        public async Task oncall(WebContext wc)
        {
            // resolve peer id
            string crypto = wc.Header(X_CRYPTO);
            short peerid = 0;

            var f = await wc.ReadAsync<JObj>();

            // var ctx = Chain.NewChainContext(IsolationLevel.ReadUncommitted, wc);
            // if (o.IsAsync)
            // {
            //     return await o.DoAsync(ctx);
            // }
            // else
            // {
            //     return o.Do(ctx);
            // }
        }

        protected bool NEW(NodeContext cc, string table)
        {
            return false;
        }

        protected bool MOD(NodeContext cc, string table)
        {
            return false;
        }

        protected bool FRO(NodeContext cc, string table)
        {
            return false;
        }

        protected bool BAK(NodeContext cc, string table)
        {
            return false;
        }

        protected bool ABT(NodeContext cc, string table)
        {
            return false;
        }

        protected bool COM(NodeContext cc, string table)
        {
            return false;
        }


        public void onask(WebContext wc)
        {
            throw new NotImplementedException();
        }

        public void onaccept(WebContext wc)
        {
            throw new NotImplementedException();
        }

        public void onquit(WebContext wc)
        {
            throw new NotImplementedException();
        }

        public void onapprovequit(WebContext wc)
        {
            throw new NotImplementedException();
        }

        public void ontransfer(WebContext wc)
        {
            throw new NotImplementedException();
        }

        void INodeService.onpoll(WebContext wc)
        {
            // veriify 
            var peerid = Home.Self.id;
            if (wc.HeaderShort(X_CRYPTO) != peerid)
            {
                wc.Give(409); // conflict
            }

            var blockid = wc.HeaderInt(X_BLOCK_ID); // asked block id
            if (!blockid.HasValue)
            {
                wc.Give(400); // bad request
                return;
            }

            using var dc = NewDbContext();
            // dc.Sql("SELECT * FROM chain.archive WHERE peerid = @1 AND seq >= @2 AND seq < @2 + 1000 ORDER BY seq");
            // var arr = await dc.QueryAsync<Archival>(p => p.Set(peerid).Set(WeaveSeq(blockid.Value, 0)));
            // var j = new JsonContent(true, 1024 * 256);
            // try
            // {
            //     j.ARR_();
            //     foreach (var o in arr)
            //     {
            //         j.OBJ_();
            //         // j.Put(nameof(o.seq), o.seq);
            //         // j.Put(nameof(o.acct), o.acct);
            //         // j.Put(nameof(o.name), o.name);
            //         // j.Put(nameof(o.remark), o.remark);
            //         // j.Put(nameof(o.amt), o.amt);
            //         // j.Put(nameof(o.bal), o.bal);
            //         // j.Put(nameof(o.stamp), o.stamp);
            //         // j.Put(nameof(o.cs), o.cs);
            //         // j.Put(nameof(o.blockcs), o.blockcs);
            //         j._OBJ();
            //     }
            //     j._ARR();
            // }
            // finally
            // {
            //     j.Clear();
            // }

            // wc.Give(200, j);
        }

        public void discover(WebContext wc)
        {
            throw new NotImplementedException();
        }
    }
}