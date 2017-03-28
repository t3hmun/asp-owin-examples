using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.Owin.Hosting;
using Owin;

namespace asp_owin_examples
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    internal class Program
    {
        private static void Main(string[] args)
        {
            var uri = "http://localhost:8910";

            using (WebApp.Start<Startup>(uri))
            {
                Console.WriteLine("## Server started, press any key to shutdown.");
                Console.ReadKey();
                Console.WriteLine("## Shutting Down.");
            }
        }

        internal class Startup
        {
            public void Configuration(IAppBuilder app)
            {
                var webApiConfig = new HttpConfiguration();
                webApiConfig.Routes.MapHttpRoute(
                    "apiRoute",
                    "api/{controller}/{id}",
                    new {id = RouteParameter.Optional});
                app.UseWebApi(webApiConfig);

                app.Use<SampleMiddleware>();
                // This next middlware would normally be written using app.Run(context=>{...}) because it does not call next.
                app.Use(async (conetext, next) =>
                {
                    Console.WriteLine("Start of lambda middleware.");
                    await conetext.Response.WriteAsync("Please say cheese.");
                    Console.WriteLine("End of lambda middleware.");
                });
            }
        }

        /// <summary>
        ///     A Middleware that writes a response if the path contains `hello` otherwise it calls the next middleware.
        /// </summary>
        internal class SampleMiddleware
        {
            private readonly AppFunc _next;

            public SampleMiddleware(AppFunc next)
            {
                _next = next;
            }

            public async Task Invoke(IDictionary<string, object> environment)
            {
                Console.WriteLine("Start of SampleMiddleWare");

                if (environment["owin.RequestPath"].ToString().Contains("cheese"))
                {
                    using (var writer = new StreamWriter((Stream) environment["owin.ResponseBody"]))
                    {
                        await writer.WriteAsync("Salutations.");
                    }
                }
                else
                {
                    Console.WriteLine("Before SampleMiddleWare next()");
                    await _next(environment);
                    Console.WriteLine("After SampleMiddleWare next()");
                }

                Console.WriteLine("End of SampleMiddleWare");
            }
        }
    }
}