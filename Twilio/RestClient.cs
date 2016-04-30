using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Twilio;
using TwilioFetchBot.Tokens;
using System.Configuration;
using System.Timers;

namespace TwilioFetchBot
{
    public interface IRestClient
    {
        Message SendMessage(string from, string to, string body, params string[] mediaUrl);
        MessageResult GetAllMessages();
        Message GetLatestInboundMessage();
        List<Message> GetAllInboundMessages();
        Message GetLastSentMessage();
        List<Message> GetAllSentMessages();
        List<Message> FindMessagesByQuery(MessageResult messageResult, string searchQuery);

    }

    public class RestClient : IRestClient
    {
        public Message LatestMessage { get; set; }

        public readonly TwilioRestClient Client;
        private readonly TwilioCredentials credentials;

        public RestClient()
        {
                credentials = new TwilioCredentials();

            Client = new TwilioRestClient(credentials.AccountSid, credentials.AuthToken);
        }

        public RestClient(TwilioRestClient client)
        {
            Client = client;
        }

        public Message SendMessage(string from, string to, string body, params string[] mediaUrl)
        {
            return Client.SendMessage(from, to, body, mediaUrl);
        }

        public MessageResult GetAllMessages()
        {
            var messageList = new MessageListRequest();
            return Client.ListMessages(messageList);
        }

        public Message GetLatestInboundMessage()
        {
            var messages = GetAllMessages();
            return messages.Messages.Where(x => x.Direction == "inbound").FirstOrDefault();
        }

        public List<Message> GetAllInboundMessages()
        {
            List<Message> inboundMessages = new List<Message>();
            var messages = GetAllMessages();
            var messageResult = messages.Messages.Where(x => x.Direction == "inbound");

            messageResult.ToList().ForEach(x => inboundMessages.Add(x));
            return inboundMessages;

        }

        public Message GetLastSentMessage()
        {
            var messages = GetAllMessages();
            return messages.Messages.Where(x => x.Direction == "outbound-api").FirstOrDefault();
        }

        public List<Message> GetAllSentMessages()
        {
            List<Message> outboundMessages = new List<Message>();
            var messages = GetAllMessages();
            var messageResult = messages.Messages.Where(x => x.Direction == "outbound-api");

            messageResult.ToList().ForEach(x => outboundMessages.Add(x));
            return outboundMessages;
        }

        public List<Message> FindMessagesByQuery(MessageResult messageResult, string searchQuery)
        {
            List<Message> inboundFilteredMessages = new List<Message>();

            foreach (var message in messageResult.Messages)
            {
                if (Regex.IsMatch(message.Body, @"\b" + searchQuery + @"\b", RegexOptions.IgnoreCase))
                {
                    inboundFilteredMessages.Add(message);
                }
            }

            return inboundFilteredMessages;
        }
        public void DeleteAllInboundMessages()
        {
            var getAllInbound = GetAllInboundMessages();
            List<string> ids = new List<string>();

            foreach (var message in getAllInbound)
            {
                ids.Add(message.Sid);
            }
            foreach (var id in ids)
            {
                Client.RedactMessage(id);
            }
        }

    }
}
