// This application entry point is based on ASP.NET Core new project templates and is included
// as a starting point for app host configuration.
// This file may need to be updated according to the specific scenario of the application being upgraded.
// For more information on ASP.NET Core hosting, see https://docs.microsoft.com/aspnet/core/fundamentals/host/web-host

using CMU.Smartlab.Communication;
using CMU.Smartlab.Identity;
using CMU.Smartlab.Rtsp;
using System;
using System.IO;
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
// using NetMQSource;
// using ZeroMQ; 
// using Operators; 


namespace SigdialDemo
{
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

        private const int SendingImageWidth = 360;
        private const int MaxSendingFrameRate = 15;
        private const string TcpIPResponder = "@tcp://*:40001";
        // private const string TcpIPPublisher = "tcp://*:40002";
        private const string TcpIPPublisher = "tcp://*:30002";
        // private const string TcpIPPublisher = "tcp://*:5500";
        

        private const double SocialDistance = 183;
        private const double DistanceWarningCooldown = 30.0;
        private const double NVBGCooldownLocation = 8.0;
        private const double NVBGCooldownAudio = 3.0;
        private static Boolean useAudio = false; 
        private static Boolean useAzure = false; 
        private static string AzureSubscriptionKey;
        private static string AzureRegion;
        // private static string AzureRegion = "eastus";
        // private static string AzureSubscriptionKey = "abee363f8d89444998c5f35b6365ca38";
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
            if (Initialize())    // TEMPORARY
            // if (true)
            {
                bool exit = false;
                while (!exit)
                {
                    SetConsole();
                    Console.WriteLine("############################################################################");
                    Console.WriteLine("1) Mobile device."); 
                    Console.WriteLine("2) Mobile device with Azure speech recognition."); 
                    Console.WriteLine("Q) Quit.");
                    Console.WriteLine("Press any key to exit."); 
                    ConsoleKey key = Console.ReadKey().Key;
                    Console.WriteLine();
                    switch (key)
                    {
                        case ConsoleKey.D1:
                            RunDemo("Nano_Text");
                            break;
                        case ConsoleKey.D2:
                            useAudio = true; 
                            useAzure = true; 
                            RunDemo("Nano_Audio");
                            // RunDemo();
                            break;
                        case ConsoleKey.Q:
                            exit = true;
                            break;
                    }
                    exit = true;   // TEMPORARY for one loop only
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
        public static void RunDemo(string inputType)
        {
            String remoteIP; 
            // String localIP = "tcp://127.0.0.1:40003";

            if (useAzure) {
                if (!GetAzureSubscriptionKey()) {
                    Console.WriteLine("Azure subscription key and region are required. Ending.");
                    return; 
                }
            }

            using (var responseSocket = new ResponseSocket("@tcp://*:40001")) {
                var message = responseSocket.ReceiveFrameString();
                Console.WriteLine("RunDemoWithRemoteMultipart, responseSocket received '{0}'", message);
                responseSocket.SendFrame(message);
                remoteIP = message; 
                Console.WriteLine("RunDemoWithRemoteMultipart: remoteIP = '{0}'", remoteIP);
            }
            Thread.Sleep(1000); 

            using (var pipeline = Pipeline.Create())
            {
                if (useAudio) {

                    var audioConfig = new AudioCaptureConfiguration()
                    {
                        // OutputFormat = WaveFormat.Create16kHz1Channel16BitPcm(),
                        // DropOutOfOrderPackets = true
                    };
                    IProducer<AudioBuffer> audio = new AudioCapture(pipeline, audioConfig);

                    // var vad = new SystemVoiceActivityDetector(pipeline);
                    // audio.PipeTo(vad);

                    if (useAzure) {
                        var recognizer = new AzureSpeechRecognizer(pipeline, new AzureSpeechRecognizerConfiguration()
                        {
                            SubscriptionKey = Program.AzureSubscriptionKey,
                            Region = Program.AzureRegion
                        });
                    }
                    // var annotatedAudio = audio.Join(vad);
                    // annotatedAudio.PipeTo(recognizer);

                    // var finalResults = recognizer.Out.Where(result => result.IsFinal);
                    // finalResults.Do(SendDialogToBazaar);
                    // finalResults.Do(Console.WriteLine("Speech: '{0}'", finalResults.text));


                    var acousticFeaturesExtractor = new AcousticFeaturesExtractor(pipeline);

                    // Display the log energy
                    acousticFeaturesExtractor.LogEnergy
                        .Sample(TimeSpan.FromSeconds(0.2))
                        .Do(logEnergy => Console.WriteLine($"LogEnergy = {logEnergy}"));

                    // Create a voice-activity stream by thresholding the log energy
                    var vad = acousticFeaturesExtractor.LogEnergy
                        .Select(l => l > 7);

                    // Create filtered signal by aggregating over historical buffers
                    var vadWithHistory = acousticFeaturesExtractor.LogEnergy
                        .Window(RelativeTimeInterval.Future(TimeSpan.FromMilliseconds(300)))
                        .Aggregate(false, (previous, buffer) => (!previous && buffer.All(v => v > 7)) || (previous && !buffer.All(v => v < 7)));

                    // Write the microphone output, VAD streams, and some acoustic features to the store
                    var store = PsiStore.Create(pipeline, "SimpleVAD", Path.Combine(Directory.GetCurrentDirectory(), "Stores"));
                    // microphone.Write("Audio", store);
                    vad.Write("VAD", store);
                    vadWithHistory.Write("VADFiltered", store);
                    acousticFeaturesExtractor.LogEnergy.Write("LogEnergy", store);
                    acousticFeaturesExtractor.ZeroCrossingRate.Write("ZeroCrossingRate", store);

                }

                if (inputType == "Nano_Audio") {

                    // Hardcoded for now as Lorex camera on back wall of SOS
                    var serverUriPSIb = new Uri("rtsp://lorex5416b1.pc.cs.cmu.edu");
                    var credentialsPSIb = new NetworkCredential("admin", "54Lorex16");
                    RtspCapture rtspPSIb = new RtspCapture(pipeline, serverUriPSIb, credentialsPSIb, true);
                    // EncodedImageSendHelper helper = new EncodedImageSendHelper(manager, "webcam", Program.TopicToPython, Program.SendToPythonLock, Program.MaxSendingFrameRate);
                    // var scaled = rtspPSIb.Out.Resize((float)Program.SendingImageWidth, Program.SendingImageWidth / 1280.0f * 720.0f);
                    // var encoded = scaled.EncodeJpeg(90, DeliveryPolicy.LatestMessage).Out;
                    // encoded.Do(helper.SendImage);
                }

                // Subscribe to messages from remote sensor using NetMQ (ZeroMQ)
                // var nmqSubFromSensor = new NetMQSubscriber<string>(pipeline, "", remoteIP, MessagePackFormat.Instance, useSourceOriginatingTimes = true, name="Sensor to PSI");
                // var nmqSubFromSensor = new NetMQSubscriber<string>(pipeline, "", remoteIP, JsonFormat.Instance, true, "Sensor to PSI");
                var nmqSubFromSensor = new NetMQSubscriber<IDictionary<string,object>>(pipeline, "", remoteIP, MessagePackFormat.Instance, true, "Sensor to PSI");

                // Create a publisher for messages from the sensor to Bazaar
                var amqPubSensorToBazaar = new AMQPublisher<IDictionary<string,object>>(pipeline, TopicFromSensor, TopicToBazaar, "Sensor to Bazaar"); 

                // Subscribe to messages from Bazaar for the agent
                var amqSubBazaarToAgent = new AMQSubscriber<IDictionary<string,object>>(pipeline, TopicFromBazaar, TopicToAgent, "Bazaar to Agent"); 

                // Create a publisher for messages to the agent using NetMQ (ZeroMQ)
                var nmqPubToAgent = new NetMQPublisher<IDictionary<string,object>>(pipeline, TopicFaceOrientation, TcpIPPublisher, MessagePackFormat.Instance);
                // nmqPubToAgent.Do(x => Console.WriteLine("RunDemoWithRemoteMultipart, nmqPubToAgent.Do: {0}", x));

                // Route messages from the sensor to Bazaar
                nmqSubFromSensor.PipeTo(amqPubSensorToBazaar.IDictionaryIn); 

                // Combine messages (1) direct from sensor, and (2) from Bazaar, and send to agent
                SmartlabMerge<IDictionary<string,object>> mergeToAgent = new SmartlabMerge<IDictionary<string,object>>(pipeline,"Merge to Agent"); 
                var receiverSensor = mergeToAgent.AddInput("Sensor to PSI"); 
                var receiverBazaar = mergeToAgent.AddInput("Bazaar to Agent"); 
                nmqSubFromSensor.PipeTo(receiverSensor); 
                amqSubBazaarToAgent.PipeTo(receiverBazaar);
                // mergeToAgent.Select(m => m.Data).PipeTo(nmqPubToAgent); 
                mergeToAgent.PipeTo(nmqPubToAgent); 

                pipeline.RunAsync();

                Console.WriteLine("Press any key to exit...");
                Console.ReadKey(true); 
            }
        }

        // Works sending to itself locally
        public static void RunDemoWithLocal()
        {
            using (var responseSocket = new ResponseSocket("@tcp://*:40001"))
            using (var requestSocket = new RequestSocket(">tcp://localhost:40001"))
            using (var p = Pipeline.Create())
            for (;;) 
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
            for (;;) {
                for (;;) {
                    pubSocket.SendFrame( "Howdy from NetMQ!", false );
                    Console.WriteLine( "About to try subSocket.ReceiveFrameString");
                    received = subSocket.ReceiveFrameString(); 
                    if  (received == "") {
                        Console.WriteLine( "Received nothing");
                        continue; 
                    }
                    Console.WriteLine( "Received something");
                    break; 
                }
                Console.WriteLine( received );
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

        private static bool GetAzureSubscriptionKey()
        {
            // Console.WriteLine("A cognitive services Azure Speech subscription key is required. For more info, see 'https://docs.microsoft.com/en-us/azure/cognitive-services/cognitive-services-apis-create-account'");
            Console.Write("Enter Azure subscription key: ");
            Console.Write(string.IsNullOrWhiteSpace(Program.AzureSubscriptionKey) ? ": " : string.Format(" (current = {0}): ", Program.AzureSubscriptionKey));

            // Read a new key or hit enter to keep using the current one (if any)
            string response = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(response))
            {
                Program.AzureSubscriptionKey = response;
            }

            Console.Write("Enter Azure region (e.g., 'eastus'): ");
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
