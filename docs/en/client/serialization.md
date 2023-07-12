# Client/Serialization

In this library, JSON Rpc request/notification/response objects are called **"headers"**, and their serialization is
handled separately from params/result/error objects. You can control how your data is serialized without worrying about headers.

**Note:** in this document, both serialization and **de**serialization are referred to as **serialization** for simplicity.

## Headers serialization

JSON Rpc spec has some oddities, which do not fit well into strong static typing and automatic converters like System.Text.Json:

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

Use property `DataJsonSerializerOptions` for setting serialization behavior.

You can use one of predefined values from `JsonRpcSerializerOptions` class or create your own `JsonSerializerOptions`.