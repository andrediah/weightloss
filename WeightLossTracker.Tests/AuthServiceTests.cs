using WeightLossTracker.Services;
using Xunit;

namespace WeightLossTracker.Tests;

public class AuthServiceTests
{
    private readonly AuthService _sut = new();

    [Fact]
    public void HashPassword_ReturnsNonEmptyString()
    {
        var hash = _sut.HashPassword("secret123");
        Assert.False(string.IsNullOrEmpty(hash));
    }

    [Fact]
    public void HashPassword_DoesNotReturnPlaintext()
    {
        var hash = _sut.HashPassword("secret123");
        Assert.NotEqual("secret123", hash);
    }

    [Fact]
    public void VerifyPassword_ReturnsTrueForCorrectPassword()
    {
        var hash = _sut.HashPassword("correct");
        Assert.True(_sut.VerifyPassword("correct", hash));
    }

    [Fact]
    public void VerifyPassword_ReturnsFalseForWrongPassword()
    {
        var hash = _sut.HashPassword("correct");
        Assert.False(_sut.VerifyPassword("wrong", hash));
    }

    [Fact]
    public void HashPassword_ProducesUniqueHashesEachCall()
    {
        var h1 = _sut.HashPassword("same");
        var h2 = _sut.HashPassword("same");
        Assert.NotEqual(h1, h2);
    }
}
