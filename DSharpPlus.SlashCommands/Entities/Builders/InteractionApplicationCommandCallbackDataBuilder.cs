using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DSharpPlus.Entities;

namespace DSharpPlus.SlashCommands.Entities.Builders
{
    public class InteractionApplicationCommandCallbackDataBuilder : IBuilder<InteractionApplicationCommandCallbackData>
    {
        public bool? TextToSpeech { get; set; }
        public string? Content { get; set; }
        public List<DiscordEmbed> Embeds { get; set; } = new();
        public List<IMention> AllowedMentions { get; set; } = new();

        public InteractionApplicationCommandCallbackDataBuilder()
        {

        }

        public InteractionApplicationCommandCallbackDataBuilder WithTTS()
        {
            TextToSpeech = true;
            return this;
        }

        public InteractionApplicationCommandCallbackDataBuilder WithContent(string content)
        {
            Content = content;
            return this;
        }

        public InteractionApplicationCommandCallbackDataBuilder WithEmbed(DiscordEmbed embed)
        {
            Embeds.Add(embed);
            return this;
        }

        public InteractionApplicationCommandCallbackDataBuilder WithAllowedMention(IMention mention)
        {
            AllowedMentions.Add(mention);
            return this;
        }

        public InteractionApplicationCommandCallbackData Build()
        {
            if (Embeds.Count <= 0 && (Content is null || Content == ""))
                throw new Exception("Either an embed or content is required.");

            return new InteractionApplicationCommandCallbackData()
            {
                AllowedMentions = AllowedMentions.Count > 0 ? AllowedMentions : null,
                Embeds = Embeds.Count > 0 ? Embeds.ToArray() : null,
                Content = Content,
                TextToSpeech = TextToSpeech
            };
        }
    }
}
