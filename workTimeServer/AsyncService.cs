using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Topshelf;

namespace workTimeServer
{
    public class AsyncService : ServiceControl
    {
        private readonly Socket mySocket;
        private readonly IPEndPoint ipLocalPoint;
        private bool RunningFlag;
        private Thread thread;
        byte[] revbufheadbak = new byte[20];
        byte[] cardnumberbuf = new byte[4];
        byte[] readcardbuf = new byte[768];
        byte[] writecardbuf = new byte[768];

        public AsyncService()
        {
            mySocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
#if DEBUG
            ipLocalPoint = new IPEndPoint(IPAddress.Parse("192.168.1.3"), 39169);
#else
            ipLocalPoint = new IPEndPoint(IPAddress.Parse("192.168.1.238"), 39169);
#endif
            mySocket.Bind(ipLocalPoint);
            IPEndPoint ipep = new IPEndPoint(IPAddress.Any, 39169);
            RemotePoint = (EndPoint)(ipep);
            mySocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, 1);
        }

        public EndPoint RemotePoint;

        public bool Start(HostControl hostControl)
        {
            RunningFlag = true;
            thread = new Thread(new ThreadStart(this.ReceiveHandle));
            thread.Start();
            return true;
        }

        private void ReceiveHandle()
        {
            //接收数据处理线程   
            string msg;
            byte[] buf = new byte[1024];
            byte[] sendbuf = new byte[9];
            byte[] sendwritbuf = new byte[200];

            uint cardhao;

            int i;
            //int j;
            //int m;

            string readeripstr;//读卡器IP地址
            string jihaostr;//读卡器机号
            string pktstr;//数据包序号
            string cardnumberstr;//卡号

            //string strls;
            string recestr = "";//接收数据显示
            //string readmac = "";//读卡器MAC
            //byte beep = 0x01;


            EndPoint RemotePointls;


            try
            {
                while (RunningFlag)
                {
                    if (mySocket == null || mySocket.Available < 1)
                    {
                        Thread.Sleep(30);
                        continue;
                    }
                    //跨线程调用控件   
                    //接收UDP数据报，引用参数RemotePoint获得源地址   


                    int rlen = mySocket.ReceiveFrom(buf, ref RemotePoint);

                    string RemoteIP = Convert.ToString(RemotePoint).Split(':')[0];                 //sock来源IP
                    int RemotePort = Convert.ToInt32(Convert.ToString(RemotePoint).Split(':')[1]); //Sock来源端口

                    recestr = "FromIP：" + (Convert.ToString(RemotePoint) + "                             ").Substring(0, 22) + "电脑接收：";
                    for (i = 0; i < rlen; i++)
                    {
                        recestr = recestr + buf[i].ToString("X2") + " ";
                    }

                    if ((buf[0] == (byte)0xc1) || (buf[0] == (byte)0xd1) || (buf[0] == (byte)0xd4)) //接收到IC卡或ID卡刷卡信息-------------------------------------------------------------------------------------------
                    {//接收成功向读卡器发送回应信息
                        sendbuf[0] = 0x69;
                        for (i = 1; i < 9; i++)
                        {
                            sendbuf[i] = buf[i];
                        }

                        readeripstr = RemoteIP;      //广域网、局域网都可以
                        IPEndPoint ipep = new IPEndPoint(IPAddress.Parse(readeripstr), RemotePort);

                        RemotePointls = (EndPoint)(ipep);
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
                                msg = msg + ",唯一硬件序号[";
                                for (i = 14; i < rlen; i++)
                                {
                                    msg = msg + buf[i].ToString("X2");
                                }
                                msg = msg + "]";
                            }
                            Console.WriteLine(msg);
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
                            //var IsAny = DbContext.Instance.Client.Queryable<QTSJ>().Where(x => x.GH == obj.GH && x.RQ == obj.RQ && x.HS1 == obj.HS1 && x.MS1 == obj.MS1).WithCache().Any();
                            //if (!IsAny)
                            //{
                            DbContext.Instance.Client.Insertable(obj).ExecuteCommand();
                            //}
                            var user = DbContext.Instance.Client.Queryable<employee>().Where(x => x.e_sushe.Equals(cardnumberstr) && x.e_lzfs.Equals("在职")).WithCache().First();
                            var strls1 = DateTime.Now.ToString("yy-MM-dd HH:mm:ss");
                            Display(RemoteIP, RemotePort, strls1 + " " + cardnumberstr + " " + user?.e_xinming);

                        }
                    }
                    else if (buf[0] == 0xf3)
                    {
                        var strls1 = DateTime.Now.ToString("yy-MM-dd HH:mm:ss");
                        Display(RemoteIP, RemotePort, strls1 + "    请刷卡.....");
                        //button2(RemoteIP, RemotePort);
                    }

                }

