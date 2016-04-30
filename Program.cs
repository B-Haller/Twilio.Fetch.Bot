﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using log4net;
using TwilioFetchBot.ImageGetter;
using static TwilioFetchBot.ImageGetter.ImgurImageGetter;
using System.Threading;

namespace TwilioFetchBot
{
    class Program
    {
        private static bool isWaiting { get; set; }
        private static int Attempts { get; set; }


        private static ILog log = LogManager.GetLogger(typeof(Program).FullName);
        private static readonly object _locker = new object();

        static void Main(string[] args)
        {
            log.Info("Warming cold servers. And building client");
            RestClient client = new RestClient();
            log.Info("Deleting all those dirty inbound messages");
            client.DeleteAllInboundMessages();
            client.SendMessage(GetFrom(), GetTo(), "Twilio Fetch Bot Now Running!");
            log.Info("Deleted");
            Attempts = 0;
            isWaiting = true;
            var timer1 = CreateTimer(CheckResponses, 120000);
            log.Info("Timer intiated. Ticker Tocker.");
            lock (_locker)
            {
                while (isWaiting)
                {
                    Monitor.Wait(_locker);
                }
            }
            timer1.Stop();

            //RestClient client = new RestClient();
            //var getAllInbound = client.GetAllInboundMessages();
            //var getAllMessages = client.GetAllMessages();
            //var getLatest = client.GetLatestInboundMessage();
            //var getLatestSent = client.GetLastSentMessage();
            //var getAllSent = client.GetAllSentMessages();
            //var findInMessage = client.FindMessagesByQuery(getAllMessages, "pop");

            //RunProgram(getTopViewedImage.link);
        }

        static string GetFrom()
        {
            var from = "+15186852160";
            return from;
        }

        static string GetTo()
        {
            return "+15182221651";
        }

        static string GetMessage()
        {
            return "Your requested Image. Please enjoy!";
        }

        static string StringValidation(string stringToBeValidated, string typeOf)
        {
            if (string.IsNullOrEmpty(stringToBeValidated))
            {
                Console.Clear();
                Console.WriteLine($"Please Enter {typeOf}:");
                return stringToBeValidated = Console.ReadLine();
            }
            return stringToBeValidated;
        }

        static string[] GetSearchQueries()
        {

            Console.WriteLine("Please enter image search Criteria:");
            var searchCriteria = Console.ReadLine();
            string[] term = searchCriteria.Split(',');
            return term;
        }

        static string[] GetRandomImageResult(List<string> urls)
        {
            var rng = new Random();
            var ndx = rng.Next(0, urls.Count);
            string[] mediaURL = { urls[ndx] };
            return mediaURL;
        }

        static void Results(string status, string messageid)
        {
            Console.Clear();
            Console.WriteLine($"The message was sent with the following status: {status}");
            Console.WriteLine($"The message has the following ID: {messageid}");
            Console.ReadKey();

        }

        static void RunProgram(string mediaUrl)
        {
            var useAgain = true;

            while (useAgain)
            {
                //var searchCriteria = GetSearchQueries();
                //var googleImages = new GoogleImageGetter(searchCriteria);
                //string[] mediaUrl = GetRandomImageResult(googleImages.Urls);
                RestClient client = new RestClient();
                var message = client.SendMessage(GetFrom(), GetTo(), GetMessage(), mediaUrl);
                Results(message.Status, message.Sid);
                Console.Clear();
                Console.WriteLine("Send a new message? 1: Yes 2: No");
                var response = int.Parse((Console.ReadKey().KeyChar.ToString()));
                if (response == 2)
                {
                    useAgain = false;
                }
            }

        }

        private static void RunAutoProgram()
        {

            RestClient client = new RestClient();
            var searchQuery = client.GetLatestInboundMessage();
            if (searchQuery == null)
            {
                var imgur = new ImgurImageGetter(SearchType.Meme, null);
                var message = client.SendMessage(GetFrom(), GetTo(), "No Response means you get a meme!", imgur.GalleryImagesList.OrderBy(x => x.vote).FirstOrDefault().link);
            }
            if (!searchQuery.Body.Contains("$") && searchQuery != null)
            {
                var imgur = new ImgurImageGetter(SearchType.Meme, null);
                var message = client.SendMessage(GetFrom(), GetTo(), GetMessage(), imgur.GalleryImagesList.OrderBy(x => x.vote).FirstOrDefault().link);
            }
            else
            {
                List<string> searchParametersList = searchQuery.Body.Split(new char[] { ',', '.', '$', '/', ' ' }, StringSplitOptions.RemoveEmptyEntries).ToList();
                var searchCriteria = searchParametersList?[0];
                var searchType = searchParametersList?[1];
                SearchType searchTypeEnum = (SearchType)Enum.Parse(typeof(SearchType), searchType);
                var imgur = new ImgurImageGetter(searchTypeEnum, searchCriteria);
                var message = client.SendMessage(GetFrom(), GetTo(), GetMessage(), imgur.GalleryImagesList.OrderBy(x => x.datetime).FirstOrDefault().link);
            }

        }

        private static void CheckResponses(object source, ElapsedEventArgs e)
        {
            RestClient client = new RestClient();
            var latestMessage = client.GetLatestInboundMessage();

            if (latestMessage == null || string.IsNullOrEmpty(latestMessage.Body))
            {
                Console.WriteLine($"No message recieved {DateTime.Now}.");
                log.Info($"No message recieved {DateTime.Now}.");
                Attempts += 1;
            }
            else if (latestMessage.Body.Contains("$"))
            {
                Console.WriteLine($"Message recieved at {DateTime.Now}.");
                log.Info($"Message recieved at {DateTime.Now}.");
                RunAutoProgram();
                lock(_locker)
                {
                    isWaiting = false;
                    Monitor.Pulse(_locker);
                }
            }
            else if (latestMessage.Body.Contains("#"))
            {
                Console.WriteLine($"Message recieved without recognized command at {DateTime.Now}.");
                log.Info($"Message recieved without recognized command at {DateTime.Now}.");
                Attempts += 1;
                client.SendMessage(GetFrom(), GetTo(), @"To issue a command, please use '$' to initiate. 
                Please enter a search term, or in the case of a subreddit image search, a valid subreddit, followed by a comma.
                Next, please use the numbers 1, 2, or 3 to denote a Subreddit Image Search, Meme, or Galery Search.");
                client.DeleteAllInboundMessages();
            }
            else
            {
                Console.WriteLine($"Message recieved without recognized command at {DateTime.Now}.");
                log.Info($"Message recieved without recognized command at {DateTime.Now}.");
                Attempts += 1;
                client.SendMessage(GetFrom(), GetTo(), "Use $ to signal a command. Or # for instructions.");
                client.DeleteAllInboundMessages();
                Attempts += 1;
            }
            if(Attempts > 4)
            {
                Console.WriteLine($"Maximum attempts reached at {DateTime.Now}.");
                log.Info($"Maximum attempts reached at {DateTime.Now}.");
                RunAutoProgram();
                lock (_locker)
                {
                    isWaiting = false;
                    Monitor.Pulse(_locker);
                }
            }

        }

        private static System.Timers.Timer CreateTimer(Action<object, ElapsedEventArgs> method, int interval)
        {
            System.Timers.Timer timer1 = new System.Timers.Timer();
            timer1.Interval = interval;
            timer1.Elapsed += new ElapsedEventHandler(method);
            timer1.Start();
            timer1.AutoReset = true;
            return timer1;
        }

    }
}
