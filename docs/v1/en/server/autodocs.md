> # **THIS IS DOCS FOR VERSIONS BEFORE 2.0**

# Server/Autodocs

There are two different autodocumentation standards:

* [Swagger/OpenAPI](https://swagger.io/)
  * designed for REST
  * well-known and wiredpread
  * has a lot of tools, libraries, code generators, UIs, etc.
* [OpenRPC](https://open-rpc.org/)
  * designed specifically for JSON Rpc
  * barely known
  * has very few tools

We support both via two different nuget packages. They do similar things: collect information about your API in a JSON document and expose it at some endpoint.

## Swagger/OpenAPI

Swagger support is based on [Swashbuckle.AspNetCore](https://github.com/domaindrivendev/Swashbuckle.AspNetCore) and also includes its UI.

> Please note that Swagger is designed for REST APIs and does not work for JSON Rpc without dirty tricks. Expect some features or advanced scenarios to be broken.

### Usage

Install [Tochka.JsonRpc.Swagger](https://www.nuget.org/packages/Tochka.JsonRpc.Swagger/) and add one line to `Startup.cs`:

```cs
public void ConfigureServices(IServiceCollection services)
{
    services.AddMvc()
        .AddJsonRpcServer()
        .SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
    services.AddSwaggerWithJsonRpc(Assembly.GetExecutingAssembly());  // <-- add this
}
```

> Important note: this extension method makes a lot of assumptions to greatly simplify things for common scenarios. If you need to customize your Swagger documents, Swagger UI, or opt-out of reading XMLdoc, write your own code! Use sources of this method as reference.

You will also need to [enable XMLdoc generation](https://docs.microsoft.com/en-us/dotnet/csharp/codedoc) in your .csproj and you'll want to suppress warning `1591`. Without XML, app will throw exceptions on all requests!

What you will get in your app:

* UI is available at `/swagger` endpoint. Note the drop-down document list on top right.
* Swagger document for regular REST actions is at `/swagger/rest/swagger.json`
* Document for JSON Rpc methods is at `/swagger/jsonrpc/swagger.json`
* If you have more than one `JsonRpcSerializer`, there will be more documents, eg. `/swagger/jsonrpc_camelcase/swagger.json`

### Details

All dirty tricks are explained here.

### Fixing URLs

All JSON Rpc requests go to one endpoint url (eg. `/api/jsonrpc`) and always via POST. For Swagger this is just one "method" with different parameters and return values.
We patch internal metadata about actions, so they appear as different methods in Swagger document, just by appending JSON Rpc `method` after an anchor `#`.  See the example:

<table>
<tr>
    <td>
        JSON Rpc method
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

### Fixing requests and responses

Second thing to patch is metadata about parameters and return values, because we need Swagger to properly generate JSON schemas for every request and response with `id`, `jsonrpc`, `method` properties. Oh, and remember how `params` can be an array or an object? We try our best to fit something into schema. Implementation involves code generation at runtime, so details are not explained here for simplicity.

### Dealing with different JSON serializers

Last trick is to correctly generate JSON schemas according to your serialization rules (see [serialization](serialization)).

Imagine this in your app:

* REST action which returns object of `ResponseData` type, serialized in camelCase
* JSON Rpc action which returns same `ResponseData` type, only now it's serialized in snake_case
* maybe another JSON Rpc action which also returns this type, but it's serialized in PascalCase

If we want all these actions in one Swagger document, we also need different schemas for their responses, because from JSON schema point of view, they are different types. But this is same type in our code!

> JSON schema generaion is based only on C# types. Only one schema is generated for a type.

Unless we separate these three actions onto three different Swagger documents, each with its own JSON schema! That's why we generate different Swagger documents by default: one for REST, and one for each JsonRpcSerializer registered in services.

---

## OpenRPC

OpenRPC is like Swagger but handles JSON Rpc specifics better:

* methods are recognized by actual `method` property, not by URLs
* there is a way to specify if method supports `params` as array, object or both

> Only basic features are currently implemented: info, server, methods and JSON schema. Missing things are: examples, external documentation, links, errors and tags.

### Usage

Install [Tochka.JsonRpc.OpenRpc](https://www.nuget.org/packages/Tochka.JsonRpc.OpenRpc/) and add two lines to `Startup.cs`:

```cs
public void ConfigureServices(IServiceCollection services)
{
    services.AddMvc()
        .AddJsonRpcServer()
        .SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
    services.AddOpenRpc(Assembly.GetExecutingAssembly());  // <-- add this
    services.AddDefaultOpenRpcDocument(Assembly.GetExecutingAssembly());  // <-- add this
}
```

> Important note: this extension method is intended for common scenarios. If you need multiple OpenRPC documents or opt-out of reading XMLdoc, write your own code! Use sources of this method as reference.

You will also need to [enable XMLdoc generation](https://docs.microsoft.com/en-us/dotnet/csharp/codedoc) in your .csproj and you'll want to suppress warning `1591`. Without XML, app will throw exceptions on all requests!

What you will get in your app:

* OpenRPC document at `/openrpc/jsonrpc.json`

### Details

### Where are multiple documents?

Compared to Swagger, here we have better control of JSON schema generation, so there is no need to generate different documents for different serializers.

### Dealing with URLs

If some of your methods are routed to a URL other than default, eg. with `RouteAttribute`, their description will have a `servers` property with this URL, which by specification overrides top-level `servers`.

### Missing features

There was no obvious way to provide information about examples, tags, errors or documentation links from C# code or XMLdoc. These features would require attributes or special "examples providers" types and reflection. This complicates things and was not required for initial release.
