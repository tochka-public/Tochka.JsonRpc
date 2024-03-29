# Overview

This is a set of packages to help build JSON-RPC 2.0 APIs like you are used to with ASP.Net Core (MVC/REST).
You only need couple of lines in `Program.cs`, and a different base class for your controllers.

**Note:** in this doc, JSON-RPC protocol is mentioned without version, implying `2.0`.


## Getting started

[Server Quickstart](server/quickstart)

[Client Quickstart](client/quickstart)


## Key features

* Simple installation
* Zero configuration with sane defaults
* Clear options like customizable serialization and routing
* You are still writing API controllers as if there is no JSON-RPC at all
* Mix REST and JSON-RPC because they don't interfere with each other
* Have different urls/controllers/actions with different configuration
* Server uses standard routing, binding and other pipeline built-ins without reinventing or breaking anything
* Client relies on modern HttpClient/HttpClientFactory
* Everything is extensible via DI so you can achieve any specific behavior
* Supports batches, notifications, array params and other JSON-RPC 2.0 quirks while hiding them from user
* Supports returning non-json data if needed, eg. to redirect browser or send binary file
* Batches use the pipeline like separate requests would do, so authentication and other middlewares/filters work independently
* Client is intended to be helpful when diagnosing errors
* Raw and typed JSON-RPC request/response data is accessible to middlewares and controllers
* Lightweight extensibility: common models and utils are not tied to client or server packages
* Autodocumentation support: [Swagger/OpenAPI](https://swagger.io/) and [OpenRPC](https://open-rpc.org/)
* Versioning support: [API Versioning](https://github.com/dotnet/aspnet-api-versioning)


## Limitations and things to improve

Some features are planned, but not implemented yet. The initial goal was just to make things work,
and fine-tune experience after release when we have some real-world usage feedback.

* Currently tested only with ASP.Net Core `6.0`. Version 1.x is obsolete and has low performance, but works for `2.2-6.0`
* Batches are currently handled in sequential fashion
* Float `id` isn't supported


## Nuget Packages

| Link| Description |
| - | - |
| [Tochka.JsonRpc.Server](https://www.nuget.org/packages/Tochka.JsonRpc.Server/) | ASP.Net Core middleware and services |
| [Tochka.JsonRpc.Swagger](https://www.nuget.org/packages/Tochka.JsonRpc.Swagger/) | Swagger document generator |
| [Tochka.JsonRpc.OpenRpc](https://www.nuget.org/packages/Tochka.JsonRpc.OpenRpc/) | OpenRPC document generator |
| [Tochka.JsonRpc.Client](https://www.nuget.org/packages/Tochka.JsonRpc.Client/) | JSON-RPC client base with error handling |
| [Tochka.JsonRpc.Common](https://www.nuget.org/packages/Tochka.JsonRpc.Common/) | Models and utils to allow extensibility without directly depending on server or client |
