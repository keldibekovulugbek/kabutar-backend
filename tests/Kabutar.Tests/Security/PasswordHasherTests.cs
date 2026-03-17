using FluentAssertions;
using Kabutar.Service.Security;

namespace Kabutar.Tests.Security;

public class PasswordHasherTests
{
    [Fact]
    public void Hash_ReturnsNonEmptyString()
    {
        var hash = PasswordHasher.Hash("Parol123!");
        hash.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void Hash_IsNotPlaintext()
    {
        var hash = PasswordHasher.Hash("Parol123!");
        hash.Should().NotBe("Parol123!");
    }

    [Fact]
    public void Hash_SamePasswordProducesDifferentHashes()
    {
        var hash1 = PasswordHasher.Hash("Parol123!");
        var hash2 = PasswordHasher.Hash("Parol123!");
        hash1.Should().NotBe(hash2);
    }

    [Fact]
    public void Verify_CorrectPassword_ReturnsTrue()
    {
        var hash = PasswordHasher.Hash("Parol123!");
        PasswordHasher.Verify("Parol123!", hash).Should().BeTrue();
    }

    [Fact]
    public void Verify_WrongPassword_ReturnsFalse()
    {
        var hash = PasswordHasher.Hash("Parol123!");
        PasswordHasher.Verify("XatoParol!", hash).Should().BeFalse();
    }

    [Fact]
    public void Verify_EmptyPassword_ReturnsFalse()
    {
        var hash = PasswordHasher.Hash("Parol123!");
        PasswordHasher.Verify("", hash).Should().BeFalse();
    }

    [Theory]
    [InlineData("short")]
    [InlineData("Parol123!")]
    [InlineData("VeryLongPasswordWithSpecialChars!@#$%^&*()")]
    public void HashAndVerify_RoundTrip_AlwaysSucceeds(string password)
    {
        var hash = PasswordHasher.Hash(password);
        PasswordHasher.Verify(password, hash).Should().BeTrue();
    }
}
