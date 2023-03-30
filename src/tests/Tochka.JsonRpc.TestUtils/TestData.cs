namespace Tochka.JsonRpc.TestUtils;

public record TestData(bool BoolField, string StringField, int IntField, double DoubleField, TestEnum EnumField, int[] ArrayField, string? NullableField, string? NotRequiredField = null, TestData? NestedField = null)
{
    #region Plain

    public static readonly TestData Plain = new(true,
        "123",
        123,
        1.23,
        TestEnum.Two,
        new[] { 1, 2, 3 },
        null);

    public const string PlainRequiredSnakeCaseJson = @"{
        ""bool_field"": true,
        ""string_field"": ""123"",
        ""int_field"": 123,
        ""double_field"": 1.23,
        ""enum_field"": ""two"",
        ""array_field"": [
            1,
            2,
            3
        ],
        ""nullable_field"": null
    }";

    public const string PlainFullSnakeCaseJson = @"{
        ""bool_field"": true,
        ""string_field"": ""123"",
        ""int_field"": 123,
        ""double_field"": 1.23,
        ""enum_field"": ""two"",
        ""array_field"": [
            1,
            2,
            3
        ],
        ""nullable_field"": null,
        ""not_required_field"": null,
        ""nested_field"": null
    }";

    public const string PlainRequiredCamelCaseJson = @"{
        ""boolField"": true,
        ""stringField"": ""123"",
        ""intField"": 123,
        ""doubleField"": 1.23,
        ""enumField"": ""two"",
        ""arrayField"": [
            1,
            2,
            3
        ],
        ""nullableField"": null
    }";

    public const string PlainFullCamelCaseJson = @"{
        ""boolField"": true,
        ""stringField"": ""123"",
        ""intField"": 123,
        ""doubleField"": 1.23,
        ""enumField"": ""two"",
        ""arrayField"": [
            1,
            2,
            3
        ],
        ""nullableField"": null,
        ""notRequiredField"": null,
        ""nestedField"": null
    }";

    #endregion

    #region Nested

    public static readonly TestData Nested = new(true,
        "123",
        123,
        1.23,
        TestEnum.Two,
        new[] { 1, 2, 3 },
        null,
        NestedField: new(true,
            "456",
            456,
            4.56,
            TestEnum.Two,
            new[] { 4, 5, 6 },
            null));

    public const string NestedRequiredSnakeCaseJson = @"{
        ""bool_field"": true,
        ""string_field"": ""123"",
        ""int_field"": 123,
        ""double_field"": 1.23,
        ""enum_field"": ""two"",
        ""array_field"": [
            1,
            2,
            3
        ],
        ""nullable_field"": null,
        ""nested_field"": {
            ""bool_field"": true,
            ""string_field"": ""456"",
            ""int_field"": 456,
            ""double_field"": 4.56,
            ""enum_field"": ""two"",
            ""array_field"": [
                4,
                5,
                6
            ],
            ""nullable_field"": null
        }
    }";

    public const string NestedFullSnakeCaseJson = @"{
        ""bool_field"": true,
        ""string_field"": ""123"",
        ""int_field"": 123,
        ""double_field"": 1.23,
        ""enum_field"": ""two"",
        ""array_field"": [
            1,
            2,
            3
        ],
        ""nullable_field"": null,
        ""not_required_field"": null,
        ""nested_field"": {
            ""bool_field"": true,
            ""string_field"": ""456"",
            ""int_field"": 456,
            ""double_field"": 4.56,
            ""enum_field"": ""two"",
            ""array_field"": [
                4,
                5,
                6
            ],
            ""nullable_field"": null,
            ""not_required_field"": null,
            ""nested_field"": null
        }
    }";

    public const string NestedRequiredCamelCaseJson = @"{
        ""boolField"": true,
        ""stringField"": ""123"",
        ""intField"": 123,
        ""doubleField"": 1.23,
        ""enumField"": ""two"",
        ""arrayField"": [
            1,
            2,
            3
        ],
        ""nullableField"": null,
        ""nestedField"": {
            ""boolField"": true,
            ""stringField"": ""456"",
            ""intField"": 456,
            ""doubleField"": 4.56,
            ""enumField"": ""two"",
            ""arrayField"": [
                4,
                5,
                6
            ],
            ""nullableField"": null
        }
    }";

    public const string NestedFullCamelCaseJson = @"{
        ""boolField"": true,
        ""stringField"": ""123"",
        ""intField"": 123,
        ""doubleField"": 1.23,
        ""enumField"": ""two"",
        ""arrayField"": [
            1,
            2,
            3
        ],
        ""nullableField"": null,
        ""notRequiredField"": null,
        ""nestedField"": {
            ""boolField"": true,
            ""stringField"": ""456"",
            ""intField"": 456,
            ""doubleField"": 4.56,
            ""enumField"": ""two"",
            ""arrayField"": [
                4,
                5,
                6
            ],
            ""nullableField"": null,
            ""notRequiredField"": null,
            ""nestedField"": null
        }
    }";

    #endregion
}

public enum TestEnum
{
    One,
    Two
}
