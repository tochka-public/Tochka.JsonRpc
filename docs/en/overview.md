# Overview

This is a set of packages to help make JSON RPC 2.0 APIs like you used to with ASP.Net Core (MVC/REST).
You only need one line in `Startup.cs`, and a different base class for your controllers.

## Key features

* Simple installation
* Zero configuration with sane defaults
* Clear options like customizable serialization and routing
* Server-side uses standard routing, binding and other pipeline built-ins without reinventing or breaking anything
* Tries to replicate ASP.Net Core experience: write controllers and actions like it's normal REST API
* Everything is extensible via DI so you can achieve any specific behavior
* Supports batches, notifications, array params and other JSON RPC 2.0 quirks while hiding them from user
* Supports returning non-json data if required, eg. to redirect browser or send binary file
* Batches which uses the pipeline like separate requests would do
* Client is intended to be helpful when diagnosing errors

## Limitations and things to improve

* Currently tested only with ASP.Net Core `2.2`
* Does not support ASP.Net Core <= `2.1` (requires endpoint routing feature)
* Planned: Not tested with ASP.Net Core `3.x` (but written with concern to be compatible with no or minimal changes)
* Planned: Supports only UTF-8 (because who does JSON serialization in different encodings?)
* Planned: Batches are currently handled in sequential fashion (easy to implement other strategies)
* Planned: Pefrormance is not measured. The initial goal was to make things work, and fine-tune performance after release when we have some real-world usage data

# Server

## Quick start

Install nuget package `Tochka.JsonRpc.Server`.

Register it in `Startup.cs` and set compatibility version:

```cs
    public void ConfigureServices(IServiceCollection services)
        {
		    services.AddMvc()
                .AddJsonRpcServer()  // <-- add this
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_2);  // <-- this is required because 2.1 disables endpoint routing
        }
```

Write your controller as usual, but instead of inheriting from `Controller`, inherit `JsonRpcController`

```cs
    public class EchoController : JsonRpcController
	{
	    public string ToLower(string value)
        {
            return value.ToLower();
        }
	}
```

Start your app and send POST
```http
POST /api/jsonrpc HTTP/1.1
Content-Type: application/json
```
```json
{
    "id": 1,
    "jsonrpc": "2.0",
    "method": "to_lower",
    "params": {
        "value": "TEST"
    }
}
```

Expect response

```HTTP
HTTP/1.1 200 OK
Content-Type: application/json; charset=utf-8
```
```json
{
    "id": 1,
    "jsonrpc": "2.0",
    "result": "test"
}
```