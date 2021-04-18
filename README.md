# Urls

Treat Urls as first class citizens

Urls is a .NET library of [records](https://docs.microsoft.com/en-us/dotnet/csharp/whats-new/tutorials/records) that represent Urls. Unlike the [Uri](https://docs.microsoft.com/en-us/dotnet/api/system.uri?view=net-5.0) class, all properties are immutable and there are a collection of Fluent API style extension methods to make Url construction easy. Use the `with` syntax to create new Urls easily, and make Http calls with [RestClient.Net](https://github.com/MelbourneDeveloper/RestClient.Net/tree/5/develop). 

```cs
private readonly string expected = $"{Scheme}://{Username}:{Password}@{Host}:{Port}/{PathPart1}/{PathPart2}?" +
  $"{FieldName1}={FieldValueEncoded1}&{FieldName2}={FieldValueEncoded2}#{Fragment}";

[TestMethod]
public void TestComposition2()
{
    var absoluteUri =
        Host.ToHttpUriFromHost(Port)
        .AddQueryParameter(FieldName1, FieldValue1)
        .WithCredentials(Username, Password)
        .AddQueryParameter(FieldName2, FieldValue2)
        .WithFragment(Fragment)
        .WithPath(PathPart1, PathPart2);

    Assert.AreEqual(
        expected,
        absoluteUri.ToString());
}
```

