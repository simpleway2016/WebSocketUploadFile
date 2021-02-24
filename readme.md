大文件上传处理中间件，支持断点续传

``` 
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
        public void OnFileComing(UploadHeader header)
        {
            //如果要阻止上传，这里可以抛出异常   
        }
        public void OnUploadCompleted(UploadHeader header)
        {
            //这里面需要把header.FilePath指向的文件拷走，因为会自动删除此文件
        }
    }

```

**Html页面的使用**

```
<body>
    <input id="file" type="file" />
    <button onclick="obj.upload()">
        upload
    </button>
    <div id="info"></div>
</body>
<script lang="ja">
    var info = document.body.querySelector("#info");

    //引用nodejs模块
    var WebSocketUploadFile = require("jack-websocket-uploadfile");

    var obj = new WebSocketUploadFile(document.body.querySelector("#file"));
    obj.onProgress = function (sender, total, sended) {
        info.innerHTML = sended + "," + total;
    }
    obj.onCompleted = function (sender) {
        info.innerHTML = "ok";
    }
    obj.onError = function (sender, err) {
        info.innerHTML = JSON.stringify( err );
        //如果断点续传，这里直接调用obj.upload()即可
    }
</script>
```

***TypeScript in webpack***
tsconfig.json
```
{
  "compilerOptions": {
    "outDir": "./dist/",
    "sourceMap": true,
    "noImplicitAny": false,
    "module": "es2015",
    "moduleResolution": "node",
    "target": "es5",
    "allowJs": true,
    "types": [
      "./node_modules/jack-websocket-uploadfile",
    ]
  }
}

```
**import**
```
import WebSocketUploadFile from "jack-websocket-uploadfile"

```
