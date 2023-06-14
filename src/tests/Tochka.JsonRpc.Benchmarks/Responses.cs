namespace Tochka.JsonRpc.Benchmarks;

internal static class Responses
{
    public static string GetPlainResponse(Guid id) => $@"
{{
  ""jsonrpc"": ""2.0"",
  ""id"": ""{id}"",
  ""result"": {{
    ""boolField"": true,
    ""stringField"": ""123"",
    ""intField"": 123,
    ""doubleField"": 1.23,
    ""enumField"": ""two"",
    ""nullableField"": null
  }}
}}";

    public static string GetNestedResponse(Guid id) => $@"
{{
  ""jsonrpc"": ""2.0"",
  ""id"": ""{id}"",
  ""result"": {{
    ""boolField"": true,
    ""stringField"": ""123"",
    ""intField"": 123,
    ""doubleField"": 1.23,
    ""enumField"": ""two"",
    ""nullableField"": null,
    ""nestedField"": {{
        ""boolField"": true,
        ""stringField"": ""456"",
        ""intField"": 456,
        ""doubleField"": 4.56,
        ""enumField"": ""two"",
        ""nullableField"": null,
        ""nestedField"": {{
            ""boolField"": true,
            ""stringField"": ""789"",
            ""intField"": 789,
            ""doubleField"": 7.89,
            ""enumField"": ""two"",
            ""nullableField"": null
        }}
    }}
  }}
}}";

    public static string GetBigResponse(Guid id) => $@"
{{
  ""jsonrpc"": ""2.0"",
  ""id"": ""{id}"",
  ""result"": {{
    ""boolField"": true,
    ""stringField"": ""0"",
    ""intField"": 0,
    ""doubleField"": 0.0,
    ""enumField"": ""two"",
    ""nullableField"": null,
    ""nestedField"": {{
        ""boolField"": true,
        ""stringField"": ""1"",
        ""intField"": 1,
        ""doubleField"": 0.1,
        ""enumField"": ""two"",
        ""nullableField"": null,
        ""nestedField"": {{
            ""boolField"": true,
            ""stringField"": ""2"",
            ""intField"": 2,
            ""doubleField"": 0.2,
            ""enumField"": ""two"",
            ""nullableField"": null,
            ""nestedField"": {{
                ""boolField"": true,
                ""stringField"": ""3"",
                ""intField"": 3,
                ""doubleField"": 0.3,
                ""enumField"": ""two"",
                ""nullableField"": null,
                ""nestedField"": {{
                    ""boolField"": true,
                    ""stringField"": ""4"",
                    ""intField"": 4,
                    ""doubleField"": 0.4,
                    ""enumField"": ""two"",
                    ""nullableField"": null,
                    ""nestedField"": {{
                        ""boolField"": true,
                        ""stringField"": ""5"",
                        ""intField"": 5,
                        ""doubleField"": 0.5,
                        ""enumField"": ""two"",
                        ""nullableField"": null,
                        ""nestedField"": {{
                            ""boolField"": true,
                            ""stringField"": ""6"",
                            ""intField"": 6,
                            ""doubleField"": 0.6,
                            ""enumField"": ""two"",
                            ""nullableField"": null,
                            ""nestedField"": {{
                                ""boolField"": true,
                                ""stringField"": ""7"",
                                ""intField"": 7,
                                ""doubleField"": 0.7,
                                ""enumField"": ""two"",
                                ""nullableField"": null,
                                ""nestedField"": {{
                                    ""boolField"": true,
                                    ""stringField"": ""8"",
                                    ""intField"": 8,
                                    ""doubleField"": 0.8,
                                    ""enumField"": ""two"",
                                    ""nullableField"": null,
                                    ""nestedField"": {{
                                        ""boolField"": true,
                                        ""stringField"": ""9"",
                                        ""intField"": 9,
                                        ""doubleField"": 0.9,
                                        ""enumField"": ""two"",
                                        ""nullableField"": null
                                    }}
                                }}
                            }}
                        }}
                    }}
                }}
            }}
        }}
    }}
  }}
}}";
}
