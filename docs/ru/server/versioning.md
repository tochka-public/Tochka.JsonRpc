# Сервер/Версионирование

По умолчанию подключено версионирование (библиотека [API Versioning](https://github.com/dotnet/aspnet-api-versioning)) со стандартной версией API = `1.0`.
Однако, если явно не использовать атрибуты `[ApiVersion]` и не использовать версию API в route (см. [Маршрутизация#Шаблонизация route](routing#шаблонизация-route)), то все будет работать и без указания версий, как для JSON-RPC контроллеров, так и для REST.

## Пример использования версий API

> `Program.cs`
```cs
builder.Services.AddJsonRpcServer(static options => options.RoutePrefix = "/api/v{version:apiVersion}/jsonrpc");
```
> `Controllers.cs`
```cs
[ApiVersion(1)]
[ApiVersion(2)]
public class VersionedController : JsonRpcControllerBase
{
    public async Task<IActionResult> Do1()
    {
        // ...
    };
}

public class UnversionedController : JsonRpcControllerBase
{
    public async Task<IActionResult> Do2()
    {
        // ...
    };
}
```
Метод `VersionedController.Do1` будет доступен по route:
 - `/api/v1/jsonrpc`
 - `/api/v2/jsonrpc`

А метод `UnversionedController.Do2` только по `/api/v1/jsonrpc`

## Автодокументация

См. [Автодокументация](autodocs)

Если используется несколько версий API, то документы OpenAPI (включая отдельные документы для зарегистрированных реализаций `IJsonSerializerOptionsProvider`) и OpenRPC будут разделены на несколько, в соответствии с версиями.
В интерфейсе Swagger будут добавлены все варианты.

Если в проекте используются `[ApiVersion(1)]`, `[ApiVersion(2)]` и `[ApiVersion("3-str")]`, то будет сформирован следующий список документов:
 - OpenAPI (Swagger):
   - `/swagger/jsonrpc_v1/swagger.json`
   - `/swagger/jsonrpc_v2/swagger.json`
   - `/swagger/jsonrpc_v3-str/swagger.json`
 - OpenRPC:
   - `/openrpc/jsonrpc_v1.json`
   - `/openrpc/jsonrpc_v2.json`
   - `/openrpc/jsonrpc_v3-str.json`