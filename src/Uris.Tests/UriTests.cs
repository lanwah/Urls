using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Net;

#pragma warning disable CA1055 // Url-like return values should not be strings
#pragma warning disable IDE0057 // Use range operator
#pragma warning disable IDE0050 // Convert to tuple
#pragma warning disable CA1305 // Specify IFormatProvider

namespace Urls.UnitTests
{
    [TestClass]
    public class UrlTests
    {
        private const string Scheme = "http";
        private const string Host = "host.com";
        private const int Port = 5000;
        private const string PathPart1 = "pathpart1";
        private const string PathPart2 = "pathpart2";
        private const string FieldName1 = "fieldname1";
        private const string FieldName2 = "FieldName2";
        private const string FieldValue1 = "field<>Value1";
        private const string FieldValue2 = "field<>Value2";
        private const string FieldValueEncoded1 = "field%3C%3EValue1";
        private const string FieldValueEncoded2 = "field%3C%3EValue2";
        private const string Fragment = "frag";
        private const string Username = "username";
        private const string Password = "password";

        private readonly string expected = $"{Scheme}://{Username}:{Password}@{Host}:{Port}/{PathPart1}/{PathPart2}?" +
            $"{FieldName1}={FieldValueEncoded1}&{FieldName2}={FieldValueEncoded2}#{Fragment}";



        [TestMethod]
        public void TestQueryParameterEquality()
        {
            var qp1 = new QueryParameter("n", "v");
            var qp2 = new QueryParameter("n", "v");
            Assert.AreEqual(qp1, qp2);
        }

        [TestMethod]
        public void TestEquality()
        {
            var absoluteUrl1 = expected.ToAbsoluteUrl();
            Uri uri = absoluteUrl1;
            var uri2 = absoluteUrl1.ToUri();
            var absoluteUrl2 = (AbsoluteUrl)uri;

            Assert.AreEqual(uri, absoluteUrl1);
            Assert.AreEqual(uri.ToString(), WebUtility.UrlDecode(absoluteUrl2.ToString()));
            Assert.AreEqual(WebUtility.UrlDecode(absoluteUrl1.ToString()), uri.ToString());
            Assert.AreEqual(absoluteUrl1.ToString(), absoluteUrl2.ToString());

            //Just here to get more granular
            Assert.AreEqual(absoluteUrl1.UserInfo, absoluteUrl2.UserInfo);
            Assert.IsTrue(absoluteUrl1.RelativeUrl.QueryParameters.SequenceEqual(absoluteUrl2.RelativeUrl.QueryParameters));
            Assert.AreEqual(absoluteUrl1.RelativeUrl, absoluteUrl2.RelativeUrl);

            Assert.AreEqual(absoluteUrl1.RelativeUrl.GetHashCode(), absoluteUrl2.RelativeUrl.GetHashCode());

            Assert.AreEqual(absoluteUrl1, absoluteUrl2);
            Assert.AreEqual(absoluteUrl2, uri);
            Assert.AreEqual(absoluteUrl2, absoluteUrl1);

            Assert.AreEqual(uri, uri2);
        }

        [TestMethod]
        public void Test()
        {
            var uriString = new AbsoluteUrl(Scheme, Host, Port,
                new RelativeUrl(
                        ImmutableList.Create(PathPart1, PathPart2),
                        ImmutableList.Create(
                            new QueryParameter(FieldName1, FieldValue1),
                            new QueryParameter(FieldName2, FieldValue2)
                            )
                    , Fragment),
                    new UserInfo(Username, Password)).ToString();


            Assert.AreEqual(
                expected,
                uriString);
        }

        [TestMethod]
        public void TestComposition2()
        {
            var url =
                new AbsoluteUrl(Scheme, Host, Port)
                .AddQueryParameter(FieldName1, FieldValue1)
                .WithCredentials(Username, Password)
                .AddQueryParameter(FieldName2, FieldValue2)
                .WithFragment(Fragment)
                .WithPath(PathPart1, PathPart2);

            Assert.AreEqual(
                expected,
                url.ToString());
        }

