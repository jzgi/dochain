﻿using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Greatbone.Core
{
    ///
    /// The connect to a remote peer service that the current service depends on.
    ///
    public class WebClient : HttpClient, IRollable
    {
        WebService service;

        // subdomain name or a reference name
        readonly string name;

        //
        // event polling & processing
        //

        private bool status;

        // tick count
        private int lastConnect;


        public WebClient(string name, string raddr)
        {
            this.name = name;
            string addr = raddr.StartsWith("http") ? raddr : "http://" + raddr;
            BaseAddress = new Uri(addr);
        }

        public string Name => name;

        internal void Schedule()
        {
            // check the status

            if (lastConnect < 100)
            {
                // create and run task
                Task.Run(() =>
                {
                    PollAsync();
                });
            }
        }

        public Task<HttpResponseMessage> CallGet(ICallerContext ctx, string uri)
        {
            HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Get, uri);
            if (ctx.IsBearer)
            {
                req.Headers.Add("Authorization", "Bearer " + "");
            }
            else
            {
                req.Headers.Add("Cookie", "");
            }
            return SendAsync(req, HttpCompletionOption.ResponseContentRead);
        }

        public Task<HttpResponseMessage> CallPostJson<D>(ICallerContext ctx, string uri, D dat) where D : IData
        {
            HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Post, uri);
            if (ctx.IsBearer)
            {
                req.Headers.Add("Authorization", "Bearer " + "");
            }
            else
            {
                req.Headers.Add("Cookie", "");
            }
            JsonContent cont = new JsonContent(true, true);
            dat.Dump(cont);
            req.Content = cont;
            return SendAsync(req, HttpCompletionOption.ResponseContentRead);
        }


        internal async void PollAsync()
        {
            HttpResponseMessage resp = await GetAsync("*");

            byte[] cont = await resp.Content.ReadAsByteArrayAsync();

            // parse and process evetns
            int pos;
            for (;;)
            {
                
                long id;
                string name = "";
                DateTime time;
                WebEvent handler = null;
                if (service.Events.TryGet(name, out handler))
                {
                    WebEventContext ec = new WebEventContext();
                    handler.Do(ec);
                }
            }
        }
    }
}