# Сервер/Привязка моделей

## Поведение по умолчанию

По умолчанию, поле `params` из запроса/уведомления привязывается к аргументам метода:

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

Параметры десериализуются с помощью `DefaultDataJsonSerializerOptions` из глобальных настроек или из `JsonRpcSerializerOptionsAttribute`, если он определен.
Вы можете писать типичные для C# имена аргументов в *camelCase* и их значения будут заданы из поля `params` запроса в *snake_case*.
Если ваши запросы содержат параметры в *camelCase*/*PascalCase* или объекты, которые требуют особой обработки, смотрите страницу [Сериализация](serialization).


## Особые случаи

Вам может потребоваться привязать объект из поля `params` целиком к одной модели, если в запросе много параметров, или просто ради читаемости:

```cs
public record MyData(int Bar, string Baz);

public void Foo(MyData data) {}
```

Однако существует проблема:

> Спецификация JSON-RPC позволяет значениям в поле `params` иметь два формата: объект `{}` или массив `[]`

Привязать JSON объект к объекту .net не составляет труда, однако привязать массив уже невозможно: из массива не понять имен аргументов, а CLR не содержит метаданных о порядке полей в сигнатуре типа.

Другой случай: вам может потребоваться привязать массив из поля `params` целиком к одной коллекции, так как он может содержать различное количество элементов:

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

Случаи описанные выше поддерживаются за счет атрибута, но вам придется положиться на тип поля `params`: всегда JSON массив или объект.
Используйте его только если вы уверены, что клиенты сериализуют запросы именно так, как вы рассчитываете.
Данное поведение не настраивается глобально, так как оно ограничивает поддержку спецификации JSON-RPC.

### Привязка объекта с параметрами к модели

```cs
public void Foo([FromParams(BindingStyle.Object)] MyData data) {}
```

Попытки отправить запрос/уведомление с **массивом** `params` по данному методу будут всегда приводить к ошибкам.

### Привязка массива с параметрами к коллекции

```cs
public void Foo([FromParams(BindingStyle.Array)] List<object> data) {}
```

Попытки отправить запрос/уведомление с **объектом** `params` по данному методу будут всегда приводить к ошибкам.

## Остальные аргументы

Вы можете использовать привязку аргументов из других источников или смешивать ее с `FromParamsAttribute`, однако все возможные комбинации не протестированы. Используйте с осторожностью:

```cs
public void Foo(int bar, string baz, [FromServices] ILogger log) {}
public void Foo(int bar, string baz, CancellationToken token) {}
public void Foo(int bar, string baz, [FromQuery] string id) {}
public void Foo([FromParams(BindingStyle.Object)] MyData data, [FromQuery] string id) {}
```
