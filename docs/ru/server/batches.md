# Сервер/Batches

Batch-запросы обрабатываются в `JsonRpcMiddleware`: для каждого элемента, в `HttpContext` подставляются endpoint и данные о единичном запросе. `HttpContext` не потокобезопасен, поэтому пока что поддерживается только последовательная обработка. Единственной альтернативой было бы копирование объекта `HttpContext` перед обработкой каждого элемента, но это приводит к дубликации данных при схлопывании копий в один ответ, и сложно в реализации. Поэтому параллельная обработка пока что не реализована.

Для определения, является ли текущий вызов частью batch, можно использовать метод расширения для `HttpContext.JsonRpcRequestIsBatch()`.
Пригодится, если нужно проставлять какой-то заголовок только один раз или реализовать особую логику в мидлварях или фильтрах.

Если включено `AllowRawResponses` и один из вызовов вернет не-JSON данные,
batch-ответ будет содержать их среди остальных JSON ответов, что **сломает** всю структуру ответа. Хорошего решения этой проблемы нет, хотя можно было бы сделать какие-то проверки.

Еще важный момент: для элементов в batch не создается отдельного scope, поэтому все `Scoped` сервисы будут одинаковыми в рамках одного batch-запроса.
Это может быть хорошо или плохо, в зависимости от сценария.