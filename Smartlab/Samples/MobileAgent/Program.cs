// This application entry point is based on ASP.NET Core new project templates and is included
// as a starting point for app host configuration.
// This file may need to be updated according to the specific scenario of the application being upgraded.
// For more information on ASP.NET Core hosting, see https://docs.microsoft.com/aspnet/core/fundamentals/host/web-host

using CMU.Smartlab.Communication;
using CMU.Smartlab.Identity;
// using CMU.Smartlab.Rtsp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
// using Microsoft.Kinect;
using Microsoft.Psi;
using Microsoft.Psi.Components;
using Microsoft.Psi.Audio;
using Microsoft.Psi.CognitiveServices;
using Microsoft.Psi.CognitiveServices.Speech;
using Microsoft.Psi.Imaging;
using Microsoft.Psi.Media;
using Microsoft.Psi.Speech;
using Microsoft.Psi.Interop.Format;
using Microsoft.Psi.Interop.Transport;
// using Microsoft.Psi.Kinect;
using Apache.NMS;
using Apache.NMS.ActiveMQ;
using Apache.NMS.ActiveMQ.Transport.Discovery;
using Rectangle = System.Drawing.Rectangle;
using NetMQ;
using NetMQ.Sockets;
using Newtonsoft.Json;
// using NetMQSource;
// using ZeroMQ; 
// using Operators; 


namespace SigdialDemo
{
    public class NanoIPs
    {
        public string audio_channel { get; set; }
        public string doa { get; set; }
        public string vad { get; set; }
        public string remoteIP { get; set; }
        public string cvPreds { get; set; }
        public string images { get; set; }
    }
    public class Program
    {
        private const string AppName = "SmartLab Project - Demo v3.0 (for SigDial Demo)";

        private const string TopicToBazaar = "PSI_Bazaar_Text";
        private const string TopicToPython = "PSI_Python_Image";
        private const string TopicToMacaw = "PSI_Macaw_Text";
        private const string TopicToNVBG = "PSI_NVBG_Location";
        private const string TopicToVHText = "PSI_VHT_Text";
        private const string TopicFromPython = "Python_PSI_Location";
        private const string TopicFromBazaar = "Bazaar_PSI_Text";
        private const string TopicToAgent = "PSI_Agent_Text";
        private const string TopicFromSensor = "Sensor_PSI_Text";
        private const string TopicFaceOrientation = "face-orientation";
        private const string TopicAgentResp= "agent-response";
        private const string TopicSendAudioToPython = "audio-psi-to-python";
        private const string TopicRespChatGPT = "chatgpt-responses";

        private const string TopicCVPreds = "cv-preds";
        private const string TopicImages = "images";



        private const int SendingImageWidth = 360;
        private const int MaxSendingFrameRate = 15;
        private const string TcpIPResponder = "@tcp://*:40001";
        private const string TcpIPPublisher = "tcp://*:40002";
        private const string TcpIPPublisherSendAudio = "tcp://*:40003";



        private const double SocialDistance = 183;
        private const double DistanceWarningCooldown = 30.0;
        private const double NVBGCooldownLocation = 8.0;
        private const double NVBGCooldownAudio = 3.0;

        private static string AzureSubscriptionKey = "165ea78f5c7f44bd9d31f07d0f319cc7";
        // private static string AzureSubscriptionKey = "b6ba5313943f4393abaa37e28a45de51";
        private static string AzureRegion = "eastus";
        public static readonly object SendToBazaarLock = new object();
        public static readonly object SendToPythonLock = new object();
        public static readonly object LocationLock = new object();
        public static readonly object AudioSourceLock = new object();

        public static volatile bool AudioSourceFlag = true;

        public static DateTime LastLocSendTime = new DateTime();
        public static DateTime LastDistanceWarning = new DateTime();
        public static DateTime LastNVBGTime = new DateTime();

        public static List<IdentityInfo> IdInfoList;
        public static Dictionary<string, IdentityInfo> IdHead;
        public static Dictionary<string, IdentityInfo> IdTail;
        public static List<String> AudioSourceList;
        public static CameraInfo VhtInfo;
        // private readonly Merger<Message<string>, int> merger;

        public static String remoteIP;

        public static void Main(string[] args)
        {
            SetConsole();
            if (Initialize())    // TEMPORARY
                if (true)
                {
                    bool exit = false;
                    while (!exit)
                    {
                        Console.WriteLine("############################################################################");
                        Console.WriteLine("1) Respond to requests from remote device.");
                        ConsoleKey key = Console.ReadKey().Key;
                        Console.WriteLine();
                        switch (key)
                        {
                            case ConsoleKey.D1:
                                RunDemo();
                                break;
                                // case ConsoleKey.Q:
                                //     exit = true;
                                //     break;
                        }
                    }
                }
            // else
            // {
            //     Console.ReadLine();
            // }
        }

