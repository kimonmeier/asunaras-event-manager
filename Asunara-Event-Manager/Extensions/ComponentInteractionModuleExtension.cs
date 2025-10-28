using NetCord;
using NetCord.Rest;
using NetCord.Services.ComponentInteractions;

namespace EventManager.Extensions;

public static class ComponentInteractionModuleExtension
{
    public async static Task Deferred<TContext>(this ComponentInteractionModule<TContext> commandModule,
        bool ephemeral = false) where TContext : IComponentInteractionContext
    {
        await commandModule.Context.Interaction.SendResponseAsync(
            InteractionCallback.DeferredMessage(MessageFlags.Ephemeral));
    }

    public async static Task Answer<TContext>(this ComponentInteractionModule<TContext> commandModule,
        string message, params object[] args) where TContext : IComponentInteractionContext
    {
        await commandModule.Context.Interaction.ModifyResponseAsync(x => x.Content = string.Format(message, args));
    }
}