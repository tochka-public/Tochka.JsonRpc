# Сервер/Сериализация

В данной библиотеке объекты JSON-RPC запроса/уведомления/ответа называются **"заголовками"**, и они сериализуются отдельно от объектов параметров/результата/ошибки. Вы можете влиять, на то как сериализуются ваши данные, не заботясь о заголовках.

**Замечание:** в данном документе и сериализация и **де**сериализация упоминаются просто как сериализация для простоты.

## Сериализация заголовков

В спецификации JSON-RPC есть некоторые странности, которые не очень хорошо сочетаются с сильной статической типизацией и автоматическими конвертерами вроде  System.Text.Json:

* Поле `id` запроса может содержать число, строку или null и должно быть возвращено в ответе в изначальном виде 
* Если поля `id` нет, то это уведомление
* Поля ответа `result` и `error` являются взаимоисключающими и отображают успех или ошибку
* Поле `params` может содержать или объект или массив
* Весь запрос может быть массивом, означая, что это батч

Адаптация данных правил к типам языка C# требовала некоторых ухищрений и не должна влиять на сериализацию пользовательских данных.
Также, формат заголовков (например snake_case) фиксирован, из-за чего нет нужды конфигурировать его. 

Данные аспекты обрабатываются благодаря `HeadersJsonSerializerOptions` и дополнительным конвертерам. Вам не нужно использовать их в своем коде.

## Сериализация параметров/результата/ошибок

С другой стороны, вам может понадобиться продвинутая сериализация JSON (kebab-case, дополнительные конвертеры, обработка DateTime и тп) для ваших данных.

Используйте глобальную настройку `DefaultDataJsonSerializerOptions`, чтобы установить значение по умолчанию и `JsonRpcSerializerOptionsAttribute` для его переопределения в контроллерах или методах. 

## IJsonSerializerOptionsProvider

Чтобы отделить сериализацию от ASP.Net Core создан интерфейс `IJsonSerializerOptionsProvider`,
который по сути является оберткой вокруг `JsonSerializerOptions`, чтобы использовать его тип в атрибуте.
Мы используем System.Text.Json под капотом, скорее всего вы уже знакомы с концептами данного решения.

В библиотеке есть две реализации данного интерфейса. Вы можете использовать их в своих проектах напрямую или как основание для собственной реализации.

* [`SnakeCaseJsonSerializerOptionsProvider`](https://github.com/tochka-public/Tochka.JsonRpc/blob/master/src/Tochka.JsonRpc.Server/Serialization/SnakeCaseJsonSerializerOptionsProvider.cs): значение по умолчанию для пользовательских данных
* [`CamelCaseJsonSerializerOptionsProvider`](https://github.com/tochka-public/Tochka.JsonRpc/blob/master/src/Tochka.JsonRpc.Server/Serialization/CamelCaseJsonSerializerOptionsProvider.cs): единственное отличие - это обработка имен в camelCase

> При реализации собственного провайдера сделайте свойство `Options` синглтоном, аналогично существующим реализациям

Зарегистрируйте свой провайдер как сервис в DI:

```cs
services.AddSingleton<IJsonSerializerOptionsProvider, YourProvider>();
```

Используйте его через атрибут контроллера или метода:

```cs
[JsonRpcSerializerOptions(typeof(YourProvider))]
public string ToLower(string value) => value.ToLowerInvariant();
```


## Сериализация ранних ошибок в пайплайне

Если обработка запроса вызывает ошибку **до** того, как запрос привязывается к методу, данные о настройках сериализации еще не известны.
В таком случая для сериализации исключений используется `HeadersJsonSerializerOptions`.

## Отображение имени метода к имени контроллера/метода

При отображении поля запроса/уведомления `method`,
библиотека сравнивает значение поля `method` с именем класса и/или метода, сериализованного с соответствующими настройками или явно указанным именем.
Таким образом, вы можете использовать camelCase для поля `method`, или вообще никак не связанное с именем метода, если потребуется.

Для ясности смотрите код с запросами/ответами в [Примеры#Сериализация](examples?id=Сериализация).

Пример процесса связывания:

* в запросе `"method": "user_data.get_name"`
* глобальная настройка `DefaultDataJsonSerializerOptions` имеет значение по умолчанию `JsonRpcSerializerOptions.SnakeCase`
* глобальная настройка `DefaultMethodStyle` имеет значение по умолчанию `JsonRpcMethodStyle.ControllerAndAction`
* имя контроллера и метода: `UserDataController.GetName()`
* `UserDataController`: Имя контроллера - `UserData`, оно сериализуется как `user_data`
* `GetName`: сериализуется как `get_name`
* `"user_data.get_name"` из запроса совпадает с `user_data`.`get_name`, поэтому будет вызван данный метод

Другой пример:

* в запросе `"method": "getName"`
* глобальная настройка `DefaultDataJsonSerializerOptions` имеет значение по умолчанию `JsonRpcSerializerOptions.SnakeCase`
* глобальная настройка `DefaultMethodStyle` имеет значение `JsonRpcMethodStyle.ActionOnly`
* имя контроллера и метода:`UserDataController.GetName()`
* у метода есть атрибут `[JsonRpcMethodStyle(typeof(CamelCaseJsonSerializerOptionsProvider))]`
* `UserDataController`: игнорируется из-за `JsonRpcMethodStyle`
* `GetName`: сериализуется как `getName` за счет особых настроек сериализации
* `"getName"` из запроса совпадает с `getName`, поэтому будет вызван данный метод

И еще один:

* в запросе `"method": "some cUsToM method-name!"`
* глобальная настройка `DefaultDataJsonSerializerOptions` имеет значение по умолчанию `JsonRpcSerializerOptions.SnakeCase`
* глобальная настройка `DefaultMethodStyle` имеет значение по умолчанию `JsonRpcMethodStyle.ControllerAndAction`
* имя контроллера и метода: `UserDataController.GetName()`
* у метода есть атрибут `[JsonRpcMethod("some cUsToM method-name!")]`
* `UserDataController`: игнорируется из-за атрибута `JsonRpcMethod`
* `GetName`: игнорируется из-за атрибута `JsonRpcMethod`
* `"some cUsToM method-name!"` из запроса совпадает с `some cUsToM method-name!` из атрибута, поэтому будет вызван данный метод