        [TestMethod]
        public void TestLocalFunctionToUri()
        {
            static AbsoluteUrl SomeFunctionTakingAUri(Uri uri) => uri.ToAbsoluteUrl();
            var absoluteUrl = "www.test.com".ToHttpsUrlFromHost().WithPort(443);
            Assert.AreEqual(absoluteUrl, SomeFunctionTakingAUri(absoluteUrl));
        }

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

        [TestMethod]
        public void TestComposition3()
        {
            var uri = Host.ToHttpsUrlFromHost();

            Assert.AreEqual(
                $"https://{Host}",
                uri.ToString());
        }

        [TestMethod]
        public void TestAbsoluteWithRelative()
        {
            var absolute = new AbsoluteUrl(Scheme, Host);

            var relativeRelativeUrl = new RelativeUrl(
                                    ImmutableList.Create(PathPart1, PathPart2),
                                    ImmutableList.Create(
                                        new QueryParameter(FieldName1, FieldValue1),
                                        new QueryParameter(FieldName2, FieldValue2)
                                        )
                                );

            absolute = absolute.WithRelativeUrl(relativeRelativeUrl);

            Assert.AreEqual(
                relativeRelativeUrl.Fragment,
                absolute.RelativeUrl.Fragment);
        }

        [TestMethod]
        public void TestRelativeWithFragment()
        {
            var relativeRelativeUrl = new RelativeUrl(
                                    ImmutableList.Create(PathPart1, PathPart2),
                                    ImmutableList.Create(
                                        new QueryParameter(FieldName1, FieldValue1),
                                        new QueryParameter(FieldName2, FieldValue2)
                                        )
                                );

            const string frag = "test";

            relativeRelativeUrl = relativeRelativeUrl.WithFragment(frag);

            Assert.AreEqual(
                frag,
                relativeRelativeUrl.Fragment);
        }

#pragma warning disable IDE0034 // Simplify 'default' expression
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        [TestMethod]
        public void TestRelativeWithNullFragment()
        {
            var relativeRelativeUrl = new RelativeUrl(ImmutableList.Create(PathPart1, PathPart2), QueryParameter.EmptyList, default(string));
            Assert.AreEqual("", relativeRelativeUrl.Fragment);

            relativeRelativeUrl = new RelativeUrl(ImmutableList.Create(PathPart1, PathPart2), QueryParameter.EmptyList, null);
            Assert.AreEqual("", relativeRelativeUrl.Fragment);

            relativeRelativeUrl = new RelativeUrl(default(RelativeUrl));
            Assert.AreEqual("", relativeRelativeUrl.Fragment);
        }
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning restore IDE0034 // Simplify 'default' expression

        [TestMethod]
        public void TestWithQueryStringStrings()
        {
            var relativeRelativeUrl = RelativeUrl.Empty.AddQueryParameter(FieldName1, FieldValue1);

            Assert.AreEqual(
            FieldName1,
            relativeRelativeUrl.QueryParameters?.First().FieldName
            );

            Assert.AreEqual(
            FieldValue1,
            relativeRelativeUrl.QueryParameters?.First().Value
            );
        }

        [TestMethod]
        public void TestAbsoluteWithQueryStringStrings()
        {
            var absoluteRelativeUrl = new AbsoluteUrl("https", "test.com");

            absoluteRelativeUrl = absoluteRelativeUrl.AddQueryParameter(FieldName1, FieldValue1);

            Assert.AreEqual(
            FieldName1,
            absoluteRelativeUrl.RelativeUrl.QueryParameters.First().FieldName
            );

            Assert.AreEqual(
            FieldValue1,
            absoluteRelativeUrl.RelativeUrl.QueryParameters.First().Value
            );
        }

        [TestMethod]
        public void TestMinimalAbsoluteToString()
        => Assert.AreEqual("https://test.com", new AbsoluteUrl("https", "test.com").ToString());

