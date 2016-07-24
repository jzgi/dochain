﻿using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Greatbone.Core
{
  ///
  /// A web service controller that may contain sub-controllers and/or a multiplexer.
  ///
  public abstract class WebService : WebSub, IParent, ICacheRealm
  {
    public static readonly Checker AnyOne = (t) => true;

    public static readonly Checker<IZone> AnyOneZone = (t, z) => true;

    // the attached sub controllers, if any
    private Set<WebSub> _subs;

    // the attached multiplexer, if any
    private IHub _hub;

    private Set<Publish> _publishes;

    private Set<Subscribe> _subscribes;

    // the async client
    private HttpClient[] client;


    protected WebService(WebCreationContext wcc) : base(wcc)
    {
    }


    public TSub AddSub<TSub>(string key, Checker checker) where TSub : WebSub
    {
      if (_subs == null)
      {
        _subs = new Set<WebSub>(16);
      }
      // create instance by reflection
      Type type = typeof(TSub);
      ConstructorInfo ci = type.GetConstructor(new[] {typeof(WebCreationContext)});
      if (ci == null)
      {
        throw new WebException(type + ": the WebCreationContext parameterized constructor not found");
      }
      WebCreationContext wcc = new WebCreationContext
      {
        Key = key,
        StaticPath = Path.Combine(StaticPath, key),
        Parent = this,
        Service = this
      };
      TSub sub = (TSub) ci.Invoke(new object[] {wcc});
      sub.Checker = checker;

      _subs.Add(sub);

      //
      // check declared event handler methods

      return sub;
    }

    public THub MountHub<THub, TZone>(Checker<TZone> checker) where THub : WebHub<TZone> where TZone : IZone
    {
      // create instance
      Type type = typeof(THub);
      ConstructorInfo ci = type.GetConstructor(new[] {typeof(WebCreationContext)});
      if (ci == null)
      {
        throw new WebException(type + ": the WebCreationContext parameterized constructor not found");
      }
      WebCreationContext wcc = new WebCreationContext
      {
        StaticPath = Path.Combine(StaticPath, "-"),
        Parent = this,
        Service = this
      };
      THub mux = (THub) ci.Invoke(new object[] {wcc});

      // call the initialization and set
      _hub = mux;
      return mux;
    }


    public void AddPublish(string topic)

    {
      if (_publishes == null)
      {
        _publishes = new Set<Publish>(32);
      }
    }


    // NOTE: for long-pulling support, a sending acitity must be initailized based on the context
    //
    public Task Handle(HttpContext context)
    {
      string host = context.Request.Headers["Host"];
      Console.WriteLine("Host: " + host);

      if (host.EndsWith("9090")) // message
      {
        // msg
      }
      else // request & response
      {
        using (WebContext wc = new WebContext(context))
        {
          Handle(context.Request.Path.Value.Substring(1), wc);
        }
      }

      return null;
    }

    public override void Handle(string relative, WebContext wc)
    {
//            Console.WriteLine("relative: " + relative);
      int slash = relative.IndexOf('/');
      if (slash == -1) // without a slash then handle it locally
      {
        WebAction a = GetAction(relative);
//                Console.WriteLine("action: " + a);
        a?.Do(wc);
      }
      else // not local then sub & mux
      {
        string dir = relative.Substring(0, slash);
        if (dir.StartsWith("-") && dir.EndsWith("-")) // mux
        {
          if (_hub == null)
          {
            // send not implemented
          }
          string zoneKey = dir.Substring(1, dir.Length - 2);
          IZone zone;
          if (_hub.ResolveZone(zoneKey, out zone))
          {
            wc.Zone = zone;
            _hub.Handle(relative.Substring(slash + 1), wc);
          }
        }
        else
        {
          WebSub sub;
          if (_subs.TryGet(dir, out sub))
          {
            sub.Handle(relative.Substring(slash + 1), wc);
          }
        }
      }
    }

    public long ModifiedOn { get; set; }


    ///
    /// sends an event to a target service
    ///
    public void Publish(string topic, string subarg, IData msg)
    {
      Socket s;
    }
  }
}