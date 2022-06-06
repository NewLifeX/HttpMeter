using System.Diagnostics;
using NewLife.Http;
using NewLife.Log;

namespace NewLife.HttpMeter;

/// <summary>压测工作者</summary>
public class MeterWorker
{
    #region 属性
    /// <summary>可选项</summary>
    public MeterOptions Options { get; set; }

    /// <summary>统计信息</summary>
    public MeterStat Stat { get; set; }

    /// <summary>链路追踪</summary>
    public ITracer Tracer { get; set; }
    #endregion

    /// <summary>开始工作</summary>
    /// <returns></returns>
    public async Task<Int32> StartAsync()
    {
        var count = 0;
        try
        {
            var client = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(15)
            };
            client.SetUserAgent();

            await Task.Yield();

            var sw = new Stopwatch();
            var cfg = Options;
            var stat = Stat;
            for (var k = 0; k < cfg.Times; k++)
            {
                sw.Reset();
                sw.Restart();
                try
                {
                    var rs = await client.GetStringAsync(cfg.Url);
                    count++;

                    stat.Inc(sw.ElapsedMilliseconds);
                }
                catch
                {
                    stat.IncError(sw.ElapsedMilliseconds);
                }

                if (cfg.Interval > 0)
                    await Task.Delay(cfg.Interval);
                else
                    await Task.Yield();
            }
        }
        catch (Exception ex)
        {
            XTrace.WriteException(ex);
        }

        return count;
    }
}