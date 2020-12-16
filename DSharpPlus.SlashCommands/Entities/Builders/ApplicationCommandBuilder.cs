using System;
using System.Collections.Generic;

namespace DSharpPlus.SlashCommands.Entities.Builders
{
    public class ApplicationCommandBuilder
    {
        public string Name { get; set; }
        public string Description { get; set; }

        /// <summary>
        /// Options/Subcommands - Max of 10.
        /// </summary>
        public List<ApplicationCommandOptionBuilder> Options { get; set; }

        public ApplicationCommandBuilder()
        {
            Name = "";
            Description = "";
            Options = new();
        }

        public ApplicationCommandBuilder WithName(string name)
        {
            if (name is null || name == "")
                throw new Exception("Name cannot be null");

            if (name.Length < 3 || name.Length > 32)
                throw new Exception("Name must be between 3 and 32 characters.");

            Name = name;
            return this;
        }

        public ApplicationCommandBuilder WithDescription(string description)
        {
            if (description.Length < 1 || description.Length > 100)
                throw new Exception("Description must be between 1 and 100 characters.");

            Description = description;
            return this;
        }

        public ApplicationCommandBuilder AddOption(ApplicationCommandOptionBuilder options)
        {
            Options.Add(options);
            return this;
        }

        public ApplicationCommand Build()
        {
            List<ApplicationCommandOption> options = new();
            foreach (var op in Options)
                options.Add(op.Build());

            if (Description is null || Description == "")
                Description = "none provided";

            return new ApplicationCommand()
            {
                Name = Name,
                Description = Description,
                Options = options.Count > 0 ? options.ToArray() : null
            };
        }
    }
}
