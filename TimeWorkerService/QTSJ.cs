namespace TimeWorkerService
{
    public partial class QTSJ
    {
        [SqlSugar.SugarColumn(IsNullable = true)]
        public short? N { get; set; }          
        [SqlSugar.SugarColumn(IsNullable = true, Length = 6)]
        public string GH { get; set; }          
        [SqlSugar.SugarColumn(IsNullable = true, Length = 8)]
        public string RQ { get; set; }         
        [SqlSugar.SugarColumn(IsNullable = true)]
        public short? HS1 { get; set; }        
        [SqlSugar.SugarColumn(IsNullable = true)]
        public short? MS1 { get; set; }         
        [SqlSugar.SugarColumn(IsNullable = true, Length = 1)]
        public string K { get; set; }         
        [SqlSugar.SugarColumn(IsNullable = true, Length = 10)]
        public string NUM { get; set; }        
        [SqlSugar.SugarColumn(IsNullable = true, Length = 2)]
        public string WN { get; set; }
    }
}
