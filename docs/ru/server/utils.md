# Сервер/Utils

Есть разные вспомогательные методы для работы с объектами запросов/ответов JSON-RPC в пайплайне.
На случай, если нужно получить `id` или преобразовать данные.

См. примеры: [Примеры#Доступ к дополнительной информации](examples?id=Доступ-к-дополнительной-информации)

## HttpContext.GetJsonRpcCall()

Метод для получения JSON-RPC request или notification.

Пояснения:

* `ICall` это абстракция для запросов (Request) и уведомлений (Notification).
* `IUntypedCall` это объект с заголовками, но его поле `params` еще не десериализовано и хранится как `JsonDocument`.

## HttpContext.GetRawJsonRpcCall()

Метод для получения "сырого" JSON-RPC вызова в виде `JsonDocument`.

## HttpContext.GetJsonRpcResponse()

Метод для получения JSON-RPC ответа.

Пояснения:

* `IResponse` это абстракция для успешных и ошибочных ответов.
* `IUntypedCall` это объект с заголовками, но его поле `params` еще не десериализовано и хранится как `JsonDocument`.

## HttpContext.JsonRpcRequestIsBatch()

Метод для проверки, является ли этот вызов частью batch-запроса.

## HttpContext.SetJsonRpcResponse(IResponse response)

Метод, чтобы вручную задать ответ. Осторожно: фильтры могут перезаписать его!

## IJsonRpcFeature

Можно вручную достать и изменить всю информацию о JSON-RPC вызове через `IJsonRpcFeature` в `HttpContext.Features`.
