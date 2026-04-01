using api;
using api.model;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using leviubot.Logger;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Reflection;

namespace leviubot.Commands.Interactions;

public class VerificationModule : InteractionModuleBase<SocketInteractionContext>
{
    private const ulong tetrIoRoleId = 1481937327793111051;
    private const ulong manualVerifiedRoleId = 1481937835463413790;

    private const ulong tetrisPlayerRole = 1483930424475516979;

    private const ulong waitingForApproveRoleId = 1483012799847469106;
    private const ulong approveHasBeenDeniedRoleId = 1483085875582734529;

    private Dictionary<string, ulong> roles = new()
    {
        { "x+", 1274075899549188237 },
        { "x",  1138937928475750472 },
        { "u",  1138938225361162250 },
        { "ss", 1138938288632250458 },
        { "s+", 1138938366524657764 },
        { "s",  1138938366524657764 },
        { "s-", 1138938366524657764 },
        { "a+", 1138938728786706442 },
        { "a",  1138938728786706442 },
        { "a-", 1138938728786706442 },
        { "b+", 1138939267100450887 },
        { "b",  1138939267100450887 },
        { "b-", 1138939267100450887 },
        { "c+", 1138939686094651432 },
        { "c",  1138939686094651432 },
        { "c-", 1138939686094651432 },
        { "d+", 1138939911542685819 },
        { "d",  1138939911542685819 },
        { "f",  1138940100089229364 }
    };

    private static ConsoleLogger _logger;

    public VerificationModule(ConsoleLogger logger, InteractionService interactionService)
    {
        _logger = logger;
    }

    [SlashCommand("verification", "Button demo command")]
    [RequireTeam]
    public async Task VerificationButton()
    {
        await RespondAsync(
            embed:
                new EmbedBuilder()
                {
                    Title = "Верификация",
                    Footer = new EmbedFooterBuilder()
                    {
                        Text = "Проверьте что ваш аккаут tetr io привязан к дискорду"
                    }
                }.Build(),
            components:
                new ComponentBuilder()
               .WithButton(
                    new ButtonBuilder()
                    {
                        Label = "Верифицироваться",
                        CustomId = "verificationButton",
                        Style = ButtonStyle.Primary,
                        Emote = new Emote(1369651856514482206, "chilik3")
                    }
               ).Build()
        );
    }



