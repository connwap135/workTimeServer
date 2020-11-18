using System;
using System.Text;
using Topshelf;

namespace workTimeServer
{
    class Program
    {
        static void Main(string[] args)
        {
            //DbContext.Instance.Client.DbFirst.CreateClassFile("d:\\Demo\\2", "Models");
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            HostFactory.Run(x => {
                x.SetDescription("D10考勤服务");
                x.SetServiceName("D10考勤服务");
                x.Service<AsyncService>();
                x.EnableServiceRecovery(r => r.RestartService(TimeSpan.FromSeconds(60)));
                x.StartAutomatically();
            });
        }
    }
}
