using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Senparc.Weixin;
using Senparc.Weixin.QY.Entities;


namespace Senparc.Weixin.MP.Sample.WebForms
{
    /// <summary>
    /// send_massage_to_all 的摘要说明
    /// </summary>
    public class send_pictext_to_all : IHttpHandler
    {

        public void ProcessRequest(HttpContext context)
        {
            string users = context.Request.Params["users"] ?? "@all";
            string content = context.Request.Params["content"] ?? "";
            //QY.Entities.Article news=new QY.Entities.Article();
            List<QY.Entities.Article> news_list = new List<QY.Entities.Article>();
            string[] content_array = content.Split(new string[]{"$"},StringSplitOptions.None);
            //news.Title="这个是测试测试测试测试测试测试测试";
            for (int i = 0; i < content_array.Length; i = i + 2)
            {
                QY.Entities.Article news = new QY.Entities.Article();
                news.Title = content_array[i].Trim();
                news.Url = content_array[i + 1].Trim();
                news_list.Add(news);
            }

            
            context.Response.ContentType = "text/plain";
            QY.CommonAPIs.AccessTokenContainer.Register("wx9b25d5460920c234", "5Iau-uGnijCNSwoQXlocce6bMBohZ2W0ZdA4bWg1x2uObwhb6fWgAK9nNLKaM5pr");
            string txt = context.Request.Params["content"] ?? " ";
            string tk = QY.CommonAPIs.AccessTokenContainer.TryGetToken("wx9b25d5460920c234", "5Iau-uGnijCNSwoQXlocce6bMBohZ2W0ZdA4bWg1x2uObwhb6fWgAK9nNLKaM5pr");
            QY.AdvancedAPIs.Mass.SendNews(tk, users, "", "", "1", news_list, 0);
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