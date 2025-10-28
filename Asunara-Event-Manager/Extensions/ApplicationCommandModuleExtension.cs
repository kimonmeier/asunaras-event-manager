using NetCord;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;

namespace EventManager.Extensions;

public static class ApplicationCommandModuleExtension
{
    public static async Task Deferred(this ApplicationCommandModule<ApplicationCommandContext> commandModule,
        bool ephemeral = false)
    {
        await commandModule.Context.Interaction.SendResponseAsync(
            InteractionCallback.DeferredMessage(MessageFlags.Ephemeral));
    }

    public static async Task Answer(this ApplicationCommandModule<ApplicationCommandContext> commandModule,
        string message, params object[] args)
    {
        await commandModule.Context.Interaction.ModifyResponseAsync(x => x.Content = string.Format(message, args));
    }
}