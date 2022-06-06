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
    public virtual async Task<Int32> ExecuteAsync()
    {
        await Task.Yield();

        var count = 0;
        try
        {
            await InitAsync();

            var sw = new Stopwatch();
            var cfg = Options;
            var stat = Stat;
            for (var k = 0; k < cfg.Times; k++)
            {
                sw.Reset();
                sw.Restart();
                try
                {
                    await ProcessAsync(k);
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

    HttpClient _client;
    /// <summary>初始化环境，执行准备工作</summary>
    protected virtual Task InitAsync()
    {
        var client = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(15)
        };
        client.SetUserAgent();

        _client = client;

        return Task.CompletedTask;
    }

    /// <summary>处理单次请求</summary>
    /// <param name="index"></param>
    /// <returns></returns>
    protected virtual async Task ProcessAsync(Int32 index)
    {
        var client = _client;
        var cfg = Options;

        var rs = await client.GetStringAsync(cfg.Url);
    }
}