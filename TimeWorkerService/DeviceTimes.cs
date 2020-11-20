namespace TimeWorkerService
{/// <summary>
 /// 设备工作时间
 /// </summary>
    public class DeviceTimes
    {
        [SqlSugar.SugarColumn(IsPrimaryKey =true)]
        public int Yes { get; set; }
    }
}
