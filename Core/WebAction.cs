﻿using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Extensions.Primitives;

namespace Greatbone.Core
{

    ///
    /// <summary>
    /// The descriptor of an action method.
    /// </summary>
    ///
    public class WebAction : IKeyed
    {
        public WebControl Control { get; }

        readonly Action<WebContext, string> doer;

        readonly ToAttribute[] tos;

        readonly ButtonAttribute button;

        public string Key { get; }

        internal WebAction(WebControl control, MethodInfo mi)
        {
            Control = control;
            Key = mi.Name; // NOTE: strict method name as key here to avoid the default base url trap
            doer = (Action<WebContext, string>)mi.CreateDelegate(typeof(Action<WebContext, string>), control);

            // prepare if attributes
            List<ToAttribute> lst = null;
            foreach (var to in mi.GetCustomAttributes<ToAttribute>())
            {
                if (lst == null) lst = new List<ToAttribute>(8);
                lst.Add(to);
            }
            tos = lst?.ToArray();

            button = mi.GetCustomAttribute<ButtonAttribute>();

        }

        public ButtonAttribute Button => button;

        const string PrivateKey = "3e43a7180";

        internal bool TryDo(WebContext wc, string subscpt)
        {
            // check ifs
            if (tos != null)
            {
                if (wc.Principal == null)
                {
                    wc.StatusCode = 401; // unauthorized
                    wc.AddHeader("WWW-Authenticate", "Bearer");
                    wc.AddHeader("WWW-Authenticate", "Digest realm=\"login\", nonce=\"" + StrUtility.MD5(wc.Connection.RemoteIpAddress.ToString() + ':' + PrivateKey) + "\"");
                    return false;
                }

                for (int i = 0; i < tos.Length; i++)
                {
                    if (!tos[i].Test(wc))
                    {
                        wc.StatusCode = 403; // forbidden
                        return false;
                    }
                }
            }

            // invoke the action method
            wc.Action = this;
            doer(wc, subscpt);
            wc.Action = null;
            return true;
        }

        public override string ToString()
        {
            return Key;
        }
    }
}