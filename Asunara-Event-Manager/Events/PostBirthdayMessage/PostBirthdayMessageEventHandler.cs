using EventManager.Configuration;
using EventManager.Extensions;
using MediatR;
using NetCord;
using NetCord.Gateway;
using NetCord.Rest;

namespace EventManager.Events.PostBirthdayMessage;

public class PostBirthdayMessageEventHandler : IRequestHandler<PostBirthdayMessageEvent>
{
    private readonly RootConfig _rootConfig;
    private readonly GatewayClient _client;

    public PostBirthdayMessageEventHandler(RootConfig rootConfig, GatewayClient client)
    {
        _rootConfig = rootConfig;
        _client = client;
    }

    public async Task Handle(PostBirthdayMessageEvent request, CancellationToken cancellationToken)
    {
        // TODO: Emoji refactor
        EmbedProperties builder = new EmbedProperties()
            .WithTitle("Geburtstage")
            .WithDescription("Hier kannst du dein Geburtstag verwalten!")
            .WithAuthor(new EmbedAuthorProperties()
            {
                Name = "Midnight-Café Event-Bot"
            })
            .AddFields(new EmbedFieldProperties()
            {
                Name = EmojiProperties.Custom(_rootConfig.Discord.Emote.Yes).ToString(),
                Value = "Hiermit kannst du deinen Geburtstag setzen und/oder ändern"
            })
            .AddFields(new EmbedFieldProperties()
            {
                Name = EmojiProperties.Custom(_rootConfig.Discord.Emote.Yes).ToString(),
                Value = "Hiermit kannst du deinen Geburtstag löschen"
            })
            .WithColor(new Color(0, 100, 255));

        ActionRowProperties rowProperties = new ActionRowProperties()
            .AddComponents([
                new ButtonProperties(Konst.ButtonBirthdayRegister,
                    EmojiProperties.Custom(_rootConfig.Discord.Emote.Yes), ButtonStyle.Primary),
                new ButtonProperties(Konst.ButtonBirthdayDelete, EmojiProperties.Custom(_rootConfig.Discord.Emote.No),
                    ButtonStyle.Secondary)
            ]);

        MessageProperties messageProperties = new MessageProperties();
        messageProperties.AddEmbeds(builder);
        messageProperties.AddComponents(rowProperties);

        await _client.Cache.Guilds[_rootConfig.Discord.MainDiscordServerId].GetTextChannel(request.TextChannelId)
            .SendMessageAsync(messageProperties, cancellationToken: cancellationToken);
    }
}