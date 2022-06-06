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
        /// <summary>可选项</summary>
        public MeterOptions Options { get; set; }

        /// <summary>链路追踪</summary>
        public ITracer Tracer { get; set; }
        #endregion

        #region 方法
        /// <summary>开始压测</summary>
        public void Execute()
        {
            var cfg = Options;

            _Counter = new PerfCounter();
            _Timer = new TimerX(ShowStat, null, 3_000, 5_000) { Async = true };
            var sw = Stopwatch.StartNew();

            // 多线程
            var ws = new List<MeterWorker>();
            var ts = new List<Task<Int32>>();
            for (var i = 0; i < cfg.Concurrency; i++)
            {
                var worker = new MeterWorker { Options = cfg, Tracer = Tracer };
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

        private Int32 _SessionCount;
        private ICounter _Counter;
        private TimerX _Timer;
        private String _LastStat;

        private void ShowStat(Object state)
        {
            var str = _Counter.ToString();
            if (_LastStat == str) return;
            _LastStat = str;

            XTrace.WriteLine("连接：{0:n0} 收发：{1}", _SessionCount, str);
        }
        #endregion

        #region 辅助
        /// <summary>显示帮助信息</summary>
        public void ShowHelp()
        {
            var cfg = Options;
            var asm = AssemblyX.Entry;

            Console.ForegroundColor = ConsoleColor.Yellow;

            Console.WriteLine("{0} v{1}", asm.Name, asm.Version);
            //Console.WriteLine("HttpMeter v{0}", Assembly.GetExecutingAssembly().GetName().Version);
            Console.WriteLine("Usage: httpmeter  [-c 100] [-n 10000] [-i 0] [-s content] http://127.0.0.1");
            Console.WriteLine("\t-c  并发数。默认100用户");
            Console.WriteLine("\t-n  请求数。默认每用户请求10000次");
            Console.WriteLine("\t-i  间隔。间隔多少毫秒发一次请求");
            Console.WriteLine("\t-s  字符串内容。支持0x开头十六进制");

            Console.ResetColor();
        }

        /// <summary>显示头部</summary>
        public void ShowHeader()
        {
            var cfg = Options;
            var asm = AssemblyX.Entry;

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("{0} v{1}", asm.Name, asm.Version);

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("目标：{0}", cfg.Url);
            Console.WriteLine("请求：{0:n0}", cfg.Times);
            Console.WriteLine("并发：{0:n0}", cfg.Concurrency);

            if (!cfg.Content.IsNullOrEmpty())
                Console.WriteLine("内容：[{0:n0}] {1}", cfg.Content.Length, cfg.Content[..100]);

            if (cfg.Interval > 0) Console.WriteLine("间隔：{0:n0}", cfg.Interval);

            Console.ResetColor();
            Console.WriteLine();
        }
        #endregion
    }
}