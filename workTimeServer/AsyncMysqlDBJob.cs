using Quartz;
using System;
using System.Threading.Tasks;

namespace workTimeServer
{
    [DisallowConcurrentExecution]
    public class AsyncMysqlDBJob : IJob
    {
        public Task Execute(IJobExecutionContext context)
        {
            return Task.Factory.StartNew(() =>
            {
                try
                {
                    //同步员工信息到树莓派Mysql
                    var ryList = DbContext.Instance.Client.Queryable<employee>().ToList();
                    DbContext.Instance.MySqlClinet.Saveable<employee>(ryList).ExecuteCommand();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            });
        }
    }
}