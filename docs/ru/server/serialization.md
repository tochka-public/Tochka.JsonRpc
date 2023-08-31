# Сервер/Сериализация

В этой библиотеке объекты JSON-RPC request/notification/response называются **"заголовками"**, и они сериализуются отдельно от объектов params/result/error. Можно влиять на то, как сериализуются полезные данные, не заботясь о заголовках.

**Примечание:** здесь и сериализация, и **де**сериализация называются просто "сериализацией" для простоты.

## Сериализация заголовков

В спецификации JSON-RPC есть некоторые странности, которые не очень хорошо сочетаются со строгой статической типизацией и автоматическими конвертерами вроде System.Text.Json:

* Поле `id` в Request может содержать число, строку или null и должно быть возвращено в ответе в изначальном виде
* Если поля `id` нет, то это Notification
* Поля Response `result` и `error` являются взаимоисключающими и обозначают успех или ошибку
* Поле `params` может содержать или объект, или массив
* Весь запрос может быть массивом - тогда это Batch

Для адаптации этих правил к типам C# потребовались разные ухищрения, и они не должны повлиять на сериализацию пользовательских данных.
Кроме того, формат заголовков (например snake_case) фиксирован, поэтому не нужно его настраивать.

Все это обрабатывается в `HeadersJsonSerializerOptions` и разных конвертерах. В пользовательском коде они не нужны.

## Сериализация параметров/результата/ошибок

С другой стороны, может понадобиться продвинутая сериализация JSON (kebab-case, дополнительные конвертеры, обработка DateTime и тп) для пользовательских данных.

Для этого есть настройка `DefaultDataJsonSerializerOptions`, чтобы задать значение по умолчанию, и `JsonRpcSerializerOptionsAttribute` для переопределения в контроллерах или методах.

## IJsonSerializerOptionsProvider

Чтобы отделить сериализацию от ASP.Net Core, есть интерфейс `IJsonSerializerOptionsProvider`,
который по сути является оберткой вокруг `JsonSerializerOptions`. Реализации этого интерфейса используются в атрибуте.
Под капотом сериализацией занимается System.Text.Json, подразумевается, что читатель знаком с его концепциями.

В библиотеке есть две реализации интерфейса. Можно использовать их напрямую, или взять за основу собственной реализации.

* [`SnakeCaseJsonSerializerOptionsProvider`](https://github.com/tochka-public/Tochka.JsonRpc/blob/master/src/Tochka.JsonRpc.Server/Serialization/SnakeCaseJsonSerializerOptionsProvider.cs): значение по умолчанию для пользовательских данных
* [`CamelCaseJsonSerializerOptionsProvider`](https://github.com/tochka-public/Tochka.JsonRpc/blob/master/src/Tochka.JsonRpc.Server/Serialization/CamelCaseJsonSerializerOptionsProvider.cs): единственное отличие - это обработка имен в camelCase

> При реализации собственного провайдера нужно сделать свойство `Options` синглтоном, аналогично существующим реализациям

Регистрация своего провайдера в DI:

```cs
services.AddSingleton<IJsonSerializerOptionsProvider, YourProvider>();
```

Использование в атрибуте контроллера или метода:

```cs
[JsonRpcSerializerOptions(typeof(YourProvider))]
public async Task<ActionResult<string>> ToLower(string value)
{
    // ...
    var result = value.ToLowerInvariant();
    return this.Ok(result);
}
```

## Сериализация ранних ошибок в пайплайне

Если обработка запроса падает с ошибкой **до** того, как к запросу был подобран метод, информации об используемом сериализаторе еще нет.
В таком случае для сериализации исключений используется `HeadersJsonSerializerOptions`.

## Сравнение метода из запроса и имени контроллера/метода

Библиотека сравнивает поле `method` из request/notification c именем класса и/или метода, сериализованного с соответствующими настройками, или явно указанным именем.
Таким образом, `method` может быть в `camelCase`, или вообще быть не связанным с именем c# метода, если потребуется.

Чтобы стало понятнее, см. код с запросами/ответами в [Примеры#Сериализация](examples?id=Сериализация).

Пример подбора метода:

* в запросе `"method": "user_data.get_name"`
* глобальная настройка `DefaultDataJsonSerializerOptions` имеет значение по умолчанию `JsonRpcSerializerOptions.SnakeCase`
* глобальная настройка `DefaultMethodStyle` имеет значение по умолчанию `JsonRpcMethodStyle.ControllerAndAction`
* имя контроллера и метода: `UserDataController.GetName()`
* `UserDataController`: Имя контроллера - `UserData`, оно сериализуется как `user_data`
* `GetName`: сериализуется как `get_name`
* `"user_data.get_name"` из запроса совпадает с `user_data`.`get_name`, поэтому будет вызван этот метод

Другой пример:

* в запросе `"method": "getName"`
* глобальная настройка `DefaultDataJsonSerializerOptions` имеет значение по умолчанию `JsonRpcSerializerOptions.SnakeCase`
* глобальная настройка `DefaultMethodStyle` имеет значение `JsonRpcMethodStyle.ActionOnly`
* имя контроллера и метода:`UserDataController.GetName()`
* у метода есть атрибут `[JsonRpcMethodStyle(typeof(CamelCaseJsonSerializerOptionsProvider))]`
* `UserDataController`: игнорируется из-за `JsonRpcMethodStyle`
* `GetName`: сериализуется как `getName` за счет особых настроек сериализации
* `"getName"` из запроса совпадает с `getName`, поэтому будет вызван этот метод

И еще один:

* в запросе `"method": "some cUsToM method-name!"`
* глобальная настройка `DefaultDataJsonSerializerOptions` имеет значение по умолчанию `JsonRpcSerializerOptions.SnakeCase`
* глобальная настройка `DefaultMethodStyle` имеет значение по умолчанию `JsonRpcMethodStyle.ControllerAndAction`
* имя контроллера и метода: `UserDataController.GetName()`
* у метода есть атрибут `[JsonRpcMethod("some cUsToM method-name!")]`
* `UserDataController`: игнорируется из-за атрибута `JsonRpcMethod`
* `GetName`: игнорируется из-за атрибута `JsonRpcMethod`
* `"some cUsToM method-name!"` из запроса совпадает с `some cUsToM method-name!` из атрибута, поэтому будет вызван этот метод
