# Сервер/Маршрутизация

Все методы JSON-RPC должны иметь одинаковый префикс адреса (по умолчанию `/api/jsonrpc`), чтобы их можно было отличить от REST запросов, если оба API используются в одном проекте. Если префикс не указан явно в route метода, то он будет добавлен автоматически. Для методов, у которых адрес не указан вручную, префикс будет использоваться как полный route (без части `/controllerName`).

## Переопределение глобального префикса

См. [Конфигурацию](configuration#RoutePrefix) и [Примеры](examples#Маршрутизация)

```cs
builder.Services.AddJsonRpcServer(static options => options.RoutePrefix = "/api");
```

Префикс `/api` будет добавлен ко всем эндпоинтам приложения, если он уже явно не указан в атрибуте `RouteAttribute`.

| RouteAttribute | Итоговый route для маршрутизации |
| - | - |
| Отсутствует | `/api` |
| `[Route("action")]` или `[Route("/action")]` | `/api/action` |
| `[Route("api/action")]` или `[Route("/api/action")]` | `/api/action` |

## Переопределение route для метода/контроллера

См. [Конфигурацию](configuration#RoutePrefix) и [Примеры](examples#Маршрутизация)

Для конкретных контроллеров или методов можно явно указать route через `RouteAttribute`. Однако, если в нем не содержится глобальный `RoutePrefix`, то данный переопределенный route будет добавлен к префиксу, а не заменит его. Это связано с необходимостью отличать JSON-RPC запросы от REST.

Если `RoutePrefix` из глобальных настроек имеет стандартное значение = `/api/jsonrpc/` (см. [Переопределение глобального префикса](#Переопределение-глобального-префикса)) и определен следующий контроллер:
```cs
[Route("users")]
public class UsersController : JsonRpcControllerBase
{
    [Route("names")]
    public async Task<ActionResult<List<string>>> GetNames()
    {
        // ...
        return this.Ok(new List<string>() { "Alice", "Bob" });
    }
}
```
То в результате маршрутизация к методу `GetNames` будет осуществляться по route = `/api/jsonrpc/users/names`

## Шаблонизация route

См. [Конфигурацию](configuration#RoutePrefix)

Как в `RoutePrefix` из глобальных настроек, так и в `RouteAttribute` поддерживается шаблонизация. По умолчанию поддерживаются [маркеры определенные в ASP.NET](https://learn.microsoft.com/ru-ru/aspnet/core/mvc/controllers/routing?view=aspnetcore-6.0#token-replacement-in-route-templates-controller-action-area): `[area]`, `[controller]` и `[action]`, а так же переменная с версией API `{version:apiVersion}` (по умолчанию `1.0`) из библиотеки [API Versioning](https://github.com/dotnet/aspnet-api-versioning) (см. [Версионирование](versioning))

Если RoutePrefix из глобальных настроек будет определен следующим образом:
```cs
builder.Services.AddJsonRpcServer(static options => options.RoutePrefix = "/api/v{version:apiVersion}/jsonrpc/[controller]");
```
То маршрутизация к методам контроллера с именем `SomethingController` будет осуществляться по route = `/api/v1/jsonrpc/something`

> Для корректной маршрутизации и генерации автодокументации [маркеры ASP.NET](https://learn.microsoft.com/ru-ru/aspnet/core/mvc/controllers/routing?view=aspnetcore-6.0#token-replacement-in-route-templates-controller-action-area) необходимо использовать именно в квадратных скобках (`[ ]`), а все остальные переменные, включая [версию API](https://github.com/dotnet/aspnet-api-versioning) - в фигурных (`{ }`)