using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Tochka.JsonRpc.Common.Serializers;
using Tochka.JsonRpc.OpenRpc;
using Tochka.JsonRpc.Server;
using Tochka.JsonRpc.Server.Pipeline;

namespace JsonRpcServer
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            // разрешаем сервису-смотрелке читать наш документ
            services.AddCors(options =>
                // разрешать все и всем на самом деле НЕ НАДО, но тут для ознакомительных целей сойдет
                options.AddPolicy(CorsStartupFilter.CorsPolicyName, builder => builder.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));
            // ВАЖНО: зарегать CORS middleware раньше чем open rpc middleware!
            services.AddSingleton<IStartupFilter, CorsStartupFilter>();

            // обязательно включите генерацию xmldoc в настройках проекта!
            services.AddOpenRpc();
            // добавляем документ который знает имя сервиса (если сервис в кубере)
            // services.AddTochkaOpenRpcDocument(Configuration);

            services.AddMvc()
                .AddJsonRpcServer()
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseMiddleware<JsonRpcMiddleware>();
            app.UseMvc();
        }
    }
}