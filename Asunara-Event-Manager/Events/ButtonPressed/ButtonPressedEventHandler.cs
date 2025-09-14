using Discord;
using Discord.WebSocket;
using EventManager.Events.AddFeedbackPreference;
using EventManager.Events.AddReminderPreference;
using EventManager.Events.BirthdayDelete;
using EventManager.Events.EventFeedbackVisibility;
using EventManager.Events.EventSendFeedbackStar;
using EventManager.Events.UpdateEventFeedbackThread;
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

        if (customId.StartsWith(Konst.ButtonFeedbackVisibilityGroup))
        {
            await HandleFeedbackVisibility(request.Context, customId, userId);
        }

        if (customId.StartsWith(Konst.ButtonBirthdayGroup))
        {
            await HandleBirthday(request.Context, customId, userId);
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
            await arg.RespondAsync(message, ephemeral: true);
        }
    }

    private async Task<bool> TryHandlePreference(string customId, ulong userId)
    {
        switch (customId)
        {
            case Konst.ButtonReminderNo:
                await _sender.Send(new AddReminderPreferenceEvent
                {
                    DiscordUserId = userId, Preference = false
                });

                return true;

            case Konst.ButtonReminderYes:
                await _sender.Send(new AddReminderPreferenceEvent
                {
                    DiscordUserId = userId, Preference = true
                });

                return true;

            case Konst.ButtonFeedbackNo:
                await _sender.Send(new AddFeedbackPreferenceEvent
                {
                    DiscordUserId = userId, Preference = false
                });

                return true;

            case Konst.ButtonFeedbackYes:
                await _sender.Send(new AddFeedbackPreferenceEvent
                {
                    DiscordUserId = userId, Preference = true
                });

                return true;
        }

        return false;
    }

    private async Task HandleStarFeedback(SocketMessageComponent arg, string customId, ulong userId)
    {
        var starButtons = new Dictionary<string, int>
        {
            {
                Konst.ButtonFeedback1Star, 1
            },
            {
                Konst.ButtonFeedback2Star, 2
            },
            {
                Konst.ButtonFeedback3Star, 3
            },
            {
                Konst.ButtonFeedback4Star, 4
            },
            {
                Konst.ButtonFeedback5Star, 5
            }
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
                DiscordEventId = eventId, DiscordUserId = userId, StarCount = starCount
            });

            await arg.Message.ModifyAsync(x =>
            {
                x.Components = new ComponentBuilder()
                    .WithButton("Anonymes Feedback!", $"{Konst.ButtonFeedbackVisibilityAnonymous}{Konst.PayloadDelimiter}{eventId}")
                    .WithButton("Nicht Anonym!", $"{Konst.ButtonFeedbackVisibilityPublic}{Konst.PayloadDelimiter}{eventId}")
                    .Build();
            });

            return;
        }

    }

    private async Task HandleFeedbackVisibility(SocketMessageComponent arg, string customId, ulong userId)
    {
        var eventId = ulong.Parse(customId.Split([Konst.PayloadDelimiter], StringSplitOptions.None)[1]);

        await _sender.Send(new EventFeedbackVisibilityEvent()
        {
            DiscordEventId = eventId, DiscordUserId = userId, Anonymous = customId.StartsWith(Konst.ButtonFeedbackVisibilityAnonymous)
        });

        await arg.Message.ModifyAsync(x =>
        {
            x.Components = new ComponentBuilder().WithButton("Mehr Feedback abgeben!", $"{Konst.ButtonMoreFeedback}{Konst.PayloadDelimiter}{eventId}").Build();
        });
    }

    private async Task HandleMoreFeedback(SocketMessageComponent arg, string customId, ulong userId)
    {
        ModalBuilder modalBuilder = new();
        modalBuilder.Title = "Event-Feedback";
        modalBuilder.CustomId = $"{Konst.Modal.Feedback.Id}{Konst.PayloadDelimiter}{customId.Split([Konst.PayloadDelimiter], StringSplitOptions.None)[1]}";
        modalBuilder.AddTextInput("Das hat mir gefallen", Konst.Modal.Feedback.GoodInputId, TextInputStyle.Paragraph, required: false);
        modalBuilder.AddTextInput("Das hat mir nicht gefallen", Konst.Modal.Feedback.CriticInputId, TextInputStyle.Paragraph, required: false);
        modalBuilder.AddTextInput("Verbesserungsvorschläge", Konst.Modal.Feedback.SuggestionInputId, TextInputStyle.Paragraph, required: false);

        await arg.RespondWithModalAsync(modalBuilder.Build());
    }

    private async Task HandleBirthday(SocketMessageComponent arg, string customId, ulong userId)
    {
        switch (customId)
        {
            case Konst.ButtonBirthdayRegister:        
                ModalBuilder modalBuilder = new();
                modalBuilder.Title = "Geburtstag";
                modalBuilder.CustomId = $"{Konst.Modal.Birthday.Id}{Konst.PayloadDelimiter}{userId}";
                modalBuilder.AddTextInput("Tag", Konst.Modal.Birthday.DayInputId, required: true);
                modalBuilder.AddTextInput("Monat", Konst.Modal.Birthday.MonthInputId, required: true);
                modalBuilder.AddTextInput("Jahr", Konst.Modal.Birthday.YearInputId, required: true);
                await arg.RespondWithModalAsync(modalBuilder.Build());
                
                break;

            case Konst.ButtonBirthdayDelete:
                await _sender.Send(new BirthdayDeleteEvent()
                {
                    DiscordUserId = userId
                });

                await SendSuccessResponse(arg, "Dein Geburtstag wurde erfolgreich gelöscht!");
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(customId), customId, null);
        }
    }
}