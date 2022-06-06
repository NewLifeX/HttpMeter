using System.Diagnostics;

namespace NewLife.HttpMeter
{
    /// <summary>压测统计信息</summary>
    public class MeterStat
    {
        #region 属性
        private Int32 _times;
        /// <summary>已完成次数</summary>
        public Int32 Times => _times;

        private Int32 _errors;
        /// <summary>错误数</summary>
        public Int32 Errors => _errors;

        private Int64 _cost;
        /// <summary>总耗时。</summary>
        public Int64 Cost => _cost;

        /// <summary>时间流逝</summary>
        public Stopwatch Watch { get; set; }
        #endregion

        #region 方法
        /// <summary>开始计时</summary>
        public void Start() => Watch = Stopwatch.StartNew();

        /// <summary>增加成功数</summary>
        public void Inc(Int64 cost)
        {
            Interlocked.Increment(ref _times);
            Interlocked.Add(ref _cost, cost);
        }

        /// <summary>增加错误数</summary>
        public void IncError(Int64 cost)
        {
            Interlocked.Increment(ref _errors); 
            Interlocked.Add(ref _cost, cost);
        }
        #endregion
    }
}