using System.Diagnostics;
using NewLife.Log;
using NewLife.Reflection;
using NewLife.Threading;

namespace NewLife.HttpMeter
{
    /// <summary>测试管理器</summary>
    public class MeterManager
    {
        #region 属性
        public MeterOptions Options { get; set; }
        #endregion

        #region 方法
        public void Start()
        {
            var cfg = Options;
            var asm = AssemblyX.Entry;

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("{0} v{1}", asm.Name, asm.Version);

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("目标：{0}", cfg.Url);
            Console.WriteLine("请求：{0:n0}", cfg.Times);
            Console.WriteLine("并发：{0:n0}", cfg.Concurrency);
            Console.WriteLine("内容：[{0:n0}] {1}", cfg.Content.Length, cfg.Content[..100]);

            if (cfg.Interval > 0) Console.WriteLine("间隔：{0:n0}", cfg.Interval);

            Console.ResetColor();
            Console.WriteLine();
            _Counter = new PerfCounter();
            _Timer = new TimerX(ShowStat, null, 3_000, 5_000) { Async = true };
            var sw = Stopwatch.StartNew();

            // 多线程
            var ws = new List<MeterWorker>();
            var ts = new List<Task<Int32>>();
            for (var i = 0; i < cfg.Concurrency; i++)
            {
                var worker = new MeterWorker { Options = cfg };
                //worker.StartAsync();

                ws.Add(worker);

                var task = Task.Run(async () => await worker.StartAsync());
                ts.Add(task);
            }

            Console.WriteLine("{0:n0} 个并发已就绪", ws.Count);
            var total = Task.WhenAll(ts.ToArray()).Result.Sum();

            sw.Stop();

            Console.WriteLine("完成：{0:n0}", total);

            var ms = sw.Elapsed.TotalMilliseconds;
            Console.WriteLine("速度：{0:n0}tps", total * 1000L / ms);
        }

        private static Int32 _SessionCount;
        private static ICounter _Counter;
        private static TimerX _Timer;
        private static String _LastStat;

        private static void ShowStat(Object state)
        {
            var str = _Counter.ToString();
            if (_LastStat == str) return;
            _LastStat = str;

            XTrace.WriteLine("连接：{0:n0} 收发：{1}", _SessionCount, str);
        }
        #endregion
    }
}