        private static void SetConsole()
        {
            Console.Title = AppName;
            Console.Write(@"                                                                                                    
                                                                                                                   
                                                                                                   ,]`             
                                                                                                 ,@@@              
            ]@@\                                                           ,/\]                ,@/=@*              
         ,@@[@@/                                           ,@@           ,@@[@@/.                 =\               
      .//`   [      ,`                 ,]]]]`             .@@^           @@`            ]]]]]     @^               
    .@@@@@\]]`    .@@`  /]   ]]      ,@/,@@^    /@@@,@@@@@@@@@@[`        @@           /@`\@@     ,@@@@@@@^         
             \@@` =@^ ,@@@`//@@^    .@^ =@@^     ,@@`     /@*           ,@^          =@*.@@@*    =@   ,@/          
             ,@@* =@,@` =@@` =@^  ` @@ //\@@  ,\ @@^     ,@^            /@          =@^,@[@@^ ./`=@. /@`           
    ,@^    ,/@[   =@@. ,@@`  ,@^//.=@\@` ,@@@@` .@@     .@@^  /@    ,@\]@`     ,@@/ @@//  \@@@/  @@]@`             
    ,\/@[[`      =@@`  \/`    [[`  =@/    ,@`   ,[`      @@@@/      [[@@@@@@@@@[`  .@@`    \/*  /@/`               
                  ,`                                                                           ,`                  
                                                                                                                   
                                                                                                                   
                                                                                                                 
");
            Console.WriteLine("############################################################################");
        }

        static bool Initialize()
        {

            return true;
        }

        // ...
        public void PrintByteArray(byte[] bytes)
        {
            var sb = new StringBuilder("new byte[] { ");
            foreach (var b in bytes)
            {
                sb.Append(b + ", ");
            }
            sb.Append("}");
            Console.WriteLine(sb.ToString());
        }
        public static void RunDemo()
        {
            String remoteIP;
            // String localIP = "tcp://127.0.0.1:40003";
            NanoIPs ips;

            using (var responseSocket = new ResponseSocket("@tcp://*:40001"))
            {
                var message = responseSocket.ReceiveFrameString();
                Console.WriteLine("RunDemoWithRemoteMultipart, responseSocket received '{0}'", message);
                responseSocket.SendFrame(message);
                ips = JsonConvert.DeserializeObject<NanoIPs>(message);
                remoteIP = ips.remoteIP;
                Console.WriteLine(ips);
                Console.WriteLine("RunDemoWithRemoteMultipart: remoteIP = '{0}'", remoteIP);
            }
            Thread.Sleep(1000);


            using (var p = Pipeline.Create())
            {
                // AUDIO SETUP
                var format = WaveFormat.Create16BitPcm(16000, 1);

                // binary data stream
                var audioFromNano = new NetMQSource<byte[]>(
                    p,
                    "temp",
                    ips.audio_channel,
                    MessagePackFormat.Instance);

                // DOA - Direction of Arrival (of sound, int values range from 0 to 360)
                var doaFromNano = new NetMQSource<int>(
                    p,
                    "temp2",
                    ips.doa,
                    MessagePackFormat.Instance);

                var vadFromNano = new NetMQSource<int>(
                    p,
                    "temp3",
                    ips.vad,
                    MessagePackFormat.Instance);

                var chatGPTResponse = new NetMQSource<string>(
                    p,
                    TopicRespChatGPT,
                    "tcp://127.0.0.1:50001",
                    MessagePackFormat.Instance);

                var cvPreds = new NetMQSource<float[]>(
                    p,
                    TopicCVPreds,
                    ips.cvPreds,
                    MessagePackFormat.Instance);

                var images = new NetMQSource<int[][]>(
                    p,
                    TopicImages,
                    ips.images,
                    MessagePackFormat.Instance);

                images.Do(resp=>{
                    Console.WriteLine("image", resp.Length);
                })

                cvPreds.Do(resp=>{
                    Console.WriteLine("[{0}]", string.Join(", ", resp));
                })
                    
                chatGPTResponse.Do(t=>{
                    Console.WriteLine(t);
                });

                // var saveToWavFile = new WaveFileWriter(p, "./psi_direct_audio_2.wav");

                // Create a publisher for messages to the agent using NetMQ (ZeroMQ)
                var nmqPubToAgent = new NetMQWriter<string>(p, TopicFaceOrientation, TcpIPPublisher, MessagePackFormat.Instance);
                chatGPTResponse.PipeTo(nmqPubToAgent);

                var nmqSendAudioToPythonBE = new NetMQWriter<byte[]>(p, TopicSendAudioToPython, TcpIPPublisherSendAudio, MessagePackFormat.Instance);
                audioFromNano.PipeTo(nmqSendAudioToPythonBE);

                p.Run();

            }
        }

        // Works sending to itself locally
        public static void RunDemoWithLocal()
        {
            using (var responseSocket = new ResponseSocket("@tcp://*:40001"))
            using (var requestSocket = new RequestSocket(">tcp://localhost:40001"))
            using (var p = Pipeline.Create())
                for (; ; )
                {
                    {
                        var mq = new NetMQSource<string>(p, "test-topic", "tcp://localhost:45678", JsonFormat.Instance);
                        Console.WriteLine("requestSocket : Sending 'Hello'");
                        requestSocket.SendFrame(">>>>> Hello from afar! <<<<<<");
                        var message = responseSocket.ReceiveFrameString();
                        Console.WriteLine("responseSocket : Server Received '{0}'", message);
                        Console.WriteLine("responseSocket Sending 'Hibackatcha!'");
                        responseSocket.SendFrame("Hibackatcha!");
                        message = requestSocket.ReceiveFrameString();
                        Console.WriteLine("requestSocket : Received '{0}'", message);
                        Console.ReadLine();
                        Thread.Sleep(1000);
                    }
                }
        }


        // This method tests sending & receiving over the same socket. 
        public static void RunDemoPubSubLocal()
        {
            string address = "tcp://127.0.0.1:40001";
            var pubSocket = new PublisherSocket();
            pubSocket.Options.SendHighWatermark = 1000;
            pubSocket.Bind(address);
            var subSocket = new SubscriberSocket();
            subSocket.Connect(address);
            Thread.Sleep(100);
            subSocket.SubscribeToAnyTopic();
            String received = "";

            // Testing send & receive over same socket
            for (; ; )
            {
                for (; ; )
                {
                    pubSocket.SendFrame("Howdy from NetMQ!", false);
                    Console.WriteLine("About to try subSocket.ReceiveFrameString");
                    received = subSocket.ReceiveFrameString();
                    if (received == "")
                    {
                        Console.WriteLine("Received nothing");
                        continue;
                    }
                    Console.WriteLine("Received something");
                    break;
                }
                Console.WriteLine(received);
                Thread.Sleep(2000);
            }
        }



        private static String getRandomName()
        {
            Random randomFunc = new Random();
            int randomNum = randomFunc.Next(0, 3);
            if (randomNum == 1)
                return "Haogang";
            else
                return "Yansen";
        }

        private static String getRandomLocation()
        {
            Random randomFunc = new Random();
            int randomNum = randomFunc.Next(0, 4);
            switch (randomNum)
            {
                case 0:
                    return "0:0:0";
                case 1:
                    return "75:100:0";
                case 2:
                    return "150:200:0";
                case 3:
                    return "225:300:0";
                default:
                    return "0:0:0";
            }
        }


        private static void Pipeline_PipelineCompleted(object sender, PipelineCompletedEventArgs e)
        {
            Console.WriteLine("Pipeline execution completed with {0} errors", e.Errors.Count);
        }

        private static void Pipeline_PipelineException(object sender, PipelineExceptionNotHandledEventArgs e)
        {
            Console.WriteLine(e.Exception);
        }

        private static bool GetSubscriptionKey()
        {
            Console.WriteLine("A cognitive services Azure Speech subscription key is required to use this. For more info, see 'https://docs.microsoft.com/en-us/azure/cognitive-services/cognitive-services-apis-create-account'");
            Console.Write("Enter subscription key");
            Console.Write(string.IsNullOrWhiteSpace(Program.AzureSubscriptionKey) ? ": " : string.Format(" (current = {0}): ", Program.AzureSubscriptionKey));

            // Read a new key or hit enter to keep using the current one (if any)
            string response = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(response))
            {
                Program.AzureSubscriptionKey = response;
            }

            Console.Write("Enter region");
            Console.Write(string.IsNullOrWhiteSpace(Program.AzureRegion) ? ": " : string.Format(" (current = {0}): ", Program.AzureRegion));

            // Read a new key or hit enter to keep using the current one (if any)
            response = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(response))
            {
                Program.AzureRegion = response;
            }

            return !string.IsNullOrWhiteSpace(Program.AzureSubscriptionKey) && !string.IsNullOrWhiteSpace(Program.AzureRegion);
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
