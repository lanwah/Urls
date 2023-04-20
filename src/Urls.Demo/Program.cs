using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Urls.Demo
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var absoluteUrl = new AbsoluteUrl("https://learn.microsoft.com/zh-cn/");
            var relativeUrl = new RelativeUrl("search/?terms=ImmutableList");
            var absoluteUrlString = absoluteUrl.ToString();
            Console.WriteLine($"absoluteUrl：{absoluteUrlString}");
            var relativeUrlString = absoluteUrl.RelativeUrl.ToString();
            Console.WriteLine($"relativeUrlString in absoluteUrl：{relativeUrlString}");
            Console.WriteLine($"relativeUrl：{relativeUrl}");

            var newAbsoluteUrl = absoluteUrl.WithRelativeUrl(relativeUrl);
            absoluteUrlString = newAbsoluteUrl.ToString();
            Console.WriteLine($"absoluteUrl WithRelativeUrl relativeUrl：{absoluteUrlString}");

            relativeUrl = relativeUrl.AddQueryParameter("id", "1");
            absoluteUrlString = relativeUrl.ToString();

            var newRelativeUrl = absoluteUrl.RelativeUrl.Concat(relativeUrl);
            Console.WriteLine($"absoluteUrl.RelativeUrl Concat relativeUrl：{newRelativeUrl}");

            Console.WriteLine($"absoluteUrl WithRelativeUrl relativeUrl correct url：{absoluteUrl.WithRelativeUrl(relativeUrl)}");

            // 参数"search/?terms=ImmutableList"字符串隐式转换为了RelativeUrl
            var newAbsoluteUrl2 = absoluteUrl.WithRelativeUrl("search/?terms=ImmutableList");
            Console.WriteLine($"absoluteUrl WithRelativeUrl relativeUrl：{newAbsoluteUrl2}");

            Console.ReadKey();
        }
    }
}
