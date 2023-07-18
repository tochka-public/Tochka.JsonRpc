# Server/Serialization

In this library, JSON-RPC request/notification/response objects are called **"headers"**, and their serialization is
handled separately from params/result/error objects. You can control how your data is serialized without worrying about headers.

**Note:** in this document, both serialization and **de**serialization are referred to as **serialization** for simplicity.

## Headers serialization

JSON-RPC spec has some oddities, which do not fit well into strong static typing and automatic converters like System.Text.Json:

* Request `id` property can be number, string or null and should be returned in Response exactly the same 
* If `id` is omitted, it is a Notification
* Response `result` and `error` properties are mutually exclusive and indicate success or error
* `params` can be either object or array
* Whole request can be an array - meaning it is a Batch

Fitting this behavior into C# types required some tricks and should not interfere with serialization of regular user data.
Also, format of headers like snake_casing is fixed, so there is no need to configure them.

All these things are handled by `HeadersJsonSerializerOptions` and some converters. You do not need to use them in your code.

## Params/result/error serialization

On the other hand, you may want advanced JSON serialization (kebab-case, extra converters, DateTime handling, etc) for your data.

Use global option `DefaultDataJsonSerializerOptions` for setting default behavior and `JsonRpcSerializerOptionsAttribute` for overriding it on controllers or actions.

## IJsonSerializerOptionsProvider

To make JsonRpc serialization separate from ASP.Net Core, there is an interface `IJsonSerializerOptionsProvider`,
which is basically a wrapper around `JsonSerializerOptions` to use it by type in attribute.
We use System.Text.Json under the hood, you should be familiar with its concepts. 

There are 2 implementations in package. Use them in you projects or as reference implementation.

* [`SnakeCaseJsonSerializerOptionsProvider`](https://github.com/tochka-public/Tochka.JsonRpc/blob/master/src/Tochka.JsonRpc.Server/Serialization/SnakeCaseJsonSerializerOptionsProvider.cs): is default for user data
* [`CamelCaseJsonSerializerOptionsProvider`](https://github.com/tochka-public/Tochka.JsonRpc/blob/master/src/Tochka.JsonRpc.Server/Serialization/CamelCaseJsonSerializerOptionsProvider.cs): only difference is camelCase name handling

> When implementing your own provider, make `Options` property singleton as in existing providers

Register your provider as service in DI:

```cs
services.AddSingleton<IJsonSerializerOptionsProvider, YourProvider>();
```

Use it via attribute for controller or action:

```cs
[JsonRpcSerializerOptions(typeof(YourProvider))]
public string ToLower(string value) => value.ToLowerInvariant();
```


## Serializing early pipeline errors

If request pipeline fails **before** it can match request to an action, it does not know yet which serializer options is configured for this action.
In this case it uses `HeadersJsonSerializerOptions` to serialize exception.

## Matching method name to controller/action names

When matching request/notification `method` property,
library compares `method` value with class name and/or method name, serialized with corresponding serializer or explicitly defined name.
This way, you can have your `method` in camelCase, or even use completely unrelated name, if desired.

For clarity check [Examples#Serialization](examples?id=serialization) and see code with requests/responses.

Matching process example:

* request has `"method": "user_data.get_name"`
* global `DefaultDataJsonSerializerOptions` is default `JsonRpcSerializerOptions.SnakeCase`
* global `DefaultMethodStyle` is default `JsonRpcMethodStyle.ControllerAndAction`
* Controller and action are `UserDataController.GetName()`
* `UserDataController`: controller name is `UserData` and serialized as `user_data`
* `GetName`: is serialized as `get_name`
* `"user_data.get_name"` from request is equals to `user_data`.`get_name`, so this action will be invoked

Another example:

* request has `"method": "getName"`
* global `DefaultDataJsonSerializerOptions` is default `JsonRpcSerializerOptions.SnakeCase`
* global `DefaultMethodStyle` is `JsonRpcMethodStyle.ActionOnly`
* Controller and action are `UserDataController.GetName()`
* Action has `[JsonRpcMethodStyle(typeof(CamelCaseJsonSerializerOptionsProvider))]` attribute
* `UserDataController`: ignored because of `JsonRpcMethodStyle`
* `GetName`: is serialized as `getName` with its specific serializer options
* `"getName"` from request is equals to `getName`, so this action will be invoked

And another one:

* request has `"method": "some cUsToM method-name!"`
* global `DefaultDataJsonSerializerOptions` is default `JsonRpcSerializerOptions.SnakeCase`
* global `DefaultMethodStyle` is default `JsonRpcMethodStyle.ControllerAndAction`
* Controller and action are `UserDataController.GetName()`
* Action has `[JsonRpcMethod("some cUsToM method-name!")]` attribute
* `UserDataController`: ignored because of `JsonRpcMethod` attribute
* `GetName`: ignored because of `JsonRpcMethod` attribute
* `"some cUsToM method-name!"` from request is equals to `some cUsToM method-name!` from attribute, so this action will be invoked
