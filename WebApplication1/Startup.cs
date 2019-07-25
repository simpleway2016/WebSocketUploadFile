using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace WebApplication1
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.Add(new ServiceDescriptor(typeof(WebSocketUploadFile.IUploadFileHandler) , new TestUploadHandler()));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            WebSocketUploadFile.Factory.Enable(app, env);
            app.UseStaticFiles();

            app.Run(async (context) =>
            {
                await context.Response.WriteAsync("Hello World!");
            });
        }
    }

    class TestUploadHandler : WebSocketUploadFile.IUploadFileHandler
    {
        Dictionary<int, System.IO.FileStream> _dict = new Dictionary<int, System.IO.FileStream>();

        public void OnBeginUploadFile(int tranid, string filename, int length, bool isContinue)
        {
           if(_dict.ContainsKey(tranid) == false)
            {
                _dict[tranid] = System.IO.File.Create($"{AppDomain.CurrentDomain.BaseDirectory}{filename}");
            }
        }

        public void OnError(int tranid, string filename)
        {

        }

        public void OnReceivedFileContent(int tranid, string filename, byte[] data, int length, int filePosition)
        {
            var stream = _dict[tranid];
            stream.Seek(filePosition, System.IO.SeekOrigin.Begin);
            stream.Write(data, 0, length);
        }

        public void OnUploadCompleted(int tranid, string filename)
        {
            var stream = _dict[tranid];
            stream.Close();
            _dict.Remove(tranid);
        }
    }
}
