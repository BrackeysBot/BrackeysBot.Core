using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BrackeysBot.Core.API;

/// <summary>
///     Represents a class which allows for the creation of embed fields for the <c>userinfo</c> command.
/// </summary>
public sealed class UserInfoFieldBuilder
{
    private string _name = string.Empty;
    private List<Predicate<UserInfoFieldContext>> _executionFilters = new();
    private Func<UserInfoFieldContext, string> _valueEvaluator = _ => string.Empty;

    /// <summary>
    ///     Gets or sets the execution filter of the field.
    /// </summary>
    /// <value>The execution filter.</value>
    /// <exception cref="ArgumentNullException"><paramref name="value" /> is <see langword="null" />.</exception>
    public List<Predicate<UserInfoFieldContext>> ExecutionFilters
    {
        get => _executionFilters;
        set => _executionFilters = value ?? throw new ArgumentNullException(nameof(value));
    }

    /// <summary>
    ///     Gets or sets a value indicating whether this field should be displayed inline.
    /// </summary>
    /// <value><see langword="true" /> if this field should be displayed inline; otherwise, <see langword="false" />.</value>
    public bool IsInline { get; set; } = true;

    /// <summary>
    ///     Gets or sets the name of the field.
    /// </summary>
    /// <value>The name.</value>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="value" /> is <see langword="null" />, empty, or consists of only whitespace.
    /// </exception>
    public string Name
    {
        get => _name;
        set => _name = string.IsNullOrWhiteSpace(value) ? throw new ArgumentNullException(nameof(value)) : value;
    }

    /// <summary>
    ///     Gets or sets the value evaluator of the field.
    /// </summary>
    /// <value>The value evaluator.</value>
    /// <exception cref="ArgumentNullException"><paramref name="value" /> is <see langword="null" />.</exception>
    public Func<UserInfoFieldContext, string> ValueEvaluator
    {
        get => _valueEvaluator;
        set => _valueEvaluator = value ?? throw new ArgumentNullException(nameof(value));
    }

    /// <summary>
    ///     Adds an execution filter to the field.
    /// </summary>
    /// <param name="filterPredicate">The execution filter to add.</param>
    /// <returns>A reference to this instance after the operation has completed.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="filterPredicate" /> is <see langword="null" />.</exception>
    public UserInfoFieldBuilder AddExecutionFilter(Predicate<UserInfoFieldContext> filterPredicate)
    {
        if (filterPredicate is null) throw new ArgumentNullException(nameof(filterPredicate));
        _executionFilters.Add(filterPredicate);
        return this;
    }

    /// <summary>
    ///     Adds an execution filter to the field.
    /// </summary>
    /// <param name="filterPredicate">The execution filter to add.</param>
    /// <returns>A reference to this instance after the operation has completed.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="filterPredicate" /> is <see langword="null" />.</exception>
    public UserInfoFieldBuilder AddExecutionFilter(Func<UserInfoFieldContext, Task<bool>> filterPredicate)
    {
        if (filterPredicate is null) throw new ArgumentNullException(nameof(filterPredicate));
        _executionFilters.Add(context => filterPredicate(context).GetAwaiter().GetResult());
        return this;
    }

    /// <summary>
    ///     Sets whether this field should be displayed inline.
    /// </summary>
    /// <param name="inline">
    ///     <see langword="true" /> if this field should be displayed inline; otherwise, <see langword="false" />.
    /// </param>
    /// <returns>A reference to this instance after the operation has completed.</returns>
    public UserInfoFieldBuilder AsInline(bool inline)
    {
        IsInline = inline;
        return this;
    }

    /// <summary>
    ///     Clears all execution filters from this builder.
    /// </summary>
    /// <returns>A reference to this instance after the operation has completed.</returns>
    public UserInfoFieldBuilder ClearExecutionFilters()
    {
        _executionFilters.Clear();
        return this;
    }

    /// <summary>
    ///     Specifies the execution filter of the field.
    /// </summary>
    /// <param name="filterPredicate">The execution filter.</param>
    /// <returns>A reference to this instance after the operation has completed.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="filterPredicate" /> is <see langword="null" />.</exception>
    public UserInfoFieldBuilder WithExecutionFilter(Predicate<UserInfoFieldContext> filterPredicate)
    {
        ExecutionFilters = new List<Predicate<UserInfoFieldContext>> {filterPredicate};
        return this;
    }

    /// <summary>
    ///     Specifies the execution filter of the field.
    /// </summary>
    /// <param name="filterPredicate">The execution filter.</param>
    /// <returns>A reference to this instance after the operation has completed.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="filterPredicate" /> is <see langword="null" />.</exception>
    public UserInfoFieldBuilder WithExecutionFilter(Func<UserInfoFieldContext, Task<bool>> filterPredicate)
    {
        ExecutionFilters = new List<Predicate<UserInfoFieldContext>>
            {context => filterPredicate(context).GetAwaiter().GetResult()};

        return this;
    }

    /// <summary>
    ///     Specifies the name of the field.
    /// </summary>
    /// <param name="name">The field name.</param>
    /// <returns>A reference to this instance after the operation has completed.</returns>
    /// <exception cref="ArgumentNullException">
    ///     <paramref name="name" /> is <see langword="null" />, empty, or consists of only whitespace.
    /// </exception>
    public UserInfoFieldBuilder WithName(string name)
    {
        Name = name;
        return this;
    }

    /// <summary>
    ///     Specifies the value evaluator of the field.
    /// </summary>
    /// <param name="evaluator">The value evaluator.</param>
    /// <typeparam name="T">The value type.</typeparam>
    /// <returns>A reference to this instance after the operation has completed.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="evaluator" /> is <see langword="null" />.</exception>
    public UserInfoFieldBuilder WithValue<T>(Func<UserInfoFieldContext, T> evaluator)
        where T : notnull
    {
        ValueEvaluator = context => evaluator(context).ToString()!;
        return this;
    }

    /// <summary>
    ///     Specifies the value evaluator of the field.
    /// </summary>
    /// <param name="evaluator">The value evaluator.</param>
    /// <typeparam name="T">The value type.</typeparam>
    /// <returns>A reference to this instance after the operation has completed.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="evaluator" /> is <see langword="null" />.</exception>
    public UserInfoFieldBuilder WithValue<T>(Func<UserInfoFieldContext, Task<T>> evaluator)
        where T : notnull
    {
        ValueEvaluator = context => evaluator(context).GetAwaiter().GetResult().ToString()!;
        return this;
    }
}
