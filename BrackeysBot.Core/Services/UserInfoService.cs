using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BrackeysBot.API.Extensions;
using BrackeysBot.Core.API;
using BrackeysBot.Core.Data;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;

namespace BrackeysBot.Core.Services;

/// <summary>
///     Represents a service that registers and handles fields that will be displayed with the <c>userinfo</c> command.
/// </summary>
internal sealed class UserInfoService
{
    private readonly List<UserInfoField> _registeredFields = new();

    /// <summary>
    ///     Gets a read-only view of the registered fields.
    /// </summary>
    /// <value>A read-only view of the registered fields.</value>
    public IReadOnlyList<UserInfoField> RegisteredFields => _registeredFields.AsReadOnly();

    /// <summary>
    ///     Constructs a new <see cref="UserInfoFieldContext" /> based on parameters provided by a
    ///     <see cref="ContextMenuContext" />.
    /// </summary>
    /// <param name="context">The context from which to construct the <see cref="UserInfoFieldContext" />.</param>
    /// <returns>A new instance of <see cref="UserInfoFieldContext" />.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="context" /> is <see langword="null" />.</exception>
    public async Task<UserInfoFieldContext> CreateContextAsync(ContextMenuContext context)
    {
        if (context is null) throw new ArgumentNullException(nameof(context));

        DiscordUser user = context.Interaction.Data.Resolved.Users.First().Value;
        DiscordMember? member = await user.GetAsMemberAsync(context.Guild);

        return new UserInfoFieldContext
        {
            Channel = context.Channel,
            Guild = context.Guild,
            Member = context.Member,
            TargetMember = member,
            TargetUser = user,
            User = context.User,
        };
    }

    /// <summary>
    ///     Constructs a new <see cref="UserInfoFieldContext" /> based on parameters provided by a
    ///     <see cref="InteractionContext" />.
    /// </summary>
    /// <param name="context">The context from which to construct the <see cref="UserInfoFieldContext" />.</param>
    /// <param name="user">The user whose information to retrieve.</param>
    /// <returns>A new instance of <see cref="UserInfoFieldContext" />.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <para><paramref name="context" /> is <see langword="null" />.</para>
    ///     -or-
    ///     <para><paramref name="user" /> is <see langword="null" />.</para>
    /// </exception>
    public async Task<UserInfoFieldContext> CreateContextAsync(InteractionContext context, DiscordUser user)
    {
        if (context is null) throw new ArgumentNullException(nameof(context));
        if (user is null) throw new ArgumentNullException(nameof(user));

        DiscordMember? member = await user.GetAsMemberAsync(context.Guild);

        return new UserInfoFieldContext
        {
            Channel = context.Channel,
            Guild = context.Guild,
            Member = context.Member,
            TargetMember = member,
            TargetUser = user,
            User = context.User,
        };
    }

    /// <summary>
    ///     Constructs a new <see cref="UserInfoFieldContext" /> based on parameters provided by a
    ///     <see cref="CommandContext" />.
    /// </summary>
    /// <param name="context">The context from which to construct the <see cref="UserInfoFieldContext" />.</param>
    /// <param name="user">The user whose information to retrieve.</param>
    /// <returns>A new instance of <see cref="UserInfoFieldContext" />.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <para><paramref name="context" /> is <see langword="null" />.</para>
    ///     -or-
    ///     <para><paramref name="user" /> is <see langword="null" />.</para>
    /// </exception>
    public async Task<UserInfoFieldContext> CreateContextAsync(CommandContext context, DiscordUser user)
    {
        if (context is null) throw new ArgumentNullException(nameof(context));
        if (user is null) throw new ArgumentNullException(nameof(user));

        DiscordMember? member = await user.GetAsMemberAsync(context.Guild);

        return new UserInfoFieldContext
        {
            Channel = context.Channel,
            Guild = context.Guild,
            Member = context.Member,
            TargetMember = member,
            TargetUser = user,
            User = context.User,
        };
    }

    /// <summary>
    ///     Constructs a user information embed from the given context.
    /// </summary>
    /// <param name="context">The context from which to construct the embed.</param>
    /// <returns>A new instance of <see cref="DiscordEmbed" />, populated with the registered fields.</returns>
    public DiscordEmbed CreateEmbed(UserInfoFieldContext context)
    {
        var embed = new DiscordEmbedBuilder();
        embed.WithAuthor(context.TargetUser);
        embed.WithTitle($"Information about {context.TargetUser.GetUsernameWithDiscriminator()}");
        embed.WithThumbnail(context.TargetUser.GetAvatarUrl(ImageFormat.Png));

        DiscordMember? member = context.TargetMember;

        if (member is null)
        {
            embed.WithColor(DiscordColor.Gray);
            embed.WithFooter("⚠️ This user is not currently in the server!");
        }
        else
        {
            embed.WithColor(member.Color);
        }

        foreach (UserInfoField field in RegisteredFields.Where(f => f.FilterPredicate(context)))
            embed.AddField(field.Name, field.ValueEvaluator(context), true);

        return embed;
    }

    /// <summary>
    ///     Registers a field to be displayed with the <c>userinfo</c> command.
    /// </summary>
    /// <param name="builder">The field builder.</param>
    public void RegisterField(UserInfoFieldBuilder builder)
    {
        RegisterField(new UserInfoField<string>(builder.Name, builder.ValueEvaluator,
            context => builder.ExecutionFilters.All(filter => filter(context))));
    }

    /// <summary>
    ///     Registers a field to be displayed with the <c>userinfo</c> command.
    /// </summary>
    /// <param name="field">The field.</param>
    /// <typeparam name="T">The type of the field value.</typeparam>
    public void RegisterField<T>(UserInfoField<T> field)
        where T : notnull
    {
        if (field is null) throw new ArgumentNullException(nameof(field));
        _registeredFields.Add(field);
    }

    /// <summary>
    ///     Registers a field to be displayed with the <c>userinfo</c> command.
    /// </summary>
    /// <param name="name">The field name.</param>
    /// <param name="evaluator">The value evaluator.</param>
    /// <typeparam name="T">The type of the field value.</typeparam>
    public void RegisterField<T>(string name, Func<UserInfoFieldContext, T> evaluator)
        where T : notnull
    {
        if (name is null) throw new ArgumentNullException(nameof(name));
        if (evaluator is null) throw new ArgumentNullException(nameof(evaluator));

        RegisterField(new UserInfoField<T>(name, evaluator));
    }
}
