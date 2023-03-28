namespace Tochka.JsonRpc.Client.Tests.Integration;

internal record TestData(bool BoolField, string StringField, int IntField, double DoubleField, TestEnum EnumField, int[] ArrayField, string? NullableField, string? NotRequiredField = null, TestData? NestedField = null)
{
    public static readonly TestData Plain = new(true,
        "123",
        123,
        1.23,
        TestEnum.Two,
        new[] { 1, 2, 3 },
        null);

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
}

internal enum TestEnum
{
    One,
    Two
}
