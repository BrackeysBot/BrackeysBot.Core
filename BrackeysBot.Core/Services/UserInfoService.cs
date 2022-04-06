using System;
using System.Collections.Generic;
using System.Linq;
using BrackeysBot.Core.API;
using BrackeysBot.Core.Data;

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
