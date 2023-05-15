using SlackNet.Events;
using SlackNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SlackNet.WebApi;

namespace IfpaSlackBot.Handlers
{
    public class PingHandler : IEventHandler<MessageEvent>
    {
        private const string Trigger = "ping";

        private readonly ISlackApiClient _slack;
        public PingHandler(ISlackApiClient slack) => _slack = slack;

        public async Task Handle(MessageEvent slackEvent)
        {
            if (slackEvent.Text?.Equals(Trigger, StringComparison.OrdinalIgnoreCase) == true)
                await _slack.Chat.PostMessage(new Message
                {
                    Text = "pong",
                    Channel = slackEvent.Channel
                });
        }
    }
}