        [TestMethod]
        public void TestConstructUri()
        {
            var uriString = new AbsoluteUrl(Scheme, Host, Port,
                new RelativeUrl(
                        ImmutableList.Create(PathPart1, PathPart2)
                        ,
                        ImmutableList.Create(
                            new QueryParameter(FieldName1, FieldValue1),
                            new QueryParameter(FieldName2, FieldValue2)
                            )
                    , Fragment),
                    new UserInfo(Username, Password)).ToString();

            var uri = new Uri(uriString, UriKind.Absolute);

            Assert.IsNotNull(uri);
            Assert.AreEqual(uri.Scheme, Scheme);
        }

        [TestMethod]
        public void TestWithQueryParams()
        {
            var item = new
            {
                somelongstring = "gvhhvhgfgfdg7676878",
                count = 50,
                message = "This is a sentence"
            };

            var relativeUrl = RelativeUrl.Empty.WithQueryParameters(item);

            Assert.AreEqual(item.somelongstring, relativeUrl.QueryParameters[0].Value);
            Assert.AreEqual(nameof(item.somelongstring), relativeUrl.QueryParameters[0].FieldName);
            Assert.AreEqual(item.count.ToString(), relativeUrl.QueryParameters[1].Value);
            Assert.AreEqual(nameof(item.count), relativeUrl.QueryParameters[1].FieldName);
            Assert.AreEqual(item.message, relativeUrl.QueryParameters[2].Value);
            Assert.AreEqual(nameof(item.message), relativeUrl.QueryParameters[2].FieldName);
        }

        [TestMethod]
        public void TestFromUri()
        {
            var uriString = new AbsoluteUrl(Scheme, Host, Port,
                new RelativeUrl(
                        ImmutableList.Create(PathPart1, PathPart2),
                        ImmutableList.Create(
                            new QueryParameter(FieldName1, FieldValue1),
                            new QueryParameter(FieldName2, FieldValue2)
                            )
                    , Fragment),
                    new UserInfo(Username, Password)).ToString();

            var uri = new Uri(uriString, UriKind.Absolute).ToAbsoluteUrl();

            Assert.IsNotNull(uri);
            Assert.AreEqual(uri.Scheme, Scheme);
            Assert.AreEqual(uri.RelativeUrl.Fragment, Fragment);
            Assert.AreEqual(uri.RelativeUrl.QueryParameters.First().FieldName, FieldName1);
            Assert.AreEqual(uri.RelativeUrl.QueryParameters.First().Value, FieldValue1);
            Assert.AreEqual(uri.RelativeUrl.QueryParameters[1].FieldName, FieldName2);
            Assert.AreEqual(uri.RelativeUrl.QueryParameters[1].Value, FieldValue2);
            Assert.AreEqual(Host, uri.Host);
            Assert.AreEqual(Port, uri.Port);
            Assert.AreEqual(PathPart1, uri.RelativeUrl.Path[0]);
            Assert.AreEqual(PathPart2, uri.RelativeUrl.Path[1]);
            Assert.AreEqual(Fragment, uri.RelativeUrl.Fragment);
            Assert.AreEqual(Username, uri.UserInfo?.Username);
            Assert.AreEqual(Password, uri.UserInfo?.Password);
        }

        [TestMethod]
        public void TestRelativeToAbsoluteUrlThrowsException()
            => Assert.AreEqual(UrlExtensions.ErrorMessageMustBeAbsolute, Assert.ThrowsException<InvalidOperationException>(()
                => new Uri("", UriKind.Relative).ToAbsoluteUrl()).Message
                );

        [TestMethod]
        public void TestRelativeToRelativeUrlThrowsException()
        => Assert.AreEqual(UrlExtensions.ErrorMessageMustBeAbsolute, Assert.ThrowsException<InvalidOperationException>(()
            => new Uri("", UriKind.Relative).ToRelativeUrl()).Message
            );