    [ComponentInteraction("verificationButton")]
    public async Task ButtonHandler()
    {
        await RespondAsync("Загрузка...", ephemeral: true);
        var result = await Api.GetResultAsync<League>($"/user/discord?discordId={Context.User.Id}");
        switch (result)
        {
            case Resource<League>.Success success:
                {
                    var user = (Context.User as SocketGuildUser)!;
                    var bestRank = success.Data.BestRank;

                    var ids = roles.Select(item => item.Value).Concat([waitingForApproveRoleId, approveHasBeenDeniedRoleId]).ToList();
                    var userRoleIds = user.Roles.Select(it => it.Id).Where(ids.Contains).ToList();
                    if (userRoleIds.Count != 0) await user.RemoveRolesAsync(roleIds: userRoleIds);

                    if (bestRank is null or "z")
                    {
                        await user.AddRolesAsync(roleIds: new List<ulong> { roles["f"], tetrIoRoleId });
                        await ModifyOriginalResponseAsync(msg => { msg.Content = $"У вас нету ранга, поэтому вы получаете <@&{roles["f"]}>"; });
                    }
                    else if (roles.TryGetValue(bestRank, out var rank))
                    {
                        await user.AddRolesAsync(roleIds: new List<ulong> { rank, tetrIoRoleId });
                        await ModifyOriginalResponseAsync(msg => { msg.Content = $"Ваш ранк: {bestRank}, поэтому вы получаете <@&{rank}>"; });
                    }
                    else
                    {
                        await ModifyOriginalResponseAsync(msg => { msg.Content = $"Error: cannot parse {bestRank}"; });
                    }
                    break;
                }

            case Resource<League>.Error error:
                {
                    await ModifyOriginalResponseAsync(msg => { msg.Content = $"Error{error.ErrorMessage}\n-# Dm leviu_4el if you are sure that your tetr io account is connected to discord"; });
                    break;
                }

            case Resource<League>.ServerError error:
                {
                    if (error.Error == AppError.ConnectionLost)
                    {
                        await ModifyOriginalResponseAsync(msg => { msg.Content = $"Произошла ошибка подключения к серверу\n-# Dm leviu_4el if this error hasn't gone away"; });
                        break;
                    }
                    else if (error.Error == AppError.UserNotFound || error.Error == AppError.UserSuccessFailed)
                    {
                        await ModifyOriginalResponseAsync(msg =>
                        {
                            msg.Content = "";
                            msg.Embeds =
                                new Embed[]
                                {
                                    new EmbedBuilder()
                                    {
                                        Title = "Произошла ошибка при верификации",
                                        Description = "Ваш аккаунт не привязан к tetr io\nПривяжите свой аккаунт к tetr io и повторите попытку",
                                    }.Build(),
                                    new EmbedBuilder()
                                    {
                                        Title = "Инструкция по привязке дискорда",
                                        Description = "CONFIG > ACCOUNT > CONNECTIONS",
                                        ImageUrl = "https://tetrio.github.io/faq/res/faq/troubleshooting/link-discord.gif",
                                        Footer = new EmbedFooterBuilder()
                                        {
                                            Text = "Eсли у вас нету аккаунта tetr io, но вы играете в тетрис, то вы можете Верифицироваться другим способом"
                                        }
                                    }.Build(),
                                };

                            msg.Components =
                                new ComponentBuilder()
                                .WithButton(
                                        new ButtonBuilder()
                                        {
                                            Label = "Повторить попытку",
                                            CustomId = "verificationButton",
                                            Style = ButtonStyle.Secondary,
                                            Emote = new Emote(1369651856514482206, "chilik3")
                                        }
                                )
                                .WithButton(
                                        new ButtonBuilder()
                                        {
                                            Label = "Верифицироваться другим способом",
                                            CustomId = "altVerification",
                                            Style = ButtonStyle.Danger,
                                            Emote = new Emote(1369651856514482206, "chilik3")
                                        }
                                ).Build();
                        });
                    }
                    break;
                }

        }


    }

    [ComponentInteraction("altVerification")]
    public async Task AltVerificationHandler()
    {
        var user = Context.User as SocketGuildUser;
        if (user.Roles.Any(role => role.Id == waitingForApproveRoleId))
        {
            await RespondAsync("Ваша заявка ещё ожидает проверки", ephemeral: true);
        }
        else if (user.Roles.Any(role => role.Id == approveHasBeenDeniedRoleId))
        {
            await RespondAsync("Ваша заявка была отклонена, используйте верификацию через tetr io", ephemeral: true);
        }
        else await RespondWithModalAsync<AltVerificationModal>("verification-modal");
    }

    Dictionary<string, string> list = new()
    {
        {"0", "Другое"},
        {"1", "Jstris" },
        {"2", "Classic tetris"},
    };
    public class AltVerificationModal : IModal
    {
        public string Title => "Оставить заявку";

        [ModalTextDisplay(content: "Fallback content")]
        public string? TextDisplay { get; set; } = "Подтверждение заявки админом может занять некоторое время";

        [InputLabel("Дополнительна информация")]
        [ModalTextInput("username", TextInputStyle.Paragraph, "Укажите дополнительную информацию(ссылки, или другую информацию)")]
        [Required]
        public string Description { get; set; }

        [InputLabel("Укажите в какой тетрис вы играете")]
        [ModalSelectMenu("list", maxValues: 1)]
        [Required]
        [ModalSelectMenuOption("Jstris", "1")]
        [ModalSelectMenuOption("Classic tetris", "2")]
        [ModalSelectMenuOption("Другое", "0")]
        public string[] TextSelectMenu { get; set; }

