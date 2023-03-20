namespace Tochka.JsonRpc.Client.Tests.Integration;

internal record TestData(bool BoolField, string StringField, int IntField, double DoubleField, TestEnum EnumField, string? NullableField, string? NotRequiredField = null, TestData? NestedField = null)
{
    public static readonly TestData Plain = new(true,
        "123",
        123,
        1.23,
        TestEnum.Two,
        null);

    public static readonly TestData Nested = new(true,
        "123",
        123,
        1.23,
        TestEnum.Two,
        null,
        NestedField: new(true,
            "456",
            456,
            4.56,
            TestEnum.Two,
            null));
}

internal enum TestEnum
{
    One,
    Two
}
