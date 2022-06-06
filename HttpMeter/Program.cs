using NewLife;
using NewLife.HttpMeter;
using NewLife.Log;
using Stardust;

XTrace.UseConsole();

try
{
    var cfg = new MeterOptions();

    // 分解参数
    if (args != null && args.Length > 0) cfg.Parse(args);

    var manager = new MeterManager
    {
        Options = cfg,
    };

    // 显示帮助菜单或执行
    if (cfg.Url.IsNullOrEmpty())
        manager.ShowHelp();
    else
    {
        var star = new StarFactory();
        manager.Tracer = star.Tracer;

        manager.ShowHeader();
        manager.Execute();
    }
}
catch (Exception ex)
{
    XTrace.WriteException(ex);
}