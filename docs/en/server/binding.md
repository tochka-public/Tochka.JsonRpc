# Server/Binding

## Default behavior

By default, `params` from request/notification are bound to action arguments:

```json
{
    "id": 1,
    "jsonrpc": "2.0",
    "method": "foo",
    "params": {
        "bar": 1,
        "baz": "test"
    }
}
```

```cs
public void Foo(int bar, string baz){}
```

### Argument names

Params are deserialized using `RequestSerializer` from global options or from `JsonRpcSerializerAttribute` if specified.
You can write regular C# *camelCase* argument names and they will be populated from *snake_case* request `params`.
If your request params are *camelCase*/*PascalCase* or have objects which require special handling, see [Serialization](serialization).


## Advanced scenarios

You may want to bind whole `params` into one model, if you have a lot of properties or just for clarity:

```cs
public class MyData{
    public int Bar {get; set;}
    public string Baz {get; set;}
}

public void Foo(MyData data){}
```

But there's a catch:

> JSON Rpc spec allows `params` in two forms: object `{}` or array `[]`.

Binding JSON object to .net object is easy, but it's impossible to bind an array: we don't know property names,
and CLR has no metadata about property order in type definition.

Another scenario: you may want to bind JSON array from `params` to a collection because it has variable items count:

```json
{
    "id": 1,
    "jsonrpc": "2.0",
    "method": "foo",
    "params": [
        "a",
        "b",
        "c"
    ]
}
```

```cs
public void Foo(List<object> data){}
public void Foo(List<string> data){}
public void Foo(params string[] data){}
```

## FromParamsAttribute

Scenarios mentioned above are covered with an attribute, but you will have to rely on `params` type.
Use it only if you are sure that your clients serialize requests as you expect.
This behavior is not globally configurable because it limits JSON Rpc spec support.

### Bind params object to model

```cs
public void Foo([FromParams(BindingStyle.Object)]MyData data){}
```

Attempt to send request/notification to this method with `params` **array** will result in error.

### Bind params array to collection

```cs
public void Foo([FromParams(BindingStyle.Array)]List<object> data){}
```

Attempt to send request/notification to this method with `params` **object** will result in error.

## Other arguments

If you want to bind arguments from other sources or mix with `FromParamsAttribute`, it will work, but all possible combinations are not tested. Use with care:

```cs
public void Foo(int bar, string baz, [FromServices]ILogger log){}
public void Foo(int bar, string baz, CancellationToken token){}
public void Foo(int bar, string baz, [FromQuery] string id){}
public void Foo([FromParams(BindingStyle.Object)]MyData data, [FromQuery] string id){}
```
