// This application entry point is based on ASP.NET Core new project templates and is included
// as a starting point for app host configuration.
// This file may need to be updated according to the specific scenario of the application being upgraded.
// For more information on ASP.NET Core hosting, see https://docs.microsoft.com/aspnet/core/fundamentals/host/web-host

using CMU.Smartlab.Communication;
using CMU.Smartlab.Identity;
using CMU.Smartlab.Rtsp;
using CMU.Smartlab.Audio; 
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
using Microsoft.CognitiveServices.Speech; 
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
        // private static string AzureSubscriptionKey;
        // private static string AzureRegion;
        private static string AzureRegion = "eastus";
        private static string AzureSubscriptionKey = "09186ea05c6a4b6eb4e33971f1976172";
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
        // public String remoteIP = "tcp://128.2.220.118:40003"; 
        // private String speechText; 

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
                    Console.WriteLine("2) Mobile device with speech recognition."); 
                    Console.WriteLine("3) Wall camera."); 
                    Console.WriteLine("4) Webcam."); 
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
                        case ConsoleKey.D3:
                            useAudio = true; 
                            useAzure = true; 
                            RunDemo("Wall_Camera");
                            // RunDemo();
                            break;
                        case ConsoleKey.D4:
                            useAudio = true; 
                            useAzure = true; 
                            RunDemo("Webcam");
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
        public static async void RunDemo(string inputType)
        {
            // String remoteIP; 
            String remoteIP = "tcp://128.2.220.118:40003";
            // String localIP = "tcp://127.0.0.1:40003";

            if (useAzure && inputType != "Webcam") {
                if (!GetAzureSubscriptionKey()) {
                    Console.WriteLine("Azure subscription key and region are required. Ending.");
                    return; 
                }
            }

            if (inputType == "Nano_Audio") {
                using (var responseSocket = new ResponseSocket("@tcp://*:40001")) {
                    Console.WriteLine("RunDemo: Waiting for request message ...");
                    var message = responseSocket.ReceiveFrameString();
                    Console.WriteLine("RunDemoWithRemoteMultipart, responseSocket received '{0}'", message);
                    responseSocket.SendFrame(message);
                    remoteIP = message; 
                    Console.WriteLine("RunDemoWithRemoteMultipart: remoteIP = '{0}'", remoteIP);
                }
                Thread.Sleep(1000); 
            } 

            RtspCapture rtspPSI = null; 

            using (var pipeline = Pipeline.Create())
            {
                if (useAudio && useAzure) {

                    if (inputType == "Nano_Audio") {
                        // Hardcoded for now as Lorex camera on back wall of SOS
                        var serverUriPSI = new Uri("rtsp://lorex5416b1.pc.cs.cmu.edu");
                        var credentialsPSI = new NetworkCredential("admin", "54Lorex16");
                        // RtspCapture rtspPSI = new RtspCapture(pipeline, serverUriPSI, credentialsPSI, true);
                        rtspPSI = new RtspCapture(pipeline, serverUriPSI, credentialsPSI, true);
                        // EncodedImageSendHelper helper = new EncodedImageSendHelper(manager, "webcam", Program.TopicToPython, Program.SendToPythonLock, Program.MaxSendingFrameRate);
                        // var scaled = rtspPSI.Out.Resize((float)Program.SendingImageWidth, Program.SendingImageWidth / 1280.0f * 720.0f);
                        // var encoded = scaled.EncodeJpeg(90, DeliveryPolicy.LatestMessage).Out;
                        // encoded.Do(helper.SendImage);

                    } else if (inputType == "Webcam") {
                        await RecognizeSpeechFromMicrophone();
                        // Console.WriteLine("Please press <Return> to continue.");
                        // Console.ReadLine();
                        // MediaCapture webcam = new MediaCapture(pipeline, 1280, 720, 30);
                        // deviceId = "0x10000050d"
                        // MediaCapture webcam = new MediaCapture(pipeline, 1280, 720);
                        // MediaCapture webcam = new MediaCapture(pipeline, 1280, 720, "avcapture://0x8020000005ac8514");
                        // MediaCapture webcam = new MediaCapture(pipeline, 1280, 720, "../../../Introduction.mp3");

                    } else if (inputType == "Wall_Camera") {
                        // Hardcoded for now as Lorex camera on back wall of SOS
                        var serverUriPSI = new Uri("rtsp://lorex5416b1.pc.cs.cmu.edu");
                        var credentialsPSI = new NetworkCredential("admin", "54Lorex16");
                        Console.WriteLine("About to create new RtspCapture"); 
                        // RtspCapture rtspPSI = new RtspCapture(pipeline, serverUriPSI, credentialsPSI, true);
                        rtspPSI = new RtspCapture(pipeline, serverUriPSI, credentialsPSI, true);
                        Console.WriteLine("Creation of new RtspCapture complete"); 

                        // EncodedImageSendHelper helper = new EncodedImageSendHelper(manager, "webcam", Program.TopicToPython, Program.SendToPythonLock, Program.MaxSendingFrameRate);
                        // var scaled = rtspPSI.Out.Resize((float)Program.SendingImageWidth, Program.SendingImageWidth / 1280.0f * 720.0f);
                        // var encoded = scaled.EncodeJpeg(90, DeliveryPolicy.LatestMessage).Out;
                        // encoded.Do(helper.SendImage);
                    }


                    // var audioConfig = new AudioCaptureConfiguration()
                    // {
                    //     // OutputFormat = WaveFormat.Create16kHz1Channel16BitPcm(),
                    //     // DropOutOfOrderPackets = true
                    // };
                    // Console.WriteLine("About to create new AudioCapture"); 
                    // IProducer<AudioBuffer> audio = new AudioCapture(pipeline, audioConfig);
                    // Console.WriteLine("Creation of new AudioCapture complete"); 



                    // ==================== SIMPLE VOICE ACTIVITY DETECTOR ====================
                    // var vad = new SystemVoiceActivityDetector(pipeline);
                    // audio.PipeTo(vad);
                    Console.WriteLine("About to create new AcousticFeaturesExtractor"); 
                    var acousticFeaturesExtractor = new AcousticFeaturesExtractor(pipeline);
                    Console.WriteLine("Creation of new AcousticFeaturesExtractor complete"); 

                    // rtspPSI.Audio.PipeTo(acousticFeaturesExtractor.In); 

                    // // Display the log energy
                    // acousticFeaturesExtractor.LogEnergy
                    //     .Sample(TimeSpan.FromSeconds(0.2))
                    //     .Do(logEnergy => Console.WriteLine($"LogEnergy = {logEnergy}"));

                    // // Create a voice-activity stream by thresholding the log energy
                    // var vad = acousticFeaturesExtractor.LogEnergy
                    //     .Select(l => l > 7);

                    // var rtspPSISpeech = rtspPSI.Where(vad.LogEnergy > 7);
                    // var rtspPSISpeech = rtspPSI.Where(vad);

                    // Create filtered signal by aggregating over historical buffers
                    // var vadWithHistory = acousticFeaturesExtractor.LogEnergy
                    //     .Window(RelativeTimeInterval.Future(TimeSpan.FromMilliseconds(300)))
                    //     .Aggregate(false, (previous, buffer) => (!previous && buffer.All(v => v > 7)) || (previous && !buffer.All(v => v < 7)));

                    // Write the microphone output, VAD streams, and some acoustic features to the store
                    // var store = PsiStore.Create(pipeline, "SimpleVAD", Path.Combine(Directory.GetCurrentDirectory(), "Stores"));
                    // // microphone.Write("Audio", store);
                    // vad.Write("VAD", store);
                    // vadWithHistory.Write("VADFiltered", store);
                    // acousticFeaturesExtractor.LogEnergy.Write("LogEnergy", store);
                    // acousticFeaturesExtractor.ZeroCrossingRate.Write("ZeroCrossingRate", store);
                    // ==================== SIMPLE VOICE ACTIVITY DETECTOR ====================


                    // rtspPSI.Audio.PipeTo(vad);
                    // rtspPSI.Audio.PipeTo(vad.In); 
                    // rtspPSI.pipeTo(vad); 

                    // if (useAzure) {
                        // var recognizer = new AzureSpeechRecognizer(pipeline, new AzureSpeechRecognizerConfiguration()
                        // {
                        //     SubscriptionKey = Program.AzureSubscriptionKey,
                        //     Region = Program.AzureRegion
                        // });
                        var recognizer = new ContinuousSpeechRecognizer(pipeline, AzureSubscriptionKey, AzureRegion);
                    // }



                    // } 
                    // var annotatedAudio = audio.Join(vad);
                    // var annotatedAudio = rtspPSI.Join(vad);
                    // annotatedAudio.PipeTo(recognizer);

                    // vad.PipeTo(recognizer); 
                    // rtspPSISpeech.PipeTo(recognizer); 
                    rtspPSI.Audio.PipeTo(recognizer); 

                    recognizer.Out.Do((result, e) => Console.WriteLine($"{e.OriginatingTime.TimeOfDay}: {result}"));

                    // var finalResults = recognizer.Out.Where(result => result.IsFinal);
                    // finalResults.Do(printSpeechResult);
                    // finalResults.Do(Console.WriteLine("Speech: '{0}'", finalResults.text));

                }


                // Subscribe to messages from remote sensor using NetMQ (ZeroMQ)
                var nmqSubFromSensor = new NetMQSubscriber<IDictionary<string,object>>(pipeline, "", remoteIP, MessagePackFormat.Instance, true, "Sensor to PSI");

                // Create a publisher for messages from the sensor to Bazaar
                var amqPubSensorToBazaar = new AMQPublisher<IDictionary<string,object>>(pipeline, TopicFromSensor, TopicToBazaar, "Sensor to Bazaar"); 

                // Subscribe to messages from Bazaar for the agent
                var amqSubBazaarToAgent = new AMQSubscriber<IDictionary<string,object>>(pipeline, TopicFromBazaar, TopicToAgent, "Bazaar to Agent"); 

                // Create a publisher for messages to the agent using NetMQ (ZeroMQ)
                var nmqPubToAgent = new NetMQPublisher<IDictionary<string,object>>(pipeline, TopicFaceOrientation, TcpIPPublisher, MessagePackFormat.Instance);

                // Route messages from remote sensor to Bazaar
                nmqSubFromSensor.PipeTo(amqPubSensorToBazaar.IDictionaryIn); 

                // Create a merge component to combine messages
                //   1. direct from sensor
                //   2. from Bazaar
                SmartlabMerge<IDictionary<string,object>> mergeToAgent = new SmartlabMerge<IDictionary<string,object>>(pipeline,"Merge to Agent"); 
                var receiverSensor = mergeToAgent.AddInput("Sensor to PSI"); 
                var receiverBazaar = mergeToAgent.AddInput("Bazaar to Agent"); 
                nmqSubFromSensor.PipeTo(receiverSensor); 
                amqSubBazaarToAgent.PipeTo(receiverBazaar);

                // Send the merged message types to the agent 
                mergeToAgent.PipeTo(nmqPubToAgent); 

                pipeline.RunAsync();

                Console.WriteLine("Press any key to exit...");
                Console.ReadKey(true); 
            }
        }
        private static void printSpeechResult(IStreamingSpeechRecognitionResult result, Envelope envelope)
        {
            String speech = result.Text;
            Console.WriteLine("printSpeechResult, speech: '{0}'", speech);
        }


