using System;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace Blog {

    public class Program {

        static void Main(string[] args) {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            Console.OutputEncoding = Encoding.UTF8;

            switch(args.FirstOrDefault()) {
                case "new-post":
                    DraftNewPost();
                    return;

                case "dynamic-site":
                    CreateWebHost().Run();
                    return;

                case "build-static-site":
                    BlogDataCache.UseSingleton();
                    //Utils.RenderTrackers = true;

                    using(var host = CreateWebHost())
                    using(var generator = new StaticGenerator(host.Services.GetService<BlogDataCache>(), "static-site")) {
                        host.Start();
                        generator.Generate();
                        host.StopAsync();
                        host.WaitForShutdown();
                    }
                    return;
            }

            throw new NotSupportedException("Unknown command");
        }

        static IWebHost CreateWebHost() {
            return WebHost.CreateDefaultBuilder()
                .UseUrls("http://*:8099")
                .UseStartup<Startup>()
                .Build();
        }

        static void DraftNewPost() {
            var file = "posts/" + DateTime.Now.Year + "/" + DateTime.Now.ToString("yyyyMMdd") + "-new.md";
            Utils.EnsureParentDir(file);
            if(!File.Exists(file))
                File.WriteAllLines(file, new[] { "Post Title", "", "Post text" });
        }
    }

}
