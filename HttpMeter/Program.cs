using System.Reflection;
using NewLife;
using NewLife.HttpMeter;
using NewLife.Log;
using NewLife.Reflection;

XTrace.UseConsole();

//{
//    Console.WriteLine("HttpMeter v{0}", Assembly.GetExecutingAssembly().GetName().Version);
//    Console.WriteLine("Usage: httpmeter http://127.0.0.1:5000/api");
//    Console.WriteLine();
//}

try
{
    var cfg = new MeterOptions();

    // 分解参数
    if (args != null && args.Length > 0) cfg.Parse(args);

    // 显示帮助菜单或执行
    if (cfg.Url.IsNullOrEmpty())
        ShowHelp();
    else
    {
        var manager = new MeterManager { Options = cfg };
        manager.Start();
    }
}
catch (Exception ex)
{
    XTrace.WriteException(ex);
}

static void ShowHelp()
{
    Console.ForegroundColor = ConsoleColor.Yellow;

    var asm = AssemblyX.Entry;
    Console.WriteLine("{0} v{1}", asm.Name, asm.Version);
    //Console.WriteLine("HttpMeter v{0}", Assembly.GetExecutingAssembly().GetName().Version);
    Console.WriteLine("Usage: httpmeter  [-c 100] [-n 10000] [-i 0] [-s content] http://127.0.0.1");
    Console.WriteLine("\t-c  并发数。默认100用户");
    Console.WriteLine("\t-n  请求数。默认每用户请求10000次");
    Console.WriteLine("\t-i  间隔。间隔多少毫秒发一次请求");
    Console.WriteLine("\t-s  字符串内容。支持0x开头十六进制");

    Console.ResetColor();
}
