# Server/Autodocs

There are two different autodocumentation standards:

* [Swagger/OpenAPI](https://swagger.io/)
  * designed for REST
  * well-known and widespread
  * has a lot of tools, libraries, code generators, UIs, etc.
* [OpenRPC](https://open-rpc.org/)
  * designed specifically for JSON-RPC
  * barely known
  * has very few tools

We support both via two different nuget packages. They do similar things: collect information about your API in a JSON document and expose it at some endpoint.

## Swagger/OpenAPI

Swagger support is based on [Swashbuckle.AspNetCore](https://github.com/domaindrivendev/Swashbuckle.AspNetCore) and also includes its UI.

> Please note that Swagger is designed for REST APIs and does not work for JSON-RPC without dirty tricks. Expect some features or advanced scenarios to be broken.

### Usage

Install [Tochka.JsonRpc.Swagger](https://www.nuget.org/packages/Tochka.JsonRpc.Swagger/) and add couple of lines to `Program.cs`:

```cs
builder.Services.AddControllers();
builder.Services.AddJsonRpcServer();
builder.Services.AddSwaggerWithJsonRpc(Assembly.GetExecutingAssembly()); // <-- add this

var app = builder.Build();

app.UseSwaggerUI(c => c.JsonRpcSwaggerEndpoints(app.Services)); // <-- add this if you also need UI
app.UseJsonRpc();
app.UseRouting();
app.UseEndpoints(static c =>
{
    c.MapControllers();
    c.MapSwagger(); //  <-- add this, alternative - UseSwagger()
});
```

> Important note: these extension methods make a lot of assumptions to greatly simplify things for common scenarios. If you need to customize your Swagger documents, Swagger UI, or opt-out of reading XMLdoc, write your own code! Use sources of these methods as reference.

You will also need to [enable XMLdoc generation](https://docs.microsoft.com/en-us/dotnet/csharp/codedoc) in your .csproj and you'll want to suppress warning `1591`. Without XML, app will throw exceptions on all requests!

What you will get in your app:

* UI is available at `/swagger` endpoint. Note the drop-down document list on top right.
* Swagger document for regular REST actions can be also added in config for `UseSwaggerUI()`:
```cs
app.UseSwaggerUI(c =>
{
    c.JsonRpcSwaggerEndpoints(app.Services); // JSON-RPC
    c.SwaggerEndpoint("/swagger/rest/swagger.json", "RESTful"); // REST
});
```
* Document for JSON-RPC methods is at `/swagger/jsonrpc_v1/swagger.json`
* If you have multiple `IJsonSerializerOptionsProvider` implementations registered in DI, there will be more documents, eg. `/swagger/jsonrpc_camelcase_v1/swagger.json` (naming based on provider's class name, see [`GetDocumentName`](https://github.com/tochka-public/Tochka.JsonRpc/blob/master/src/Tochka.JsonRpc.ApiExplorer/Utils.cs) for info)
* If you use multiple API versions in project (see [Versioning](versioning)), there will be more documents, eg. `/swagger/jsonrpc_v2/swagger.json` and `/swagger/jsonrpc_camelcase_v2/swagger.json`

### Details

All dirty tricks are explained here.

#### Fixing URLs

All JSON-RPC requests usually go to one endpoint url (eg. `/api/jsonrpc`) and always via POST. For Swagger this is just one "method" with different parameters and return values.
We patch internal metadata about actions, so they appear as different methods in Swagger document, just by appending JSON-RPC `method` after an anchor `#`.
This way Swagger treats them as different urls, but sending request via Swagger UI still works. Combination of url + method must be unique.
See the example:

<table>
<tr>
    <td>
        JSON-RPC method
    </td>
    <td>
        Swagger representation
    </td>
</tr>
<tr>
    <td>
        echo.to_lower
    </td>
    <td>
        POST /api/jsonrpc#echo.to_lower
    </td>
</tr>
</table>

#### Fixing requests and responses

Second thing to patch is metadata about parameters and return values, because we need Swagger to properly generate JSON schemas for every request and response with `id`, `jsonrpc`, `method` properties. Oh, and remember how `params` can be an array or an object? We try our best to fit something into schema. Implementation involves code generation at runtime, so details are not explained here for simplicity.

#### Dealing with different JSON serializers

Last trick is to correctly generate JSON schemas according to your serialization rules (see [Serialization](serialization)).

Imagine this in your app:

* REST action which returns object of `ResponseData` type, serialized in camelCase
* JSON-RPC action which returns same `ResponseData` type, only now it's serialized in snake_case
* maybe another JSON-RPC action which also returns this type, but it's serialized in PascalCase

If we want all these actions in one Swagger document, we also need different schemas for their responses, because from JSON schema point of view, they are different types. But this is same type in our code!

> JSON schema generation is based only on C# types. Only one schema is generated for a type.

Unless we separate these three actions onto three different Swagger documents, each with its own JSON schema! That's why we generate different Swagger documents by default: one for each `IJsonSerializerOptionsProvider` registered in services. If you have REST, you also need to add separate document for it.

---

## OpenRPC

OpenRPC is like Swagger but handles JSON-RPC specifics better:

* methods are recognized by actual `method` property, not by URLs
* there is a way to specify if method supports `params` as array, object or both

> Only basic features are currently implemented: info, server, methods and JSON schema. Missing things are: examples, external documentation, links, errors and tags.

### Usage

Install [Tochka.JsonRpc.OpenRpc](https://www.nuget.org/packages/Tochka.JsonRpc.OpenRpc/) and add couple of lines to `Program.cs`:

```cs
builder.Services.AddControllers();
builder.Services.AddJsonRpcServer();
builder.Services.AddOpenRpc(Assembly.GetExecutingAssembly()); // <-- add this

var app = builder.Build();

app.UseJsonRpc();
app.UseRouting();
app.UseEndpoints(static c =>
{
    c.MapControllers();
    c.MapOpenRpc(); //  <-- add this, alternative - UseOpenRpc()
});
```

> Important note: these extension methods is intended for common scenarios. If you need multiple OpenRPC documents or opt-out of reading XMLdoc, write your own code! Use sources of these methods as reference.

You will also need to [enable XMLdoc generation](https://docs.microsoft.com/en-us/dotnet/csharp/codedoc) in your .csproj and you'll want to suppress warning `1591`. Without XML, app will throw exceptions on all requests!

What you will get in your app:

* OpenRPC document at `/openrpc/jsonrpc_v1.json` (you can use it to build UI on [OpenRPC Playground](https://playground.open-rpc.org/))
* If you use multiple API versions in project (see [Versioning](versioning)), there will be more documents according to versions, eg. `/openrpc/jsonrpc_v2.json`

### Details

#### Where are multiple documents?

Documents are divided only by API versions. Compared to Swagger, here we have better control of JSON schema generation, so there is no need to generate different documents for different serializers.

#### Dealing with URLs

If some of your methods are routed to a URL other than default, eg. with `RouteAttribute` or because of using templates, their description will have a `servers` property with this URL, which by specification overrides top-level `servers`.

#### Missing features

There was no obvious way to provide information about examples, tags, errors or documentation links from C# code or XMLdoc. These features would require attributes or special "examples providers" types and reflection. This complicates things and was not required for initial release.
