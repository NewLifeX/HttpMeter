using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text;
using NewLife.Http;
using NewLife.Log;

namespace NewLife.HttpMeter;

/// <summary>压测工作者</summary>
public class MeterWorker
{
    #region 属性
    /// <summary>序号</summary>
    public Int32 Index { get; set; }

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
    Byte[] _body;
    String _content;

    /// <summary>初始化环境，执行准备工作</summary>
    protected virtual Task InitAsync()
    {
        var client = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(15)
        };
        client.SetUserAgent();

        var cfg = Options;
        if (!cfg.Token.IsNullOrEmpty())
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", cfg.Token);
        }

        _client = client;

        if (!cfg.File.IsNullOrEmpty())
        {
            _body = File.ReadAllBytes(cfg.File.GetFullPath());
            _content = File.ReadAllText(cfg.File.GetFullPath());
        }

        return Task.CompletedTask;
    }

    /// <summary>处理单次请求</summary>
    /// <param name="index"></param>
    /// <returns></returns>
    protected virtual async Task ProcessAsync(Int32 index)
    {
        var client = _client;
        var cfg = Options;

        switch (cfg.Method?.ToLower())
        {
            case "postjson":
                {
                    var request = new HttpRequestMessage(HttpMethod.Post, cfg.Url);
                    if (!_content.IsNullOrEmpty())
                    {
                        // 简单的变量替换
                        var txt = _content.Replace("{{index}}", Index + "");
                        request.Content = new StringContent(txt, Encoding.UTF8, "application/json");
                    }

                    var response = await client.SendAsync(request);
                    response.EnsureSuccessStatusCode();
                    _ = await response.Content.ReadAsByteArrayAsync();

                    break;
                }
            case "post":
                {
                    var request = new HttpRequestMessage(HttpMethod.Post, cfg.Url);
                    if (_body != null)
                        request.Content = new ByteArrayContent(_body);

                    var response = await client.SendAsync(request);
                    response.EnsureSuccessStatusCode();
                    _ = await response.Content.ReadAsByteArrayAsync();

                    break;
                }
            case "get":
            default:
                {
                    // 简单的变量替换
                    var url = cfg.Url.Replace("{{index}}", Index + "");
                    _ = await client.GetStringAsync(url);
                }
                break;
        }
    }
}