static async Task RecognizeSpeechFromMicrophone() {
    await RecognizeSpeechAsyncOnce();
    Console.WriteLine("Please press <Return> to continue.");
    Console.ReadLine();
}


public static async Task RecognizeSpeechAsyncOnce() {
    var config = SpeechConfig.FromSubscription("09186ea05c6a4b6eb4e33971f1976172", "eastus");
    using (var speechRecognizer = new SpeechRecognizer(config)) {
        Console.WriteLine("Say something...");
        var result = await speechRecognizer.RecognizeOnceAsync();
        Console.WriteLine("Waiting ...");
        // Checks result.
        if (result.Reason == ResultReason.RecognizedSpeech)
        {
            Console.WriteLine($"We recognized: {result.Text}");
        }
        else if (result.Reason == ResultReason.NoMatch)
        {
            Console.WriteLine($"NOMATCH: Speech could not be recognized.");
        }
        else if (result.Reason == ResultReason.Canceled)
        {
            var cancellation = CancellationDetails.FromResult(result);
            Console.WriteLine($"CANCELED: Reason={cancellation.Reason}");

            if (cancellation.Reason == CancellationReason.Error)
            {
                Console.WriteLine($"CANCELED: ErrorCode={cancellation.ErrorCode}");
                Console.WriteLine($"CANCELED: ErrorDetails={cancellation.ErrorDetails}");
                Console.WriteLine($"CANCELED: Did you update the subscription info?");
            }
        }
    }
}
public static async Task RecognizeSpeechAsyncContinuous() {
    var config = SpeechConfig.FromSubscription("09186ea05c6a4b6eb4e33971f1976172", "eastus");
    using (var speechRecognizer = new SpeechRecognizer(config)) {
        Console.WriteLine("Say something...");
        // var result = await speechRecognizer.RecognizeOnceAsync();
        await speechRecognizer.StartContinuousRecognitionAsync();
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
