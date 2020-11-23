using System;
using System.Collections.Generic;
using System.Text;

namespace workTimeServer
{
    ///<summary>
    /// 员工信息表
    ///</summary>
#pragma warning disable IDE1006
    public partial class employee
    {
        [SqlSugar.SugarColumn(IsPrimaryKey = true, Length = 5)]
        public string e_id { get; set; }
        [SqlSugar.SugarColumn(Length = 12)]
        public string e_xinming { get; set; }
        [SqlSugar.SugarColumn(IsNullable = true, Length = 2)]
        public string e_xinbie { get; set; }
        [SqlSugar.SugarColumn(IsNullable = true, Length = 10)]
        public string e_mingzu { get; set; }
        [SqlSugar.SugarColumn(IsNullable = true, Length = 8)]
        public string e_csny { get; set; }
        [SqlSugar.SugarColumn(IsNullable = true)]
        public double? e_yuefen { get; set; }
        [SqlSugar.SugarColumn(IsNullable = true, Length = 8)]
        public string e_xueli { get; set; }
        [SqlSugar.SugarColumn(IsNullable = true, Length = 30)]
        public string e_xuexiao { get; set; }
        [SqlSugar.SugarColumn(IsNullable = true, Length = 20)]
        public string e_zhuye { get; set; }
        [SqlSugar.SugarColumn(IsNullable = true)]
        public double? e_sg { get; set; }
        [SqlSugar.SugarColumn(IsNullable = true)]
        public double? e_tz { get; set; }
        [SqlSugar.SugarColumn(Length = 20)]
        public string e_bumen { get; set; }
        [SqlSugar.SugarColumn(Length = 20)]
        public string e_banzhu { get; set; }
        [SqlSugar.SugarColumn(Length = 20)]
        public string e_zhiwu { get; set; }
        [SqlSugar.SugarColumn(IsNullable = true, Length = 8)]
        public string e_rcrq { get; set; }
        [SqlSugar.SugarColumn(IsNullable = true, Length = 20)]
        public string e_senfen { get; set; }
        [SqlSugar.SugarColumn(IsNullable = true, Length = 60)]
        public string e_dizhi { get; set; }
        [SqlSugar.SugarColumn(IsNullable = true, Length = 20)]
        public string e_sfz { get; set; }
        [SqlSugar.SugarColumn(IsNullable = true, Length = 15)]
        public string e_dianhua { get; set; }
        [SqlSugar.SugarColumn(IsNullable = true, Length = 1)]
        public string e_jiehun { get; set; }
        [SqlSugar.SugarColumn(IsNullable = true, Length = 12)]
        public string e_spouse { get; set; }
        [SqlSugar.SugarColumn(IsNullable = true, Length = 40)]
        public string e_techar { get; set; }
        [SqlSugar.SugarColumn(IsNullable = true, Length = 40)]
        public string e_aihao { get; set; }
        [SqlSugar.SugarColumn(IsNullable = true, Length = 1)]
        public string e_js { get; set; }
        [SqlSugar.SugarColumn(IsNullable = true, Length = 8)]
        public string e_rzrq { get; set; }
        [SqlSugar.SugarColumn(IsNullable = true, Length = 8)]
        public string e_tzrq { get; set; }
        [SqlSugar.SugarColumn(Length = 6)]
        public string e_sushe { get; set; }
        [SqlSugar.SugarColumn(IsNullable = true, Length = 4)]
        public string e_changhao { get; set; }
        [SqlSugar.SugarColumn(IsNullable = true, Length = 300)]
        public string e_beizhu { get; set; }
        [SqlSugar.SugarColumn(IsNullable = true, Length = 8)]
        public string e_xingzi { get; set; }
        [SqlSugar.SugarColumn(IsNullable = true, Length = 8)]
        public string e_lizhi { get; set; }
        [SqlSugar.SugarColumn(IsNullable = true, Length = 10)]
        public string e_lzfs { get; set; }
        [SqlSugar.SugarColumn(IsNullable = true)]
        public DateTime? e_hetong { get; set; }
        [SqlSugar.SugarColumn(IsNullable = true, Length = 1)]
        public string e_fk { get; set; }
        [SqlSugar.SugarColumn(IsNullable = true, Length = 1)]
        public string e_cz { get; set; }
    }
#pragma warning restore IDE1006
}
