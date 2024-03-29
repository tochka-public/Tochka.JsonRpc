# Сервер/Автодокументация

Есть два стандарта автодокументации:

* [Swagger/OpenAPI](https://swagger.io/)
  * создан для REST
  * широко известен и распространен
  * для него есть много инструментов, библиотек, генераторов кода и тп.
* [OpenRPC](https://open-rpc.org/)
  * создан специально для JSON-RPC
  * малоизвестен
  * практически нет инструментов

Поддерживаются оба стандарта, двумя разными nuget-пакетами. Они выполняют схожие функции: собирают информацию об API в JSON документ и выставляют его на какой-то эндпоинт.

## Swagger/OpenAPI

Поддержка Swagger основана на [Swashbuckle.AspNetCore](https://github.com/domaindrivendev/Swashbuckle.AspNetCore), сразу вместе с UI.

> Swagger создавался для описания REST API, и не работает с JSON-RPC без костылей. Некоторые фичи и сложные сценарии могут не работать.

### Использование

Ставим [Tochka.JsonRpc.Swagger](https://www.nuget.org/packages/Tochka.JsonRpc.Swagger/) и добавляем в `Program.cs`:

```cs
builder.Services.AddControllers();
builder.Services.AddJsonRpcServer();
builder.Services.AddSwaggerWithJsonRpc(Assembly.GetExecutingAssembly()); // <-- добавляем это

var app = builder.Build();

app.UseSwaggerUI(c => c.JsonRpcSwaggerEndpoints(app.Services)); // <-- добавляем это, если нужен UI
app.UseJsonRpc();
app.UseRouting();
app.UseEndpoints(static c =>
{
    c.MapControllers();
    c.MapSwagger(); //  <-- добавляем это. в качестве альтернативы есть UseSwagger()
});
```

> Важное замечание: эти extension-методы делают много предположений, чтобы было легче пользоваться в простых случаях. Если нужно настраивать Swagger документы, Swagger UI или избавиться от чтения XML документации, придется написать свою реализацию! Можно оттолкнуться от исходников этих extension-ов.

Также потребуется [включить генерацию XML документации](https://docs.microsoft.com/en-us/dotnet/csharp/codedoc) в своем .csproj, и скорее всего понадобится отключить warning `1591`. Без XML документации приложение будет выкидывать исключения на все запросы!

Что получим в приложении:

* UI по адресу `/swagger`. Нужно обратить внимание на выпадающий список сверху справа.
* Еще можно добавить Swagger документ для обычных REST контроллеров, в `UseSwaggerUI()`:
```cs
app.UseSwaggerUI(c =>
{
    c.JsonRpcSwaggerEndpoints(app.Services); // JSON-RPC
    c.SwaggerEndpoint("/swagger/rest/swagger.json", "RESTful"); // REST
});
```
* Документ для методов JSON-RPC по адресу `/swagger/jsonrpc_v1/swagger.json`
* Если в DI зарегистрировано несколько реализаций `IJsonSerializerOptionsProvider`, то получим несколько документов, например, `/swagger/jsonrpc_camelcase_v1/swagger.json` (нейминг основан на имени типа провайдера, см. [`GetDocumentName`](https://github.com/tochka-public/Tochka.JsonRpc/blob/master/src/Tochka.JsonRpc.ApiExplorer/Utils.cs))
* Если в проекте используется несколько версий API (см. [Версионирование](versioning)), то получим несколько документов в соответствии с версиями, например `/swagger/jsonrpc_v2/swagger.json` и `/swagger/jsonrpc_camelcase_v2/swagger.json`

### Детали

Здесь описаны все костыли.

#### Исправление URL

Обычно все JSON-RPC запросы принято отправлять на один url (например `/api/jsonrpc`), и всегда через POST. Для Swagger это все один "метод" с различными параметрами и возвращаемыми значениями.
Мы меняем внутренние метаданные о методах, чтобы они отображались как разные методы в Swagger, за счет добавления имени метода после url-якоря `#`.
Поэтому Swagger считает их за разные url-ы, и отправка запроса через Swagger UI не ломается. Комбинация url + метод должна быть уникальной.
Пример:

| JSON-RPC метод | Отображение в Swagger |
| - | - |
| echo.to_lower | POST /api/jsonrpc#echo.to_lower |

#### Исправление запросов и ответов

Вторая вещь, которую нужно патчить - метаданные параметров и возвращаемых значений, так как нам нужно, чтобы Swagger правильно сгенерировал JSON схемы для каждого запроса и ответа с полями `id`, `jsonrpc`, `method`. О, еще вспоминаем, что поле `params` может быть как объектом, так и массивом. Делаем все возможное, чтобы запихать что-нибудь в схему. Реализация содержит генерацию кода в рантайме, поэтому не описана здесь для простоты.

#### Разные настройки сериализации JSON

Последний хак - правильная генерация JSON схем в соответствии с настройками сериализации (см. [Сериализация](serialization)).

Например, в приложении есть:

* REST метод, возвращающий объект типа `ResponseData` в camelCase
* JSON-RPC метод, который возвращает тот же самый объект типа `ResponseData`, но уже snake_case
* возможно еще один JSON-RPC метод, тоже возвращающий этот тип, но в PascalCase

Чтобы все эти методы были в одном Swagger-документе, то понадобятся различные схемы для их response, потому что с точки зрения JSON схемы, это разные типы. Но это один и тот же тип в коде!

> JSON схемы генерируются из C# типов. Для каждого типа - только одна схема.

Из-за этого приходится разделять эти три обработчика по разным Swagger документам, каждый со своей JSON схемой! Именно поэтому генерируются разные Swagger документы: один для каждого зарегистрированного в DI `IJsonSerializerOptionsProvider`. Если в приложении используется REST, то нужен отдельный документ и для него.

---

## OpenRPC

OpenRPC похож на Swagger, но лучше поддерживает специфику JSON-RPC:

* методы различаются по полю `method`, а не по url
* можно указать, принимает ли метод `params` в виде массива, объекта, или оба варианта

> Реализованы только базовые фичи: общая информация, сервер, методы и JSON схемы. Не сделаны: примеры, внешняя документация, ссылки, ошибки и теги.

### Использование

Ставим [Tochka.JsonRpc.OpenRpc](https://www.nuget.org/packages/Tochka.JsonRpc.OpenRpc/) и добавляем в `Program.cs`:

```cs
builder.Services.AddControllers();
builder.Services.AddJsonRpcServer();
builder.Services.AddOpenRpc(Assembly.GetExecutingAssembly()); // <-- добавляем это

var app = builder.Build();

app.UseJsonRpc();
app.UseRouting();
app.UseEndpoints(static c =>
{
    c.MapControllers();
    c.MapOpenRpc(); //  <-- добавляем это. в качестве альтернативы есть UseOpenRpc()
});
```

> Важное замечание: эти extension-методы делают много предположений, чтобы было легче пользоваться в простых случаях. Если нужно настраивать несколько OpenRPC документов или избавиться от чтения XML документации, придется написать свою реализацию! Можно оттолкнуться от исходников этих extension-ов.

Также потребуется [включить генерацию XML документации](https://docs.microsoft.com/en-us/dotnet/csharp/codedoc) в своем .csproj, и скорее всего понадобится отключить warning `1591`. Без XML документации приложение будет выкидывать исключения на все запросы!

Что получим в приложении:

* OpenRPC документ по адресу `/openrpc/jsonrpc_v1.json` (из него можно сгенерировать UI в [OpenRPC Playground](https://playground.open-rpc.org/))
* Если в проекте используется несколько версий API (см. [Версионирование](versioning)), то получим несколько документов в соответствии с версиями, например `/openrpc/jsonrpc_v2.json`

### Детали

#### Где разные документы?

Документы разбиваются только по версиям API. По сравнению со Swagger, здесь есть больший контроль над генерацией JSON схем. Поэтому нет необходимости разбивать все на несколько документов, даже если используются разные сериализаторы.

#### Разные URL

Если какие-то методы маршрутизируются на отдельные url, например, с помощью `RouteAttribute` или из-за шаблонизации, их описания будут включать поле `servers` с нужным url. По спецификации OpenRPC, это поле переопределяет значения `servers`, которые могли быть выше.

#### Недоделанные фичи

Не нашлось очевидных способов собирать примеры, теги, ошибки или ссылки на документацию из C# кода или XMLdoc. Для этого понадобились бы атрибуты или специальные "провайдеров примеров", и рефлексия. Это бы сильно усложнило логику и не нужно для первого релиза.