                mySocket.Close();
            }
            catch
            {

            }
        }

        public bool Stop(HostControl hostControl)
        {
            RunningFlag = false;
            return true;
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
            sendbuf1[4] = (byte)20;//显示保留时间，单位为秒，为255时表示永久显示
            byte[] strlsansi = System.Text.Encoding.GetEncoding(936).GetBytes(content);
            for (var i = 0; i < strlsansi.Length; i++)
            {
                sendbuf1[5 + i] = (byte)strlsansi[i];
            }
            IPEndPoint ipep = new IPEndPoint(IPAddress.Parse(ipAddress), port);
            var RemotePointls = (EndPoint)(ipep);
            mySocket.SendTo(sendbuf1, 39, SocketFlags.None, RemotePointls);
        }

        private void button2(string ipAddress, int port)
        {
            EndPoint RemotePointls;
            byte[] sendbuf;
            sendbuf = new byte[280];
            int i;
            int dispbytes;    //显示文字节数
            int speakbytes;   //语音播报字节表
            int sendbytes;    //报文数据长度
            IPEndPoint ipep = new IPEndPoint(IPAddress.Parse(ipAddress), port);
            RemotePointls = (EndPoint)(ipep);
            dispbytes = 34; //全屏显示长度，如不显示文字可以为0
            var strls = "[v10]";
            byte[] SpeakArr = System.Text.Encoding.GetEncoding(936).GetBytes(strls);//TTS语音转换为Ansi
            speakbytes = SpeakArr.GetLength(0);             //语音长度
            sendbytes = 11 + dispbytes + speakbytes + 4;    //总计发送数据长度

            sendbuf[0] = 0x5C;                             //表示驱动蜂鸣器+TTS组合语音+显示文字+继电器
            i = 0;            //机号
            sendbuf[1] = (byte)(i % 256);
            sendbuf[2] = (byte)(i / 256);
            sendbuf[3] = 0xff;
            sendbuf[3] = (byte)(sendbuf[3] & 0x80);
            sendbuf[4] = 0xF0;

            i = 300;      //继电器时长
            sendbuf[5] = (byte)(i % 256);
            sendbuf[6] = (byte)(i / 256);

            sendbuf[7] = (byte)(20);//显示保留时间，单位为秒，为255时表示永久显示
            sendbuf[8] = 0;                           //在显示屏中的哪个位置开始
            sendbuf[9] = (byte)dispbytes;             //显示字符串长度 0-34为全屏

            sendbuf[10] = (byte)speakbytes;           //TTS语音长度

            //显示文字的ASCII码
            strls = "12345                                      ";
            byte[] strlsansi = System.Text.Encoding.GetEncoding(936).GetBytes(strls);//显示文字转换为Ansi
            for (i = 0; i < speakbytes; i++)
            {
                sendbuf[11 + i] = (Byte)strlsansi[i];
            }

            for (i = 0; i < speakbytes; i++)          //连播语音代码
            {
                sendbuf[11 + sendbuf[9] + i] = (Byte)SpeakArr[i];
            }

            sendbuf[10 + sendbuf[9] + speakbytes + 1] = 0x55;   //防干扰后缀
            sendbuf[10 + sendbuf[9] + speakbytes + 2] = 0xAA;
            sendbuf[10 + sendbuf[9] + speakbytes + 3] = 0x66;
            sendbuf[10 + sendbuf[9] + speakbytes + 4] = 0x99;

            //try
            //{
            mySocket.SendTo(sendbuf, sendbytes, SocketFlags.None, RemotePointls);
            //    mySocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, 1);

            //    string sendstr = "SendTo：" + (RemotePointls + "                        ").Substring(0, 22) + "电脑发送：";

            //    for (i = 0; i < sendbytes; i++)
            //    {
            //        sendstr = sendstr + sendbuf[i].ToString("X2") + " ";
            //    }

            //}
            //catch (Exception ex)
            //{
            //    //MessageBox.Show(ex.Message.ToString());
            //}
        }
    }
}