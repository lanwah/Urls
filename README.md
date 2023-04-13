# Urls

于2023/04/12从 [MelbourneDeveloper/Urls](https://github.com/MelbourneDeveloper/Urls) 迁出，以便代码的升级和bug修复。

希望用到此库发现问题的人员能提出bug一起参与维护此库。

---

Treat Urls as first-class citizens

[![.NET](https://github.com/MelbourneDeveloper/Urls/actions/workflows/dotnet.yml/badge.svg?branch=main)](https://github.com/MelbourneDeveloper/Urls/actions/workflows/dotnet.yml)

Nuget: [Urls](https://www.nuget.org/packages/Urls) 

| .NET Framework 4.5 | .NET Standard 2.0 | .NET Core 5.0 |
|--------------------|:-----------------:|---------------|

Urls is a .NET library of [records](https://docs.microsoft.com/en-us/dotnet/csharp/whats-new/tutorials/records) that represent Urls. All properties are immutable, and there are a collection of Fluent API style extension methods to make Url construction easy. I designed this library with F# in mind. Use the [non-destructive mutation](https://docs.microsoft.com/en-us/dotnet/csharp/whats-new/tutorials/records#non-destructive-mutation) (`with`) syntax to create new Urls easily and make HTTP calls with [RestClient.Net](https://github.com/MelbourneDeveloper/RestClient.Net/tree/5/develop). 

See all samples in the unit tests [here](https://github.com/MelbourneDeveloper/Urls/blob/ab57a866d27cb5653b97ca6fcf8fe51242d5b274/src/Uris.Tests/UriTests.cs#L38).

#### C#

```cs
private readonly string expected = $"{Scheme}://{Username}:{Password}@{Host}:{Port}/{PathPart1}/{PathPart2}?" +
  $"{FieldName1}={FieldValueEncoded1}&{FieldName2}={FieldValueEncoded2}#{Fragment}";

[TestMethod]
public void TestComposition()
{
    var absoluteUrl =
        Host.ToHttpUrlFromHost(Port)
        .AddQueryParameter(FieldName1, FieldValue1)
        .WithCredentials(Username, Password)
        .AddQueryParameter(FieldName2, FieldValue2)
        .WithFragment(Fragment)
        .WithPath(PathPart1, PathPart2);

    Assert.AreEqual(
        expected,
        absoluteUrl.ToString());

    //C# 9 records non-destructive mutation (with syntax)
    var absoluteUrl2 = absoluteUrl with { Port = 1000 };

    Assert.AreEqual(1000, absoluteUrl2.Port);
}
```

#### F#

```fs
[<TestMethod>]
member this.TestComposition () =

    let uri =
        "host.com".ToHttpUrlFromHost(5000)
        .AddQueryParameter("fieldname1", "field<>Value1")
        .WithCredentials("username", "password")
        .AddQueryParameter("FieldName2", "field<>Value2")
        .WithFragment("frag")
        .WithPath("pathpart1", "pathpart2")

    Assert.AreEqual("http://username:password@host.com:5000/pathpart1/pathpart2?fieldname1=field%3C%3EValue1&FieldName2=field%3C%3EValue2#frag",uri.ToString());
```

#### Pass `AbsoluteUrl` as `System.Uri`

Automatically convert between `System.Uri` and back

```cs
public static HttpClient GetHttpClientWithAbsoluteUrl
    => GetHttpClient(new AbsoluteUrl("http", "host.com")
        .AddQueryParameter(FieldName1, FieldValue1));

public static HttpClient GetHttpClient(Uri uri) => new() { BaseAddress = uri };

public static Uri GetUri() => new AbsoluteUrl("http", "host.com").ToUri();
```

#### Quality First

![Code Coverage](https://github.com/MelbourneDeveloper/Urls/blob/main/Images/CodeCoverage.png) 
![Mutation Score](https://github.com/MelbourneDeveloper/Urls/blob/main/Images/MutationScore.png)

