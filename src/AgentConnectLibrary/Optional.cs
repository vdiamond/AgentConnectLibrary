namespace AgentConnectLibrary;

/// <summary>
/// A value that was either left unset or set, including set to null.
/// The default value is unset, so it is left out of the JSON.
/// Assigning null, including <c>description: null</c>, sets the value to JSON null.
/// </summary>
/// <typeparam name="T">The value type. It is a reference type so null can be stored.</typeparam>
public readonly record struct Optional<T> where T : class
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Optional{T}"/> struct.
    /// </summary>
    /// <param name="isSpecified">Whether the caller set the value.</param>
    /// <param name="value">The value, which may be null when <paramref name="isSpecified"/> is true.</param>
    public Optional(bool isSpecified, T? value)
    {
        IsSpecified = isSpecified;
        Value = value;
    }

    /// <summary>Gets whether the caller set this value.</summary>
    public bool IsSpecified { get; }

    /// <summary>Gets the value when <see cref="IsSpecified"/> is true.</summary>
    public T? Value { get; }

    /// <summary>Gets a value that is left out of the JSON.</summary>
    public static Optional<T> Unspecified => default;

    /// <summary>
    /// Sets the value, including null.
    /// </summary>
    /// <param name="value">The value to send. Null is sent as JSON null.</param>
    public static implicit operator Optional<T>(T? value)
    {
        return new Optional<T>(true, value);
    }
}
