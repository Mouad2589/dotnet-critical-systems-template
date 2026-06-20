using Domain.ValueObjects;
using FluentAssertions;

namespace Unit.Domain;

public sealed class NationalIdTests
{
    [Theory]
    [InlineData("AB12345678")]
    [InlineData("ABCD1234")]
    [InlineData("123456789012")]
    public void From_WithValidFormat_ReturnsNationalId(string value)
    {
        var id = NationalId.From(value);
        id.Value.Should().Be(value);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("ab12345678")]   // lowercase rejected
    [InlineData("AB1234567")]    // too short (7 chars)
    [InlineData("AB1234567890X")] // too long (13 chars)
    [InlineData("AB-1234567")]   // invalid character
    public void From_WithInvalidFormat_ThrowsArgumentException(string value)
    {
        var act = () => NationalId.From(value);
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void TwoNationalIds_WithSameValue_AreEqual()
    {
        var a = NationalId.From("AB12345678");
        var b = NationalId.From("AB12345678");

        a.Should().Be(b);
        (a == b).Should().BeTrue();
    }

    [Fact]
    public void TwoNationalIds_WithDifferentValues_AreNotEqual()
    {
        var a = NationalId.From("AB12345678");
        var b = NationalId.From("AB87654321");

        a.Should().NotBe(b);
        (a != b).Should().BeTrue();
    }
}
