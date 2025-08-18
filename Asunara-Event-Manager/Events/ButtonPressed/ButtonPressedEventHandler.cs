using Discord;
using Discord.WebSocket;
using EventManager.Events.AddFeedbackPreference;
using EventManager.Events.AddReminderPreference;
using EventManager.Events.EventSendFeedbackStar;
using MediatR;

namespace EventManager.Events.ButtonPressed;

public class ButtonPressedEventHandler : IRequestHandler<ButtonPressedEvent>
{
    private readonly ISender _sender;

    public ButtonPressedEventHandler(ISender sender)
    {
        _sender = sender;
    }

    public async Task Handle(ButtonPressedEvent request, CancellationToken cancellationToken)
    {
        var customId = request.Context.Data.CustomId;
        var userId = request.Context.User.Id;
    
        if (await TryHandlePreference(customId, userId))
        {
            await SendSuccessResponse(request.Context, "Deine Wunsch wurde registriert!");
            return;
        }

        if (customId.StartsWith(Konst.ButtonFeedbackStarGroup))
        {
            await HandleStarFeedback(request.Context, customId, userId);
        }

        if (customId.StartsWith(Konst.ButtonMoreFeedback))
        {
            await HandleMoreFeedback(request.Context, customId, userId);
        }
    }

    private async Task SendSuccessResponse(SocketMessageComponent arg, string message)
    {
        if (arg.HasResponded)
        {
        
            await arg.ModifyOriginalResponseAsync(x => x.Content = message);
        }
        else
        {
            await arg.RespondAsync(message);
        }
    }

    private async Task<bool> TryHandlePreference(string customId, ulong userId)
    {
        switch (customId)
        {
            case Konst.ButtonReminderNo:
                await _sender.Send(new AddReminderPreferenceEvent 
                { 
                    DiscordUserId = userId, 
                    Preference = false 
                });
                return true;
            
            case Konst.ButtonReminderYes:
                await _sender.Send(new AddReminderPreferenceEvent 
                { 
                    DiscordUserId = userId, 
                    Preference = true 
                });
                return true;
            
            case Konst.ButtonFeedbackNo:
                await _sender.Send(new AddFeedbackPreferenceEvent 
                { 
                    DiscordUserId = userId, 
                    Preference = false 
                });
                return true;
            
            case Konst.ButtonFeedbackYes:
                await _sender.Send(new AddFeedbackPreferenceEvent 
                { 
                    DiscordUserId = userId, 
                    Preference = true 
                });
                return true;
        }

        return false;
    }

    private async Task HandleStarFeedback(SocketMessageComponent arg, string customId, ulong userId)
    {
        var starButtons = new Dictionary<string, int>
        {
            { Konst.ButtonFeedback1Star, 1 },
            { Konst.ButtonFeedback2Star, 2 },
            { Konst.ButtonFeedback3Star, 3 },
            { Konst.ButtonFeedback4Star, 4 },
            { Konst.ButtonFeedback5Star, 5 }
        };

        foreach (var (buttonPrefix, starCount) in starButtons)
        {
            if (!customId.StartsWith(buttonPrefix))
            {
                continue;
            }

            var eventId = ulong.Parse(customId.Split([Konst.PayloadDelimiter], StringSplitOptions.None)[1]);
            await _sender.Send(new EventSendFeedbackStarEvent
            {
                DiscordEventId = eventId,
                DiscordUserId = userId,
                StarCount = starCount
            });

            await arg.Message.ModifyAsync(x =>
            {
                x.Components = new ComponentBuilder().WithButton("Mehr Feedback abgeben!", $"{Konst.ButtonMoreFeedback}{Konst.PayloadDelimiter}{eventId}").Build();
            });

            return;
        }

    }

    private async Task HandleMoreFeedback(SocketMessageComponent arg, string customId, ulong userId)
    {
        ModalBuilder modalBuilder = new();
        modalBuilder.Title = "Event-Feedback";
        modalBuilder.CustomId = $"{Konst.Modal.Feedback.Id}{Konst.PayloadDelimiter}{customId.Split([Konst.PayloadDelimiter], StringSplitOptions.None)[1]}";
        modalBuilder.AddTextInput("Das hat mir gefallen", Konst.Modal.Feedback.GoodInputId, TextInputStyle.Paragraph);
        modalBuilder.AddTextInput("Das hat mir nicht gefallen", Konst.Modal.Feedback.CriticInputId, TextInputStyle.Paragraph);
        modalBuilder.AddTextInput("Verbesserungsvorschläge", Konst.Modal.Feedback.SuggestionInputId, TextInputStyle.Paragraph);
        
        await arg.RespondWithModalAsync(modalBuilder.Build());
    }
}