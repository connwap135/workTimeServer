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
                var ryList = DbContext.Instance.MySqlClinet.Queryable<QTSJ>().Where(x => x.RQ == "20201218").ToList();
                DbContext.Instance.Client.Insertable<QTSJ>(ryList).ExecuteCommand();
                Console.WriteLine($"����ͬ��{ryList.Count}����¼");
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
