> # **THIS IS DOCS FOR VERSIONS BEFORE 2.0**

# Server/Serialization

In this library, JSON Rpc request/notification/response objects are called **"headers"**, and their serialization is
handled separately from params/result/error objects. You can control how your data is serialized without worrying about headers.

**Note:** in this document, both serialization and **de**serialization are referred to as **serialization** for simplicity.

## Headers serialization

JSON Rpc spec has some oddities, which do not fit well into strong static typing and automatic converters like Json.NET:

* Request `id` property can be number, string or null and should be returned in Response exactly the same 
* If `id` is omitted, it is a Notification
* Response `result` and `error` properties are mutually exclusive and indicate success or error
* `params` can be either object or array
* Whole request can be an array - meaning it is a Batch

Fitting this behavior into C# types required some tricks and should not interfere with serialization of regular user data.
Also, format of headers like snake_casing is fixed, so there is no need to configure them.

All these things are handled by `HeaderJsonRpcSerializer` and some converters. You do not need to use them in your code.

## Params/result/error serialization

On the other hand, you may want advanced JSON serialization (kebab-case, extra converters, DateTime handling, etc) for your data.

Use global option `RequestSerializer` for setting default behavior and `JsonRpcSerializerAttribute` for overriding it on controllers or actions.

## IJsonRpcSerializer

To make JsonRpc serialization separate from ASP.Net Core, there is an interface `IJsonRpcSerializer`.
which is basically a wrapper around `JsonSerializerSettings` and `JsonSerializer` singletons.
We use Json.NET under the hood. You should be familiar with its concepts. 

There are 3 serializers in `Tochka.JsonRpc.Common` package. Use them as reference implementation.

* [`SnakeCaseJsonRpcSerializer`](https://github.com/tochka-public/Tochka.JsonRpc/blob/master/src/Tochka.JsonRpc.Common/Serializers/SnakeCaseRpcSerializer.cs): is default for user data
* [`CamelCaseJsonRpcSerializer`](https://github.com/tochka-public/Tochka.JsonRpc/blob/master/src/Tochka.JsonRpc.Common/Serializers/CamelCaseRpcSerializer.cs): only difference is camelCase name handling
* `HeaderJsonRpcSerializer`: as explained above, not needed for user code

> When implementing your own serializer, make both properties singletons as in existing serializers.

Register your serializer as service in DI:

```cs
services.TryAddJsonRpcSerializer<YourSerializer>();
```

> This step is important. If you register manually eg. with `.AddSingleton`, it won't work. Use `.TryAddJsonRpcSerializer`.

Use it via global option or attribute as described above.


## Serializing early pipeline errors

If request pipeline fails **before** it can match request to an action, it does not know yet which serializer is configured for this action.
In this case it uses `HeaderJsonRpcSerializer` to serialize exception.

## Matching method name to controller/action names

When matching request/notification `method` property,
library compares `method` value with class name and/or method name, serialized with corresponding serializer.
This way, you can have your `method` in camelCase, if desired.

For clarity check [Examples#Serialization](examples?id=serialization) and see code with responses/requests.

Matching process example:

* request has `"method": "user_data.get_name"`
* global `RequestSerializer` is default `SnakeCaseJsonRpcSerializer`
* global `MethodStyle` is default `MethodStyle.ControllerAndAction`
* Controller and action are `UserDataController.GetName()`
* `UserDataController`: controller name is `UserData` and serialized as `user_data`
* `GetName`: is serialized as `get_name`
* `"user_data.get_name"` from request is equals to `user_data`.`get_name`, so this action will be invoked

Another example:

* request has `"method": "setValue"`
* global `RequestSerializer` is default `SnakeCaseJsonRpcSerializer`
* global `MethodStyle` is default `MethodStyle.ActionOnly`
* Controller and action are `ServiceDataController.SetValue()`
* Action has `[JsonRpcSerializerAttribute(typeof(CamelCaseJsonRpcSerializer))]`
* `ServiceDataController`: ignored because of `MethodStyle`
* `SetValue`: is serialized as `setValue` with its specific serializer
* `"setValue"` from request is equals to `setValue`, so this action will be invoked
