using EventManager.Events.EventFeedbackVisibility;
using EventManager.Events.EventSendFeedbackStar;
using MediatR;
using Microsoft.Diagnostics.Tracing.Parsers.MicrosoftWindowsTCPIP;
using NetCord;
using NetCord.Rest;
using NetCord.Services.ComponentInteractions;

namespace EventManager.Interactions.Button;

public class FeedbackButtonInteraction(ISender sender) : ComponentInteractionModule<ButtonInteractionContext>
{
    [ComponentInteraction(Konst.ButtonFeedbackStar)]
    public async Task UserStartsFeedback(int stars, ulong discordEventId)
    {
        await sender.Send(new EventSendFeedbackStarEvent
        {
            DiscordEventId = discordEventId, DiscordUserId = Context.User.Id, StarCount = stars
        });

        await Context.Message.ModifyAsync(x =>
            x.WithComponents([
                new ActionRowProperties()
                    .AddComponents([
                        new ButtonProperties(
                            $"{Konst.ButtonFeedbackVisibility}{Konst.PayloadDelimiter}{discordEventId}{Konst.PayloadDelimiter}{true}",
                            "Anonymes Feedback!", ButtonStyle.Primary),
                        new ButtonProperties(
                            $"{Konst.ButtonFeedbackVisibility}{Konst.PayloadDelimiter}{discordEventId}{Konst.PayloadDelimiter}{false}",
                            "Nicht Anonym!", ButtonStyle.Secondary)
                    ])
            ]));
    }

    [ComponentInteraction(Konst.ButtonFeedbackVisibility)]
    public async Task UserEnhancesFeedbackWithAnonymity(ulong discordEventId, bool isAnonym)
    {
        await sender.Send(new EventFeedbackVisibilityEvent()
        {
            DiscordEventId = discordEventId, DiscordUserId = Context.User.Id, Anonymous = isAnonym
        });

        await Context.Message.ModifyAsync(x =>
        {
            x.WithComponents([
                new ActionRowProperties().WithComponents([
                    new ButtonProperties($"{Konst.ButtonMoreFeedback}{Konst.PayloadDelimiter}{discordEventId}",
                        "Mehr Feedback abgeben!", ButtonStyle.Primary)
                ])
            ]);
        });
    }

    [ComponentInteraction(Konst.ButtonMoreFeedback)]
    public async Task UserEnhancesFeedback(ulong discordEventId)
    {
        ModalProperties modalProperties =
            new ModalProperties($"{Konst.Modal.Feedback.Id}{Konst.PayloadDelimiter}{discordEventId}",
                "Enhanced Feedback");
        modalProperties.AddComponents([
            new LabelProperties("Das hat dir gefallen?",
                new TextInputProperties(Konst.Modal.Feedback.GoodInputId, TextInputStyle.Paragraph)),
            new LabelProperties("Das hat dir nicht gefallen?",
                new TextInputProperties(Konst.Modal.Feedback.CriticInputId, TextInputStyle.Paragraph)),
            new LabelProperties("Das würdest du besser machen?",
                new TextInputProperties(Konst.Modal.Feedback.SuggestionInputId, TextInputStyle.Paragraph))
        ]);

        await Context.Interaction.SendResponseAsync(InteractionCallback.Modal(modalProperties));
    }
}