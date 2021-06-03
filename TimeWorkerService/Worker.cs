using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace TimeWorkerService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly Socket mySocket;
        private readonly IPEndPoint ipLocalPoint;
        private EndPoint RemotePoint;
        byte[] revbufheadbak = new byte[20];
        private byte[] cardnumberbuf = new byte[4];
        private byte[] readcardbuf = new byte[768];
        private byte[] writecardbuf = new byte[768];
        private readonly IMemoryCache cache;

        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;
            cache = new MemoryCache(Options.Create(new MemoryCacheOptions()));
            mySocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            ipLocalPoint = new IPEndPoint(IPAddress.Any, 39169);
            try
            {
                mySocket.Bind(ipLocalPoint);
            }
            catch (Exception ex)
            {
                _logger.LogError("端口已占用,{0}", ex.Message);
                return;
            }

            RemotePoint = ipLocalPoint;
            mySocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, 1);
            _logger.LogInformation($"{DateTime.Now} D10考勤服务已启动 for LINUX");
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            string msg;
            byte[] buf = new byte[1024];
            byte[] sendbuf = new byte[9];
            byte[] sendwritbuf = new byte[200];
            uint cardhao;
            int i;
            string readeripstr;//读卡器IP地址
            string jihaostr;//读卡器机号
            string pktstr;//数据包序号
            string cardnumberstr;//卡号
            string recestr = "";//接收数据显示

            EndPoint RemotePointls;
            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    if (mySocket == null || mySocket.Available < 1)
                    {
                        await Task.Delay(30, stoppingToken);
                        continue;
                    }
                    int rlen = mySocket.ReceiveFrom(buf, ref RemotePoint);
                    string RemoteIP = Convert.ToString(RemotePoint).Split(':')[0];                 //sock来源IP
                    int RemotePort = Convert.ToInt32(Convert.ToString(RemotePoint).Split(':')[1]); //Sock来源端口

                    recestr = "FromIP：" + (Convert.ToString(RemotePoint) + "                             ").Substring(0, 22) + "电脑接收：";
                    for (i = 0; i < rlen; i++)
                    {
                        recestr = recestr + buf[i].ToString("X2") + " ";
                    }

                    if ((buf[0] == (byte)0xc1) || (buf[0] == (byte)0xd1) || (buf[0] == (byte)0xd4)) //接收到IC卡或ID卡刷卡信息
                    {//接收成功向读卡器发送回应信息
                        sendbuf[0] = 0x69;
                        for (i = 1; i < 9; i++)
                        {
                            sendbuf[i] = buf[i];
                        }

                        readeripstr = RemoteIP;      //广域网、局域网都可以
                        IPEndPoint ipep = new IPEndPoint(IPAddress.Parse(readeripstr), RemotePort);

                        RemotePointls = ipep;
                        mySocket.SendTo(sendbuf, 9, SocketFlags.None, RemotePointls); //向读卡器发送确认已收到的信息,否则读卡器将发送三次
                        string sendstr = "SendTo：" + (RemotePointls + "                        ").Substring(0, 22) + "电脑发送：";
                        for (i = 0; i < 9; i++)
                        { sendstr = sendstr + sendbuf[i].ToString("X2") + " "; }

                        if (revbufheadbak[0] == buf[0] && revbufheadbak[1] == buf[1] && revbufheadbak[2] == buf[2] && revbufheadbak[3] == buf[3] && revbufheadbak[4] == buf[4] && revbufheadbak[5] == buf[5] && revbufheadbak[6] == buf[6] && revbufheadbak[7] == buf[7] && revbufheadbak[8] == buf[8])
                        {
                            //接收到重发的数据包，不进行处理直接丢弃                       
                        }
                        else
                        {
                            //接收到刷卡信息：读卡器IP地址[192.168.1.218],机号[2],数据包序号[21],物理卡号[00-B2-BA-A9-04]
                            jihaostr = Convert.ToString(buf[5] + buf[6] * 256);//机号
                            pktstr = Convert.ToString(buf[7] + buf[8] * 256);//数据包序号
                            if (buf[0] == (byte)0xc1)
                            { cardhao = (uint)(buf[13] * 256 * 256 * 256 + buf[12] * 256 * 256 + buf[11] * 256 + buf[10]); }
                            else
                            { cardhao = (uint)(buf[12] * 256 * 256 * 256 + buf[11] * 256 * 256 + buf[10] * 256 + buf[9]); }

                            cardnumberstr = "0000000000" + Convert.ToString(cardhao);//卡号
                            cardnumberstr = cardnumberstr.Substring(cardnumberstr.Length - 10, 10);
                            cardnumberstr = cardnumberstr.Substring(4, 6);
                            msg = "接收到刷卡信息：读卡器IP地址[" + readeripstr + "],机号[" + jihaostr + "],数据包序号[" + pktstr + "],物理卡号[" + cardnumberstr + "]";

                            //唯一硬件序号
                            if (rlen > 14)
                            {
                                msg += ",唯一硬件序号[";
                                for (i = 14; i < rlen; i++)
                                {
                                    msg += buf[i].ToString("X2");
                                }
                                msg += "]";
                            }
                            _logger.LogInformation(msg);
                            var ts = DateTime.Now;
                            var obj = new QTSJ
                            {
                                GH = cardnumberstr,
                                RQ = ts.ToString("yyyyMMdd"),
                                HS1 = (short?)ts.Hour,
                                MS1 = (short?)ts.Minute,
                                K = "",
                                NUM = ts.ToString("yyyy-MM-dd"),
                                WN = jihaostr
                            };
                            try
                            {
                                var user = await DbContext.Instance.Client.Queryable<employee>().Where(x => x.e_sushe.Equals(cardnumberstr) && x.e_lzfs.Equals("在职")).FirstAsync();
                                var strls1 = DateTime.Now.ToString("yy-MM-dd HH:mm:ss");
                                string CacheKey = $"VID{obj.GH}";
                                var cacheObj = cache.Get(CacheKey);
                                if (cacheObj == null || user.e_xinming.Equals("宋小康"))
                                {
                                    await DbContext.Instance.Client.Insertable(obj).ExecuteCommandAsync();
                                    MemoryCacheEntryOptions options = new MemoryCacheEntryOptions
                                    {
                                        AbsoluteExpiration = DateTime.Now.AddMinutes(15),
                                        SlidingExpiration = TimeSpan.FromMinutes(15)
                                    };
                                    cache.Set(CacheKey, DateTime.Now.ToString("HH:mm:ss"), options);
                                    Display(RemoteIP, RemotePort, strls1 + " " + cardnumberstr + " " + user?.e_xinming);
                                    //if (IsUnix())
                                    //{
                                    //    var parm = user?.e_xinming + "已签到";
                                    //    ShellHelper.Bash($"/usr/local/ekho/bin/ekho {parm} -a 100 -s 60");
                                    //}
                                }
                                else
                                {
                                    var str = cacheObj.ToString();
                                    Display(RemoteIP, RemotePort, "请间隔15分钟刷卡!上次打卡:" + str);
                                    //if (IsUnix())
                                    //{
                                    //    var parm = user?.e_xinming + "已签到";
                                    //    ShellHelper.Bash($"/usr/local/ekho/bin/ekho {parm} -a 100 -s 60");
                                    //}
                                }
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex.Message);
                            }
                        }
                    }
                    else if (buf[0] == 0xf3)
                    {
                        //var any = await DbContext.Instance.Client.Queryable<DeviceTimes>().Where(x => x.Yes == DateTime.Now.Hour).WithCache(3600).AnyAsync();
                        //if (!any)
                        //{
                        //    var strls1 = DateTime.Now.ToString("yy-MM-dd HH:mm:ss") + "    请刷卡.....";
                        //    Display(RemoteIP, RemotePort, strls1);
                        //    _logger.LogInformation($"{RemoteIP}:{RemotePort}     {strls1}");
                        //}
                        //_logger.LogInformation($"{RemoteIP}:{RemotePort}     {DateTime.Now}");
                    }
                }
                mySocket.Close();
            }
            catch
            {

            }
        }

        private void Display(string ipAddress, int port, string content)
        {
            byte[] sendbuf1;
            sendbuf1 = new byte[39];
            int ii = 0;

            sendbuf1[0] = 0x5a;//声响指令
            sendbuf1[1] = (byte)(ii % 256);
            sendbuf1[2] = (byte)(ii / 256);
            sendbuf1[3] = 0xff;//不发出声响
            sendbuf1[4] = 0x05;//显示保留时间，单位为秒，为255时表示永久显示
            byte[] strlsansi = System.Text.Encoding.GetEncoding(936).GetBytes(content);
            for (var i = 0; i < (strlsansi.Length > 34 ? 34 : strlsansi.Length); i++)
            {
                sendbuf1[5 + i] = (byte)strlsansi[i];
            }
            IPEndPoint ipep = new IPEndPoint(IPAddress.Parse(ipAddress), port);
            var RemotePointls = (EndPoint)(ipep);
            mySocket.SendTo(sendbuf1, 39, SocketFlags.None, RemotePointls);
        }

        public static bool IsUnix()
        {
            var isUnix = RuntimeInformation.IsOSPlatform(OSPlatform.OSX) || RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
            return isUnix;
        }
    }
}
