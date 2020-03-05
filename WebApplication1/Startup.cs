using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using WebSocketUploadFile;
namespace WebApplication1
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IUploadCompleted>(new TestUploadComleted());
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

    class TestUploadComleted : IUploadCompleted
    {
        public void OnUploadCompleted(UploadHeader header)
        {
            
        }
    }

    class TestUploadHandler : WebSocketUploadFile.IUploadFileHandler
    {
        Dictionary<int, System.IO.FileStream> _dict = new Dictionary<int, System.IO.FileStream>();

        public void OnBeginUploadFile(WebSocketUploadFile.UploadHeader header, bool isContinue)
        {
           if(_dict.ContainsKey(header.TranId.Value) == false)
            {
                _dict[header.TranId.Value] = System.IO.File.Create($"{AppDomain.CurrentDomain.BaseDirectory}{header.FileName}");
            }
        }

        public void OnError(WebSocketUploadFile.UploadHeader header)
        {

        }

        public void OnReceivedFileContent(WebSocketUploadFile.UploadHeader header, byte[] data, int length, long filePosition)
        {
            var stream = _dict[header.TranId.Value];
            stream.Seek(filePosition, System.IO.SeekOrigin.Begin);
            stream.Write(data, 0, length);
        }

        public void OnUploadCompleted(WebSocketUploadFile.UploadHeader header)
        {
            var stream = _dict[header.TranId.Value];
            stream.Close();
            _dict.Remove(header.TranId.Value);
        }
    }
}
