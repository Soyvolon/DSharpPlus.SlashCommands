using System;

namespace DSharpPlus.SlashCommands.Entities.Builders
{
    public class ApplicationCommandOptionChoiceBuilder
    {
        public string Name { get; set; }

        /// <summary>
        /// Must be string or int
        /// </summary>
        public object Value { get; set; }

        public ApplicationCommandOptionChoiceBuilder()
        {
            Name = "";
            Value = null;
        }

        public ApplicationCommandOptionChoiceBuilder WithName(string name)
        {
            if (name is null || name == "")
                throw new Exception("Name can not be null");

            if (name.Length < 1 || name.Length > 100)
                throw new Exception("Name must be between 1 and 100 characters.");

            Name = name;
            return this;
        }

        public ApplicationCommandOptionChoiceBuilder WithValue(int value)
        {
            Value = value;
            return this;
        }

        public ApplicationCommandOptionChoiceBuilder WithValue(string value)
        {
            Value = value;
            return this;
        }

        public ApplicationCommandOptionChoice Build()
        {
            return new ApplicationCommandOptionChoice()
            {
                Name = Name,
                Value = Value
            };
        }
    }
}
