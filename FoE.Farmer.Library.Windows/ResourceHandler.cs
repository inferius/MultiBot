using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CefSharp;
using CefSharp.Callback;

namespace FoE.Farmer.Library.Windows
{
    internal class FoFSchemeHandlerFactory : ISchemeHandlerFactory
    {
        public const string SchemeName = "FoFScheme";

        public IResourceHandler Create(IBrowser browser, IFrame frame, string schemeName, IRequest request)
        {
            return new FoFResourceHandler();
        }
    }

    public class FoFResourceHandler : IResourceHandler
    {
        public void Dispose()
        {
        }

        public bool ProcessRequest(IRequest request, ICallback callback)
        {
            //var uri = new Uri(request.Url);
            //var fileName = uri.AbsolutePath;

            //Task.Run(() =>
            //{
            //    using (callback)
            //    {
            //        Stream stream = null;
            //        var postDataElement = request.PostData.Elements.FirstOrDefault();
            //        stream = ResourceHandler.GetMemoryStream(postDataElement == null ? "null" : postDataElement.GetBody(), Encoding.UTF8);

                    

            //        if (stream == null)
            //        {
            //            callback.Cancel();
            //        }
            //        else
            //        {
            //            //Reset the stream position to 0 so the stream can be copied into the underlying unmanaged buffer
            //            stream.Position = 0;
            //            //Populate the response values - No longer need to implement GetResponseHeaders (unless you need to perform a redirect)
            //            //ResponseLength = stream.Length;
            //            //MimeType = "text/html";
            //            //StatusCode = (int)HttpStatusCode.OK;
            //            //Stream = stream;

            //            callback.Continue();
            //        }
            //    }
            //});

            return true;
        }

        public void GetResponseHeaders(IResponse response, out long responseLength, out string redirectUrl)
        {
            responseLength = 0;
            redirectUrl = null;
        }

        public bool ReadResponse(Stream dataOut, out int bytesRead, ICallback callback)
        {
            bytesRead = 0;
            return false;
        }

        public bool CanGetCookie(Cookie cookie)
        {
            return false;
        }

        public bool CanSetCookie(Cookie cookie)
        {
            return false;
        }

        public void Cancel()
        {
        }

        public bool Open(IRequest request, out bool handleRequest, ICallback callback)
        {
            handleRequest = false;
            return false;
        }

        public bool Skip(long bytesToSkip, out long bytesSkipped, IResourceSkipCallback callback)
        {
            bytesSkipped = 0;
            return false;
        }

        public bool Read(Stream dataOut, out int bytesRead, IResourceReadCallback callback)
        {
            bytesRead = 0;
            return false;
        }
    }
}
