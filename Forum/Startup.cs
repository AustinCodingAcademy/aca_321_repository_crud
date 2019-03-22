using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Forum.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace Forum
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.Map(new PathString("/api"), subApp => subApp.UseMvc());

            var posts = new List<ForumPost>();
            app.Use(async (context, next) =>
            {
                if (context.Request.Path == new PathString("/api/forum"))
                {
                    if (context.Request.Method == "PUT")
                    {
                        using (var reader = new StreamReader(context.Request.Body))
                        {
                            var body = await reader.ReadToEndAsync();
                            var forumPost = JsonConvert.DeserializeObject<ForumPost>(body);
                            forumPost.Id = posts.Count + 1;
                            posts.Add(forumPost);
                            var json = JsonConvert.SerializeObject(forumPost, Formatting.Indented);
                            await context.Response.WriteAsync(json);
                        }
                    }

                    if (context.Request.Method == "GET")
                    {
                        var json = JsonConvert.SerializeObject(posts, Formatting.Indented);
                        await context.Response.WriteAsync(json);
                    }
                }
            });
        }
    }
}
