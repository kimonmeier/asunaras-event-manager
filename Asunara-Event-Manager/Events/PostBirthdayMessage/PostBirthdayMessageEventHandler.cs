using Discord;
using Discord.WebSocket;
using EventManager.Configuration;
using MediatR;

namespace EventManager.Events.PostBirthdayMessage;

public class PostBirthdayMessageEventHandler : IRequestHandler<PostBirthdayMessageEvent>
{
    private readonly RootConfig _rootConfig;
    private readonly DiscordSocketClient _client;

    public PostBirthdayMessageEventHandler(RootConfig rootConfig, DiscordSocketClient client)
    {
        _rootConfig = rootConfig;
        _client = client;
    }

    public async Task Handle(PostBirthdayMessageEvent request, CancellationToken cancellationToken)
    {
        EmbedBuilder builder = new EmbedBuilder()
            .WithTitle("Geburtstage")
            .WithDescription("Hier kannst du dein Geburtstag verwalten!")
            .WithAuthor(new EmbedAuthorBuilder()
            {
                Name = "Midnight-Café Event-Bot"
            })
            .AddField(_rootConfig.Discord.Emote.Yes, "Hiermit kannst du deinen Geburtstag setzen und/oder ändern")
            .AddField(_rootConfig.Discord.Emote.No, "Hiermit kannst du deinen Geburtstag löschen")
            .WithColor(Color.DarkBlue);
        
        ComponentBuilder componentBuilder = new ComponentBuilder()
            .AddRow(new ActionRowBuilder()
                .WithButton(" ", Konst.ButtonBirthdayRegister, ButtonStyle.Primary, Emote.Parse(_rootConfig.Discord.Emote.Yes))
                .WithButton(" ", Konst.ButtonBirthdayDelete, ButtonStyle.Secondary, Emote.Parse(_rootConfig.Discord.Emote.No)));


        await _client.GetGuild(_rootConfig.Discord.MainDiscordServerId).GetTextChannel(request.TextChannelId).SendMessageAsync(embed: builder.Build(), components: componentBuilder.Build());
    }
}