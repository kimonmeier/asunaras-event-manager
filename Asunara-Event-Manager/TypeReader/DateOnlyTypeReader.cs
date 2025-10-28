using NetCord;
using NetCord.Services.ApplicationCommands;
using NetCord.Services.Commands;
using NetCord.Services.Commands.TypeReaders;
using NetCord.Services.ComponentInteractions;

namespace EventManager.TypeReader;

public class DateOnlyTypeReader : SlashCommandTypeReader<ApplicationCommandContext>
{
    public override async ValueTask<SlashCommandTypeReaderResult> ReadAsync(string value, ApplicationCommandContext context, SlashCommandParameter<ApplicationCommandContext> parameter,
        ApplicationCommandServiceConfiguration<ApplicationCommandContext> configuration, IServiceProvider? serviceProvider)
    {
        await Task.CompletedTask;
        
        if (string.IsNullOrWhiteSpace(value))
        {
            return SlashCommandTypeReaderResult.Success(null);
        }

        if (DateOnly.TryParse(value, out var dateOnly))
        {
            return SlashCommandTypeReaderResult.Success(dateOnly);
        }
        
        return SlashCommandTypeReaderResult.Fail(parameter.Name);
    }

    public override ApplicationCommandOptionType Type => ApplicationCommandOptionType.String;
}