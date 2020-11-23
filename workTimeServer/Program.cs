using System;
using System.Text;
using Topshelf;

namespace workTimeServer
{
    class Program
    {
        static void Main(string[] args)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            var title = "D10考勤服务";
            HostFactory.Run(x => {
                x.SetServiceName(title);
                x.SetDisplayName(title);
                x.SetDescription(title);
                x.Service<AsyncService>();
                x.EnableServiceRecovery(r => r.RestartService(TimeSpan.FromSeconds(60)));
                x.StartAutomatically();
            });
        }
    }
}