        [TestMethod]
        public void TestRelativeUrl()
        {
            var RelativeUrl = "/".ToRelativeUrl();
            Assert.AreEqual(string.Empty, RelativeUrl.ToString());
            Assert.IsTrue(RelativeUrl.QueryParameters.Count == 0);
        }

        [TestMethod]
        public void TestRelativeUrlConstructors()
        {
            var RelativeUrl = "a/a".ToRelativeUrl();
            Assert.IsTrue(RelativeUrl.Path.SequenceEqual(new[] { "a", "a" }));

            RelativeUrl = "a/".ToRelativeUrl();
            Assert.IsTrue(RelativeUrl.Path.SequenceEqual(new[] { "a" }));

            RelativeUrl = "a/b/c".ToRelativeUrl();
            Assert.IsTrue(RelativeUrl.Path.SequenceEqual(new[] { "a", "b", "c" }));

            RelativeUrl = "a/b/c/".ToRelativeUrl();
            Assert.IsTrue(RelativeUrl.Path.SequenceEqual(new[] { "a", "b", "c" }));

            RelativeUrl = "".ToRelativeUrl();
            Assert.IsTrue(RelativeUrl.Path.SequenceEqual(new string[] { }));

            RelativeUrl = "?asd=1".ToRelativeUrl();
            Assert.AreEqual("asd", RelativeUrl.QueryParameters[0].FieldName);
            Assert.AreEqual("1", RelativeUrl.QueryParameters[0].Value);

            RelativeUrl = "a/a?a=1#a".ToRelativeUrl();
            Assert.AreEqual("a", RelativeUrl.Path[0]);
            Assert.AreEqual("a", RelativeUrl.Path[1]);
            Assert.AreEqual("a", RelativeUrl.QueryParameters[0].FieldName);
            Assert.AreEqual("1", RelativeUrl.QueryParameters[0].Value);
            Assert.AreEqual("a", RelativeUrl.Fragment);

            RelativeUrl = "a/a?a=<hi>#a".ToRelativeUrl();
            Assert.AreEqual("a", RelativeUrl.Path[0]);
            Assert.AreEqual("a", RelativeUrl.Path[1]);
            Assert.AreEqual("a", RelativeUrl.QueryParameters[0].FieldName);
            Assert.AreEqual("<hi>", RelativeUrl.QueryParameters[0].Value);
            Assert.AreEqual("a", RelativeUrl.Fragment);

            RelativeUrl = new RelativeUrl("a/a?a=<hi>#a");
            Assert.AreEqual("a", RelativeUrl.Path[0]);
            Assert.AreEqual("a", RelativeUrl.Path[1]);
            Assert.AreEqual("a", RelativeUrl.QueryParameters[0].FieldName);
            Assert.AreEqual("<hi>", RelativeUrl.QueryParameters[0].Value);
            Assert.AreEqual("a", RelativeUrl.Fragment);

            RelativeUrl = "a/a?a#a".ToRelativeUrl();
            Assert.AreEqual("a", RelativeUrl.Path[0]);
            Assert.AreEqual("a", RelativeUrl.Path[1]);
            Assert.AreEqual("a", RelativeUrl.QueryParameters[0].FieldName);
            Assert.AreEqual(null, RelativeUrl.QueryParameters[0].Value);
            Assert.AreEqual("a", RelativeUrl.Fragment);
            Assert.AreEqual("/a/a?a#a", RelativeUrl.ToString());
        }

        [TestMethod]
        public void TestToAbsoluteUrlThings()
        {
            var absoluteUrl = new AbsoluteUrl($"{Scheme}://{Host}");

            Assert.AreEqual(Scheme, absoluteUrl.Scheme);
            Assert.AreEqual(Host, absoluteUrl.Host);

            absoluteUrl = new AbsoluteUrl($"{Scheme}://{Host}:{Port}");
            Assert.AreEqual(Port, absoluteUrl.Port);

            absoluteUrl = new AbsoluteUrl("http://www.hotmail.com");
            Assert.AreEqual("www.hotmail.com", absoluteUrl.Host);

            absoluteUrl = new AbsoluteUrl("http://bob:@www.hotmail.com");
            Assert.AreEqual("bob", absoluteUrl.UserInfo.Username);
        }

