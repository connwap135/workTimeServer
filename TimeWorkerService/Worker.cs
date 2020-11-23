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
                _logger.LogError("�˿���ռ��,{0}", ex.Message);
                return;
            }

            RemotePoint = ipLocalPoint;
            mySocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, 1);
            _logger.LogInformation($"{DateTime.Now} D10���ڷ��������� for LINUX");
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            string msg;
            byte[] buf = new byte[1024];
            byte[] sendbuf = new byte[9];
            byte[] sendwritbuf = new byte[200];
            uint cardhao;
            int i;
            string readeripstr;//������IP��ַ
            string jihaostr;//����������
            string pktstr;//���ݰ����
            string cardnumberstr;//����
            string recestr = "";//����������ʾ

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
                    string RemoteIP = Convert.ToString(RemotePoint).Split(':')[0];                 //sock��ԴIP
                    int RemotePort = Convert.ToInt32(Convert.ToString(RemotePoint).Split(':')[1]); //Sock��Դ�˿�

                    recestr = "FromIP��" + (Convert.ToString(RemotePoint) + "                             ").Substring(0, 22) + "���Խ��գ�";
                    for (i = 0; i < rlen; i++)
                    {
                        recestr = recestr + buf[i].ToString("X2") + " ";
                    }

                    if ((buf[0] == (byte)0xc1) || (buf[0] == (byte)0xd1) || (buf[0] == (byte)0xd4)) //���յ�IC����ID��ˢ����Ϣ
                    {//���ճɹ�����������ͻ�Ӧ��Ϣ
                        sendbuf[0] = 0x69;
                        for (i = 1; i < 9; i++)
                        {
                            sendbuf[i] = buf[i];
                        }

                        readeripstr = RemoteIP;      //��������������������
                        IPEndPoint ipep = new IPEndPoint(IPAddress.Parse(readeripstr), RemotePort);

                        RemotePointls = ipep;
                        mySocket.SendTo(sendbuf, 9, SocketFlags.None, RemotePointls); //�����������ȷ�����յ�����Ϣ,�������������������
                        string sendstr = "SendTo��" + (RemotePointls + "                        ").Substring(0, 22) + "���Է��ͣ�";
                        for (i = 0; i < 9; i++)
                        { sendstr = sendstr + sendbuf[i].ToString("X2") + " "; }

                        if (revbufheadbak[0] == buf[0] && revbufheadbak[1] == buf[1] && revbufheadbak[2] == buf[2] && revbufheadbak[3] == buf[3] && revbufheadbak[4] == buf[4] && revbufheadbak[5] == buf[5] && revbufheadbak[6] == buf[6] && revbufheadbak[7] == buf[7] && revbufheadbak[8] == buf[8])
                        {
                            //���յ��ط������ݰ��������д���ֱ�Ӷ���                       
                        }
                        else
                        {
                            //���յ�ˢ����Ϣ��������IP��ַ[192.168.1.218],����[2],���ݰ����[21],������[00-B2-BA-A9-04]
                            jihaostr = Convert.ToString(buf[5] + buf[6] * 256);//����
                            pktstr = Convert.ToString(buf[7] + buf[8] * 256);//���ݰ����
                            if (buf[0] == (byte)0xc1)
                            { cardhao = (uint)(buf[13] * 256 * 256 * 256 + buf[12] * 256 * 256 + buf[11] * 256 + buf[10]); }
                            else
                            { cardhao = (uint)(buf[12] * 256 * 256 * 256 + buf[11] * 256 * 256 + buf[10] * 256 + buf[9]); }

                            cardnumberstr = "0000000000" + Convert.ToString(cardhao);//����
                            cardnumberstr = cardnumberstr.Substring(cardnumberstr.Length - 10, 10);
                            cardnumberstr = cardnumberstr.Substring(4, 6);
                            msg = "���յ�ˢ����Ϣ��������IP��ַ[" + readeripstr + "],����[" + jihaostr + "],���ݰ����[" + pktstr + "],������[" + cardnumberstr + "]";

                            //ΨһӲ�����
                            if (rlen > 14)
                            {
                                msg += ",ΨһӲ�����[";
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
                                var user = await DbContext.Instance.Client.Queryable<employee>().Where(x => x.e_sushe.Equals(cardnumberstr) && x.e_lzfs.Equals("��ְ")).WithCache().FirstAsync();
                                var strls1 = DateTime.Now.ToString("yy-MM-dd HH:mm:ss");
                                string CacheKey = $"VID{obj.GH}";
                                var cacheObj = cache.Get(CacheKey);
                                if (cacheObj == null)
                                {
                                    await DbContext.Instance.Client.Insertable(obj).ExecuteCommandAsync();
                                    MemoryCacheEntryOptions options = new MemoryCacheEntryOptions
                                    {
                                        AbsoluteExpiration = DateTime.Now.AddMinutes(15),
                                        SlidingExpiration = TimeSpan.FromMinutes(15)
                                    };
                                    cache.Set(CacheKey, DateTime.Now.ToString("HH:mm:ss"), options);
                                    Display(RemoteIP, RemotePort, strls1 + " " + cardnumberstr + " " + user?.e_xinming);
                                    if (IsUnix())
                                    {
                                        var parm = user?.e_xinming + "��ǩ��";
                                        ShellHelper.Bash($"/usr/local/ekho/bin/ekho {parm} -a 100 -s 60");
                                    }
                                }
                                else
                                {
                                    var str = cacheObj.ToString();
                                    Display(RemoteIP, RemotePort, "����15����ˢ��!�ϴδ�:" + str);
                                    if (IsUnix())
                                    {
                                        var parm = user?.e_xinming + "��ǩ��";
                                        ShellHelper.Bash($"/usr/local/ekho/bin/ekho {parm} -a 100 -s 60");
                                    }
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
                        //    var strls1 = DateTime.Now.ToString("yy-MM-dd HH:mm:ss") + "    ��ˢ��.....";
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

            sendbuf1[0] = 0x5a;//����ָ��
            sendbuf1[1] = (byte)(ii % 256);
            sendbuf1[2] = (byte)(ii / 256);
            sendbuf1[3] = 0xff;//����������
            sendbuf1[4] = 0x05;//��ʾ����ʱ�䣬��λΪ�룬Ϊ255ʱ��ʾ������ʾ
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
