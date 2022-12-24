using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CefSharp;
using CefSharp.Handler;

namespace FoE.Farmer.Library.Windows
{
    public class FoFResourceRequestHandler : ResourceRequestHandler
    {
        public static readonly string VersionNumberString =
            $"Chromium: {Cef.ChromiumVersion}, CEF: {Cef.CefVersion}, CefSharp: {Cef.CefSharpVersion}";


        protected override CefReturnValue OnBeforeResourceLoad(IWebBrowser browserControl, IBrowser browser, IFrame frame, IRequest request, IRequestCallback callback)
        {
            //Example of how to set Referer
            // Same should work when setting any header

            // For this example only set Referer when using our custom scheme
            //if (url.Scheme == CefSharpSchemeHandlerFactory.SchemeName)
            //{
            //    //Referrer is now set using it's own method (was previously set in headers before)
            //    request.SetReferrer("http://google.com", ReferrerPolicy.Default);
            //}

            //Example of setting User-Agent in every request.
            //var headers = request.Headers;

            //var userAgent = headers["User-Agent"];
            //headers["User-Agent"] = userAgent + " CefSharp";

            //request.Headers = headers;

            //NOTE: If you do not wish to implement this method returning false is the default behaviour
            // We also suggest you explicitly Dispose of the callback as it wraps an unmanaged resource.
            //callback.Dispose();
            //return false;

            //NOTE: When executing the callback in an async fashion need to check to see if it's disposed
            if (!callback.IsDisposed)
            {
                using (callback)
                {
                    if (request.Method == "POST")
                    {
                        if (request.Url != null)
                        {
                            var uri = request.Url.ToString();
                            if (uri.IndexOf("/json") != -1)
                            {
                                Requests.UserKey = uri.Substring(uri.IndexOf("json?h=") + "json?h=".Length);
                                //Requests.TemplateRequest = (HttpWebRequest)WebRequest.Create(uri);

                                foreach (var key in request.Headers.AllKeys)
                                {
                                    Requests.TemplateRequestHeader[key] = request.Headers[key];
                                }
                                Requests.TemplateRequestHeader["Uri"] = uri;
                                Requests.TemplateRequestHeader["Referrer"] = request.ReferrerUrl;

                                MainPage.ShowCookies();

                                //Requests.SID = request.c
                                //using (var sw = new StreamWriter("_temp.txt", true))
                                //{
                                //    foreach (string name in request.Headers.AllKeys)
                                //    {
                                //        sw.WriteLine($"{name}: {request.Headers[name]}");
                                //    }
                                //    sw.WriteLine();
                                //    sw.WriteLine();
                                //}
                            }
                        }


                        //using (var postData = request.PostData)
                        //{
                        //    if (postData != null)
                        //    {
                        //        var elements = postData.Elements;

                        //        var charSet = request.GetCharSet();

                        //        foreach (var element in elements)
                        //        {
                        //            if (element.Type == PostDataElementType.Bytes)
                        //            {
                        //                var body = element.GetBody(charSet);
                        //            }
                        //        }
                        //    }
                        //}
                    }

                    //Note to Redirect simply set the request Url
                    //if (request.Url.StartsWith("https://www.google.com", StringComparison.OrdinalIgnoreCase))
                    //{
                    //    request.Url = "https://github.com/";
                    //}

                    //Callback in async fashion
                    //callback.Continue(true);
                    //return CefReturnValue.ContinueAsync;
                }
            }

            return base.OnBeforeResourceLoad(browserControl, browser, frame, request, callback);

        }

        protected override bool OnResourceResponse(IWebBrowser browserControl, IBrowser browser, IFrame frame, IRequest request, IResponse response)
        {
            //NOTE: You cannot modify the response, only the request
            // You can now access the headers
            //var headers = response.ResponseHeaders;

            Uri url;
            if (Uri.TryCreate(request.Url, UriKind.Absolute, out url) == false)
            {
                throw new Exception("Request to \"" + request.Url + "\" can't continue, not a valid URI");
            }

            //NOTE: When executing the callback in an async fashion need to check to see if it's disposed
            if (request.Method == "POST")
            {
                if (request.Url != null)
                {
                    var uri = request.Url.ToString();

                    if (uri.IndexOf("/json") != -1)
                    {
                        Requests.UserKey = uri.Substring(uri.IndexOf("json?h=") + "json?h=".Length);

                    }
                }

            }


            return base.OnResourceResponse(browserControl, browser, frame, request, response);
        }


        protected override void OnResourceLoadComplete(IWebBrowser browserControl, IBrowser browser, IFrame frame, IRequest request, IResponse response, UrlRequestStatus status, long receivedContentLength)
        {

            if (request.Method == "POST")
            {
                if (request.Url != null)
                {
                    var uri = request.Url.ToString();


                    if (uri.IndexOf("/json") != -1)
                    {


                    }
                }


            }

            base.OnResourceLoadComplete(browserControl, browser, frame, request, response, status, receivedContentLength);
        }

    }
}