        [TestMethod]
        public void TestWithRelative()
        {
            const string urlString = "https://localhost:44337/JsonPerson";
            var baseUri = new AbsoluteUrl(urlString);
            var completeUri = baseUri.WithRelativeUrl(baseUri.RelativeUrl.AddQueryParameter("personKey", "abc"));
            Assert.AreEqual($"{urlString}?personKey=abc", completeUri.ToString());
        }

        [TestMethod]
        public void TestWithRelative2()
        {
            var relativeUrl = "/JsonPerson?personKey=123".ToRelativeUrl();
            Assert.AreEqual("personKey", relativeUrl.QueryParameters.First().FieldName);
        }

        [TestMethod]
        public void TestAppendPath()
        {
            const string urlString = "http://www.test.com:80/test";
            var baseUri = new AbsoluteUrl(urlString);
            var completeUri = baseUri.AppendPath("test");
            Assert.AreEqual($"http://www.test.com:80/test/test", completeUri.ToString());
        }

        [TestMethod]
        public void TestUserInfoStuff() => Assert.IsTrue((new UserInfo("a", "b") with { Username = "b" }).Equals(new UserInfo("b", "b")));

#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        [TestMethod]
        public void TestToAbsoluteUriNullGuard() => Assert.ThrowsException<ArgumentNullException>(() => ((Uri)null).ToAbsoluteUrl());

        [TestMethod]
        public void TestToRelativeUriNullGuard() => Assert.ThrowsException<ArgumentNullException>(() => ((Uri)null).ToRelativeUrl());

        [TestMethod]
        public void TestToRelativeUriNullGuard2() => Assert.ThrowsException<ArgumentNullException>(() => ((string)null).ToRelativeUrl());

        [TestMethod]
        public void TestAbsoluteUrlNullIsEmpty() => Assert.AreEqual(AbsoluteUrl.Empty, new AbsoluteUrl((AbsoluteUrl)null));

        [TestMethod]
        public void TestRelativeUrlNullIsEmpty() => Assert.AreEqual(RelativeUrl.Empty, new RelativeUrl((RelativeUrl)null));

        [TestMethod]
        public void TestUserInfoNullIsEmpty() => Assert.AreEqual(UserInfo.Empty, new UserInfo(null));
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.

        [TestMethod]
        public void TestToQueryParameter() => "a".ToQueryParameter("b").Equals(new QueryParameter("a", "b"));

        [TestMethod]
        public void TestEmpty()
        {
            Assert.AreEqual("://", AbsoluteUrl.Empty.ToString());
            Assert.AreEqual("", UserInfo.Empty.ToString());
            Assert.AreEqual("", RelativeUrl.Empty.ToString());
            Assert.AreEqual(0, QueryParameter.EmptyList.Count);
        }

        [TestMethod]
        public void TestToQueryParameters() => Assert.AreEqual("a", new QueryParameter("a", "a").ToQueryParameters()[0].FieldName);


        [TestMethod]
        public void TestCanUseWith()
        {
            const string bee = "b";
            const string ay = "a";
            var queryParameter = new QueryParameter(ay, ay) with { FieldName = bee, Value = "<b" };
            Assert.AreEqual(bee, queryParameter.FieldName);
            Assert.AreEqual("<b", queryParameter.Value);
            Assert.AreEqual("b=%3Cb", queryParameter.ToString());

            var relativeUrl = RelativeUrl.Empty with
            {
                Fragment = bee,
                Path = new List<string> { bee }.ToImmutableList(),
                QueryParameters = queryParameter.ToQueryParameters()
            };
            Assert.AreEqual(bee, relativeUrl.Fragment);
            Assert.IsTrue(queryParameter.ToQueryParameters().SequenceEqual(relativeUrl.QueryParameters));

        }

    }
}


