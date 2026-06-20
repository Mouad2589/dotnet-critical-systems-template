using System.Text.RegularExpressions;

namespace Domain.ValueObjects;

/// <summary>
/// National identifier value object.
/// Encapsulates format validation and equality semantics.
/// </summary>
public sealed class NationalId : IEquatable<NationalId>
{
    private static readonly Regex _format = new(@"^[A-Z0-9]{8,12}$", RegexOptions.Compiled);

    public string Value { get; }

    private NationalId(string value) => Value = value;

    public static NationalId From(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("National ID cannot be empty.", nameof(value));

        var normalised = value.Trim().ToUpperInvariant();

        if (!_format.IsMatch(normalised))
            throw new ArgumentException(
                $"National ID '{normalised}' does not match the expected format.", nameof(value));

        return new NationalId(normalised);
    }

    public bool Equals(NationalId? other) => other is not null && Value == other.Value;
    public override bool Equals(object? obj) => obj is NationalId other && Equals(other);
    public override int GetHashCode() => Value.GetHashCode();
    public override string ToString() => Value;

    public static bool operator ==(NationalId? left, NationalId? right) =>
        left?.Equals(right) ?? right is null;

    public static bool operator !=(NationalId? left, NationalId? right) => !(left == right);
}
