using System;
using System.Web;
using System.Web.Caching;
using System.Text;

namespace CSSImport
{
    public class HttpHandler : IHttpHandler
    {
        HttpContext Context { get; set; }
        HttpRequest Request { get; set; }
        HttpResponse Response { get; set; }
        Cache Cache { get; set; }

        public bool IsReusable
        {
            get { return true; }
        }

        public void ProcessRequest(HttpContext context)
        {
            Configure(context);
            Response.ContentType = "text/css";
            Response.ContentEncoding = Encoding.UTF8;

            string path = Request.PhysicalPath;

            Resolver resolver = new Resolver();
            string result = resolver.ProcessFile(path);
            Response.Write(result);
        }

        void Configure(HttpContext context)
        {
            Context = context;
            Request = context.Request;
            Response = context.Response;
            Cache = context.Cache;
        }
    }
}
