using System.Text.Json;
using System.Text.Json.Serialization;

namespace AccountingBot.Models
{
    public class MessageOutput
    {
        public string Type { get; set; }

        public GroupSender Sender { get; set; }

        public List<TextMessage> MessageChain { get; set; }

        public long MessageId { get; set; }

        public Group Group { get; set; }
    }

    public partial class GroupSender
    {
        public long Id { get; set; }

        public string MemberName { get; set; }

        public string SpecialTitle { get; set; }

        public string Permission { get; set; }

        public long JoinTimestamp { get; set; }

        public long LastSpeakTimestamp { get; set; }

        public long MuteTimeRemaining { get; set; }

        public Group Group { get; set; }
    }

    public partial class Group
    {
        public long Id { get; set; }

        public string Name { get; set; }

        public string Permission { get; set; }
    }

    public class TextMessage
    {
        [JsonPropertyName("id")]
        public long Id { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("text")]
        public string Text { get; set; }
    }
}
