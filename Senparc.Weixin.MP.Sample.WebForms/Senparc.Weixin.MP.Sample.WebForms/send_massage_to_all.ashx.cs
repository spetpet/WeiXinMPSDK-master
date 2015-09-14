using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Senparc.Weixin;


namespace Senparc.Weixin.MP.Sample.WebForms
{
    /// <summary>
    /// send_massage_to_all 的摘要说明
    /// </summary>
    public class send_massage_to_all : IHttpHandler
    {

        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "text/plain";
            QY.CommonAPIs.AccessTokenContainer.Register("wx9b25d5460920c234", "5Iau-uGnijCNSwoQXlocce6bMBohZ2W0ZdA4bWg1x2uObwhb6fWgAK9nNLKaM5pr");
            string txt = context.Request.Params["content"] ?? " ";
            string tk=QY.CommonAPIs.AccessTokenContainer.TryGetToken("wx9b25d5460920c234", "5Iau-uGnijCNSwoQXlocce6bMBohZ2W0ZdA4bWg1x2uObwhb6fWgAK9nNLKaM5pr");
            QY.AdvancedAPIs.Mass.SendText(tk, "@all", "", "", "1", txt);
            context.Response.Write(QY.CommonAPIs.AccessTokenContainer.CheckRegistered("wx9b25d5460920c234").ToString());
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}