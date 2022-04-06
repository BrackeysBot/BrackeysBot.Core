using System;
using BrackeysBot.Core.API;

namespace BrackeysBot.Core.Data;

internal abstract class UserInfoField
{
    protected UserInfoField(string name, Func<UserInfoFieldContext, string> evaluator,
        Predicate<UserInfoFieldContext>? filterPredicate = null)
    {
        Name = string.IsNullOrWhiteSpace(name) ? throw new ArgumentNullException(nameof(name)) : name;
        ValueEvaluator = evaluator ?? throw new ArgumentNullException(nameof(evaluator));
        FilterPredicate = filterPredicate ?? (_ => true);
    }

    /// <summary>
    ///     Gets or sets the filter by which the field is displayed.
    /// </summary>
    /// <value>The display filter.</value>
    public Predicate<UserInfoFieldContext> FilterPredicate { get; protected set; }

    /// <summary>
    ///     Gets the value evaluator for this field.
    /// </summary>
    /// <value>The value evaluator.</value>
    public Func<UserInfoFieldContext, string> ValueEvaluator { get; protected set; }

    /// <summary>
    ///     Gets the name of this field.
    /// </summary>
    public string Name { get; protected set; }
}

internal sealed class UserInfoField<T> : UserInfoField
    where T : notnull
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="UserInfoField{T}" /> class.
    /// </summary>
    /// <param name="name">The field name.</param>
    /// <param name="evaluator">The field value evaluator.</param>
    /// <param name="filterPredicate">The field display filter.</param>
    public UserInfoField(string name, Func<UserInfoFieldContext, T> evaluator,
        Predicate<UserInfoFieldContext>? filterPredicate = null)
        : base(name, context => evaluator(context).ToString()!, filterPredicate)
    {
    }
}
