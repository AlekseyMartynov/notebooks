using System;
using System.Linq;
using System.Text.Unicode;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.WebEncoders;

namespace Blog {

    public class Startup {
        public Startup(IConfiguration configuration) {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services) {
            services.AddMvc();

            // https://github.com/aspnet/HttpAbstractions/issues/315
            services.Configure<WebEncoderOptions>(options => options.TextEncoderSettings = new TextEncoderSettings(UnicodeRanges.All));

            BlogDataCache.Register(services);
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env) {
            app.UseDeveloperExceptionPage();
            app.UseStaticFiles(new StaticFileOptions {
                OnPrepareResponse = r => r.Context.Response.Headers["Cache-Control"] = "no-store,no-cache"
            });
            app.UseMvc();
        }
    }

}
