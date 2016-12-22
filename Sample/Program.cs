﻿using System.Collections.Generic;
using Greatbone.Core;

namespace Greatbone.Sample
{
    public class Program
    {
        ///
        /// The program's entry point.
        ///
        public static void Main(string[] args)
        {
            DbConfig pg = new DbConfig
            {
                host = "106.14.45.109",
                port = 5432,
                username = "postgres",
                password = "GangShang721004",
                queue = false
            };

            WebAuth<Shop, Shop> auth = new WebAuth<Shop, Shop>(0x4a78be76, 0x1f0335e2);

            List<WebService> services = new List<WebService>(4);

            WebConfig cfg;

            if ((cfg = new WebConfig("www")
            {
                outer = "http://127.0.0.1:8080",
                inner = "http://127.0.0.1:7070",
                refs = new Obj{
                    new Member("shop-01", "http://127.0.0.1:7072"),
                },
                db = pg
            }).LoadJson())
            {
                services.Add(new WwwService(cfg) { Auth = auth });
            }

            if ((cfg = new WebConfig("shop")
            {
                shard = "01",
                outer = "http://127.0.0.1:8081",
                inner = "http://127.0.0.1:7071",
                refs = new Obj{
                    new Member("shop-01", "http://127.0.0.1:7072"),
                },
                db = pg
            }).LoadJson())
            {
                services.Add(new ShopService(cfg) { Auth = auth });
            }

            WebService.Run(services);
        }
    }
}