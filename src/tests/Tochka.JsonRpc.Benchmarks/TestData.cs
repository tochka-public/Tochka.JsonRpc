namespace Tochka.JsonRpc.Benchmarks;

public record TestData(bool BoolField, string StringField, int IntField, double DoubleField, TestEnum EnumField, string? NullableField, string? NotRequiredField = null, TestData? NestedField = null, string? Name = null)
{
    public override string? ToString() => string.IsNullOrWhiteSpace(Name)
        ? base.ToString()
        : Name;

    public static readonly TestData Plain = new(true,
        "123",
        123,
        1.23,
        TestEnum.Two,
        null,
        Name: nameof(Plain));

    public static readonly TestData Nested = new(true,
        "123",
        123,
        1.23,
        TestEnum.Two,
        null,
        Name: nameof(Nested),
        NestedField: new(true,
            "456",
            456,
            4.56,
            TestEnum.Two,
            null,
            NestedField: new(true,
                "789",
                789,
                7.89,
                TestEnum.Two,
                null)));

    public static readonly TestData Big = new(true,
        "0",
        0,
        0.0,
        TestEnum.Two,
        null,
        Name: nameof(Big),
        NestedField: new(true,
            "1",
            1,
            0.1,
            TestEnum.Two,
            null,
            NestedField: new(true,
                "2",
                2,
                0.2,
                TestEnum.Two,
                null,
                NestedField: new(true,
                    "3",
                    3,
                    0.3,
                    TestEnum.Two,
                    null,
                    NestedField: new(true,
                        "4",
                        4,
                        0.4,
                        TestEnum.Two,
                        null,
                        NestedField: new(true,
                            "5",
                            5,
                            0.5,
                            TestEnum.Two,
                            null,
                            NestedField: new(true,
                                "6",
                                6,
                                0.6,
                                TestEnum.Two,
                                null,
                                NestedField: new(true,
                                    "7",
                                    7,
                                    0.7,
                                    TestEnum.Two,
                                    null,
                                    NestedField: new(true,
                                        "8",
                                        8,
                                        0.8,
                                        TestEnum.Two,
                                        null,
                                        NestedField: new(true,
                                            "9",
                                            9,
                                            0.9,
                                            TestEnum.Two,
                                            null))))))))));

    public Dictionary<string, object?> ToDictionary() => new()
    {
        ["boolField"] = BoolField,
        ["stringField"] = StringField,
        ["intField"] = IntField,
        ["doubleField"] = DoubleField,
        ["enumField"] = EnumField,
        ["nullableField"] = NullableField,
        ["nestedField"] = NestedField?.ToDictionary(),
    };
}

public enum TestEnum
{
    One,
    Two
}
