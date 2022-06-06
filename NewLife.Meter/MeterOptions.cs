namespace NewLife.HttpMeter;

/// <summary>压测选项</summary>
public class MeterOptions
{
    #region 属性
    /// <summary>请求地址。最后一个字符串</summary>
    public String Url { get; set; }

    /// <summary>次数。-n，默认10000</summary>
    public Int32 Times { get; set; } = 10000;

    /// <summary>并行度。-c，默认100</summary>
    public Int32 Concurrency { get; set; } = 100;

    /// <summary>间隔，默认0毫秒，持续发起请求</summary>
    public Int32 Interval { get; set; }

    /// <summary>请求方法。GET/POST/PostJson，默认GET</summary>
    public String Method { get; set; }

    /// <summary>请求令牌。用于JWT鉴权</summary>
    public String Token { get; set; }

    /// <summary>POST提交的文件</summary>
    public String File { get; set; }
    #endregion

    /// <summary>解析参数</summary>
    /// <param name="args"></param>
    public void Parse(String[] args)
    {
        for (var i = 0; i < args.Length; i++)
        {
            switch (args[i].ToLower())
            {
                case "-n":
                    if (i + 1 < args.Length)
                    {
                        Times = args[i + 1].ToInt();
                        i++;
                    }
                    break;
                case "-c":
                    if (i + 1 < args.Length)
                    {
                        Concurrency = args[i + 1].ToInt();
                        i++;
                    }
                    break;
                case "-i":
                    if (i + 1 < args.Length)
                    {
                        Interval = args[i + 1].ToInt();
                        i++;
                    }
                    break;
                case "-m":
                    if (i + 1 < args.Length)
                    {
                        Method = args[i + 1];
                        i++;
                    }
                    break;
                case "-t":
                    if (i + 1 < args.Length)
                    {
                        Token = args[i + 1];
                        i++;
                    }
                    break;
                case "-f":
                    if (i + 1 < args.Length)
                    {
                        File = args[i + 1];
                        i++;
                    }
                    break;
            }
        }

        var str = args.LastOrDefault();
        if (!str.StartsWith("-")) Url = str;
    }
}