using NetCord;
using NetCord.Rest;
using NetCord.Services.ApplicationCommands;

namespace EventManager.Extensions;

public static class ApplicationCommandModuleExtension
{
    public async static Task Deferred(this ApplicationCommandModule<ApplicationCommandContext> commandModule,
        bool ephemeral = false)
    {
        await commandModule.Context.Interaction.SendResponseAsync(
            InteractionCallback.DeferredMessage(MessageFlags.Ephemeral));
    }

    public async static Task Answer(this ApplicationCommandModule<ApplicationCommandContext> commandModule,
        string message, params object[] args)
    {
        await commandModule.Context.Interaction.ModifyResponseAsync(x => x.Content = string.Format(message, args));
    }
}