        [InputLabel("Дополнительно")]
        [RequiredInput(false)]
        [ModalTextInput("other", TextInputStyle.Short, "Укажите какой тетрис, если вы выбрали другое", maxLength: 50)]
        public string? Other { get; set; }

    }


    [ModalInteraction("verification-modal")]
    public async Task AltVerificationModalHandler(AltVerificationModal modal)
    {
        var user = Context.User as SocketGuildUser;
        var value = list[modal.TextSelectMenu[0]];

        var channel = Context.Guild.GetChannel(1483012073737945128) as SocketTextChannel;
        await channel!.SendMessageAsync(
            embed: new EmbedBuilder()
            {
                Description = $"Заявка от <@{user.Id}>",
                Fields = new List<EmbedFieldBuilder> {
                    new EmbedFieldBuilder()
                    {
                        Name = "Описание",
                        Value = modal.Description
                    },
                    new EmbedFieldBuilder()
                    {
                        Name = "Вид тетриса",
                        Value = value == "Другое" ? $"Другой:{modal.Other ?? "Ничего не указал"}" : value
                    }
                }
            }.Build(),
            components: new ComponentBuilder()
                .WithButton(
                        new ButtonBuilder()
                        {
                            Label = "Приянять",
                            CustomId = $"approveBtn-{user.Id}",
                            Style = ButtonStyle.Success
                        }
                )
                .WithButton(
                        new ButtonBuilder()
                        {
                            Label = "Отклонить",
                            CustomId = $"denyBtn-{user.Id}",
                            Style = ButtonStyle.Danger
                        }
                ).Build()
        );
        await user.AddRoleAsync(waitingForApproveRoleId);
        await RespondAsync("Ожидайте принятия вашей заявки", ephemeral: true);
    }

    [ComponentInteraction("approveBtn-.+?$", TreatAsRegex = true)]
    public async Task ApproveButtonHandler()
    {
        var component = Context.Interaction as SocketMessageComponent;

        var id = component!.Data.CustomId.Split("-").Last();
        var user = Context.Guild.GetUser(ulong.Parse(id));


        var message = component.Message;

        var oldEmbed = message.Embeds.First();
        var embedBuilder = oldEmbed.ToEmbedBuilder();

        await user.RemoveRoleAsync(waitingForApproveRoleId);
        await user.AddRoleAsync(manualVerifiedRoleId);
        await user.AddRoleAsync(tetrisPlayerRole);

        embedBuilder.Footer = new EmbedFooterBuilder()
        {
            Text = $"Была одобренна: {Context.User.Username}",
            IconUrl = Context.User.GetAvatarUrl()
        };

        await message.ModifyAsync(msg =>
        {
            msg.Embed = embedBuilder.Build();
            msg.Components = new ComponentBuilder().Build();
        });
    }

    [ComponentInteraction("denyBtn-.+?$", TreatAsRegex = true)]
    public async Task DenyButtonHandler()
    {
        var component = Context.Interaction as SocketMessageComponent;

        var id = component!.Data.CustomId.Split("-").Last();
        var user = Context.Guild.GetUser(ulong.Parse(id));


        var message = component.Message;

        var oldEmbed = message.Embeds.First();
        var embedBuilder = oldEmbed.ToEmbedBuilder();

        await user.RemoveRoleAsync(waitingForApproveRoleId);
        await user.AddRoleAsync(approveHasBeenDeniedRoleId);

        embedBuilder.Footer = new EmbedFooterBuilder()
        {
            Text = $"Была отклонена: {Context.User.Username}",
            IconUrl = Context.User.GetAvatarUrl()
        };

        await message.ModifyAsync(msg =>
        {
            msg.Embed = embedBuilder.Build();
            msg.Components = new ComponentBuilder().Build();
        });
    }
}