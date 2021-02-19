using System;
using System.Runtime.InteropServices;
using workTimeServer;

namespace WorkerClient
{
    public class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                //ͬ����ݮ��Mysql��Ϣ��Mssql
                DateTime starttime = new DateTime(2021, 2, 8).Date;
                DateTime endtime = DateTime.Now.Date.AddDays(1).AddSeconds(-1);
                TimeSpan ts1 = new TimeSpan(starttime.Ticks);
                TimeSpan ts2 = new TimeSpan(endtime.Ticks);
                if (ts1 > ts2) { throw new Exception("��ʼ���ڲ�Ӧ���ڽ�������"); }
                TimeSpan ts = ts1.Subtract(ts2).Duration();
                if (ts.Days > 30)
                {
                    endtime = starttime.AddDays(30);
                }

                for (int i = 0; i < (ts.Days + 1); i++)//ɾ���Ѵ��ڵļ�¼
                {
                    var curDate = starttime.AddDays(i).ToString("yyyyMMdd");
                    //await QtsjDll.Instance.DeleteAsync(x => x.RQ.Equals(curDate) && x.WN.Equals("00"));
                    var ryList = DbContext.Instance.MySqlClinet.Queryable<QTSJ>().Where(x => x.RQ == curDate).ToList();
                    DbContext.Instance.Client.Insertable<QTSJ>(ryList).ExecuteCommand();
                    Console.WriteLine($"����ͬ��{ryList.Count}����¼");
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        //public static IHostBuilder CreateHostBuilder(string[] args) =>
        //    Host.CreateDefaultBuilder(args)
        //        .ConfigureServices((hostContext, services) =>
        //        {
        //            services.AddHostedService<Worker>();
        //        });
        public static bool IsUnix()
        {
            var isUnix = RuntimeInformation.IsOSPlatform(OSPlatform.OSX) || RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
            return isUnix;
        }
    }
}
