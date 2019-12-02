using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Xamarin.AspNetCore.Auth;

namespace Sample3
{
	public class Startup
	{
		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
		}

		public IConfiguration Configuration { get; }

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{
			services.AddControllers();

			services.AddXamarinAuth(o =>
			{
				o.CallbackUri = new Uri("myapp://");
			});

			services.AddAuthentication()
				.AddCookie()
				.AddFacebook(fb =>
				{
					fb.AppId = "xxxxxxxxxxxxxxxxxxxxxxxxxx";
					fb.AppSecret = "yyyyyyyyyyyyyyyyyy";
					fb.SaveTokens = true;
				})
				.AddAppleSignIn(new AppleSignInOptions
				{
					ServerId = "com.yourserverid",
					TeamId = "your-team-id",
					KeyId = "your-key-id",
					P8Key = "your-p8-contents-without-newlines-or-comments"
				});
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			if (env.IsDevelopment())
				app.UseDeveloperExceptionPage();
			
			app.UseHttpsRedirection();

			app.UseXamarinAuth();

			app.UseRouting();

			app.UseAuthentication();
			app.UseAuthorization();

			app.UseEndpoints(endpoints =>
			{
				endpoints.MapControllers();
			});
		}
	}
}
