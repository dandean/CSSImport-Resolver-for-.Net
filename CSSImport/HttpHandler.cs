using System;
using System.Web;
using System.Web.Caching;
using System.Text;
using System.IO.Compression;

namespace CSSImport
{
    public class HttpHandler : IHttpHandler
    {
        const string AcceptEncodingHeader = "Accept-Encoding";
        const string ContentEncodingHeader = "Content-Encoding";

        enum HttpCompressionType
        {
            Deflate,
            Gzip,
            None
        }

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
            EnableCompression(Context);

            string path = Request.PhysicalPath;
            string result = string.Empty;

            if (Cache[path] == null) {
                result = ProcessAndCacheFile(path);
            } else {
                result = (string)Cache[path];
            }

            Response.Write(result);
        }

        string ProcessAndCacheFile(string path)
        {
            // Process the file
            Resolver resolver = new Resolver();
            string result = resolver.ProcessFile(path);

            // Cache css based on file list.
            Cache.Insert(path,
                result ?? string.Empty,
                new CacheDependency(resolver.GetListOfFiles()),
                DateTime.Now.AddDays(30),
                Cache.NoSlidingExpiration);
            return result;
        }

        void Configure(HttpContext context)
        {
            if (context == null) {
                throw new ArgumentNullException("context");
            }

            Context = context;
            Request = context.Request;
            Response = context.Response;
            Cache = context.Cache;
        }

        static void EnableCompression(HttpContext context)
        {
            if (context == null) {
                throw new ArgumentNullException("context");
            }

            HttpCompressionType type = GetSupportedCompression(context.Request);

            switch (type) {
                case HttpCompressionType.Deflate:
                    context.Response.Filter = new DeflateStream(context.Response.Filter, CompressionMode.Compress, true);
                    break;

                case HttpCompressionType.Gzip:
                    context.Response.Filter = new GZipStream(context.Response.Filter, CompressionMode.Compress, true);
                    break;
            }

            string contentEncoding = GetContentEncoding(type);
            if (contentEncoding != null) {
                context.Response.AppendHeader(ContentEncodingHeader, contentEncoding);
            }

            context.Response.Cache.VaryByHeaders[AcceptEncodingHeader] = true;
        }

        static string GetContentEncoding(HttpCompressionType type)
        {
            switch (type) {
                case HttpCompressionType.Deflate:
                    return "deflate";

                case HttpCompressionType.Gzip:
                    return "gzip";

                case HttpCompressionType.None:
                default: return null;
            }
        }

        static HttpCompressionType GetSupportedCompression(HttpRequest request)
        {
            string acceptEncoding = request.Headers[AcceptEncodingHeader];
            if (acceptEncoding != null) {
                acceptEncoding = acceptEncoding.ToLower();
                if (acceptEncoding.Contains("*") || acceptEncoding.Contains("deflate")) {
                    return HttpCompressionType.Deflate;

                } else if (acceptEncoding.Contains("gzip")) {
                    return HttpCompressionType.Gzip;
                }
            }

            return HttpCompressionType.None;
        }
    }
}
