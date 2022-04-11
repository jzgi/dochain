using System;

namespace Chainly.Nodal
{
    public class Peer : Info, IKeyable<short>
    {
        public static readonly Peer Empty = new Peer();

        public const short
            TYP_ASK = 1,
            TYP_ASKED = 2,
            TYP_OKAY = 3,
            TYP_OKAYED = 4,
            TYP_DENIAL = -1,
            TYP_DENIED = -2;

        public static readonly Map<short, string> Typs = new Map<short, string>
        {
            {TYP_ASK, "发出请求"},
            {TYP_ASKED, "收到请求"},
            {TYP_OKAY, "接纳请求"},
            {TYP_OKAYED, "请求成功"},
            {TYP_DENIAL, "回拒请求"},
            {TYP_DENIED, "请求被拒"},
        };

        // unique id within the network
        internal short id;

        // remote address
        internal string url;

        internal string secret;

        public Peer()
        {
        }

        public Peer(ISource cfg)
        {
            Read(cfg);
        }

        public sealed override void Read(ISource s, short mask = 0xff)
        {
            base.Read(s, mask);

            s.Get(nameof(id), ref id);
            s.Get(nameof(url), ref url);
            s.Get(nameof(secret), ref secret);
        }

        public override void Write(ISink s, short mask = 0xff)
        {
            base.Write(s, mask);

            s.Put(nameof(id), id);
            s.Put(nameof(url), url);
            s.Put(nameof(secret), secret);
        }

        public short Key => id;

        public short Id => id;

        public string Name => name;

        public string Tip => tip;

        public short Status => status;

        public string Url => url;

        public DateTime Created => created;

        public bool IsRunning => status == STA_ENABLED;
    }
}