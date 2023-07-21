# Сервер/Binding

## Поведение по умолчанию

По умолчанию, поле `params` из request/notification биндится в аргументы метода:

```json
{
    "id": 1,
    "method": "foo",
    "params": {
        "bar": 1,
        "baz": "test"
    },
    "jsonrpc": "2.0"
}
```

```cs
public void Foo(int bar, string baz) {}
```

### Имена аргументов

Params десериализуется с помощью `DefaultDataJsonSerializerOptions` из глобальных настроек или из `JsonRpcSerializerOptionsAttribute`, если он есть.
Можно писать обычные для C# имена аргументов в *camelCase*, их значения будут заполнены из *snake_case* `params` в запросе.
Если запросы содержат параметры в *camelCase*/*PascalCase*, или объекты, которые требуют особой обработки, см. [Сериализация](serialization).

## Продвинутые сценарии

Может потребоваться забиндить объект из поля `params` целиком в одну модель, если в запросе много параметров, или просто ради читаемости:

```cs
public record MyData(int Bar, string Baz);

public void Foo(MyData data) {}
```

Но есть проблема:

> Спецификация JSON-RPC позволяет присылать `params` двух форматов: объект `{}` или массив `[]`

Забиндить JSON-объект в объект .net легко, а массив - невозможно: из массива мы не знаем имен аргументов, а CLR не содержит метаданных о порядке определения полей в типе.

Другой сценарий: может потребоваться забиндить массив из поля `params` целиком к одной коллекции, так как он может содержать различное количество элементов:

```json
{
    "id": 1,
    "method": "foo",
    "params": [
        "a",
        "b",
        "c"
    ],
    "jsonrpc": "2.0"
}
```

```cs
public void Foo(List<object> data) {}
public void Foo(List<string> data) {}
public void Foo(params string[] data) {}
```

## FromParamsAttribute

Случаи описанные выше поддерживаются за счет атрибута, но придется положиться на тип поля `params`: всегда JSON массив, или объект.
Можно пользоваться, только если есть уверенность, что клиенты сериализуют запросы именно так, как ожидается.
Это поведение не настраивается глобально, так как оно ограничивает поддержку спецификации JSON-RPC.

### Биндинг params-объекта в модель

```cs
public void Foo([FromParams(BindingStyle.Object)] MyData data) {}
```

Попытки отправить request/notification с **массивом** `params` в такой метод всегда будут падать с ошибкой.

### Биндинг params-массива в коллекцию

```cs
public void Foo([FromParams(BindingStyle.Array)] List<object> data) {}
```

Попытки отправить request/notification с **объектом** `params` в такой метод всегда будут падать с ошибкой.

## Прочие аргументы

Можно биндить аргументы из других источников или смешивать с `FromParamsAttribute`. Это будет работать, но не все возможные комбинации протестированы. Использовать с осторожностью:

```cs
public void Foo(int bar, string baz, [FromServices] ILogger log) {}
public void Foo(int bar, string baz, CancellationToken token) {}
public void Foo(int bar, string baz, [FromQuery] string id) {}
public void Foo([FromParams(BindingStyle.Object)] MyData data, [FromQuery] string id) {}
```
