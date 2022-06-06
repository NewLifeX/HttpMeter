using System.Net;
using NewLife.Collections;
using NewLife.Data;
using NewLife.Log;
using NewLife.Net;

namespace NewLife.HttpMeter;

/// <summary>压测工作者</summary>
public class MeterWorker
{
    #region 属性
    /// <summary>可选项</summary>
    public MeterOptions Options { get; set; }

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

            await Task.Yield();

            var cfg = Options;
            for (var k = 0; k < cfg.Times; k++)
            {
                var rs = await client.GetStringAsync(cfg.Url);
                count++;

                if (cfg.Interval > 0)
                    await Task.Delay(cfg.Interval);
                else
                    await Task.Yield();
            }
        }
        catch (Exception ex)
        {
        }

        return count;
    }
}