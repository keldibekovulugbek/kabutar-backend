using FluentAssertions;
using Kabutar.Service.Services.Common;
using Microsoft.Extensions.Configuration;

namespace Kabutar.Tests.Security;

public class AesEncryptionTests
{
    private readonly AesEncryptionService _sut;

    public AesEncryptionTests()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ENCRYPTION_KEY"] = "KabutarTestEncryptionKey_ForTests"
            })
            .Build();

        _sut = new AesEncryptionService(config);
    }

    [Fact]
    public void Encrypt_ReturnsNonEmptyString()
    {
        var result = _sut.Encrypt("Salom");
        result.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void Encrypt_IsNotPlaintext()
    {
        var result = _sut.Encrypt("Salom");
        result.Should().NotBe("Salom");
    }

    [Fact]
    public void Encrypt_SamePlaintext_ProducesDifferentCiphers()
    {
        var c1 = _sut.Encrypt("Salom");
        var c2 = _sut.Encrypt("Salom");
        c1.Should().NotBe(c2);
    }

    [Fact]
    public void Decrypt_AfterEncrypt_ReturnOriginal()
    {
        var plaintext = "Bu maxfiy xabar!";
        var cipher = _sut.Encrypt(plaintext);
        var result = _sut.Decrypt(cipher);
        result.Should().Be(plaintext);
    }

    [Theory]
    [InlineData("")]
    [InlineData("A")]
    [InlineData("Unicode: O'zbek tili \u0627\u0644\u0639\u0631\u0628\u064a\u0629")]
    [InlineData("Special chars: !@#$%^&*()_+-=[]{}|;':\",./<>?")]
    public void EncryptDecrypt_RoundTrip_VariousInputs(string input)
    {
        var cipher = _sut.Encrypt(input);
        var result = _sut.Decrypt(cipher);
        result.Should().Be(input);
    }

    [Fact]
    public void Decrypt_InvalidBase64_ThrowsException()
    {
        var act = () => _sut.Decrypt("not-valid-base64!!!");
        act.Should().Throw<Exception>();
    }

    [Fact]
    public void Decrypt_TamperedCipher_ThrowsException()
    {
        var cipher = _sut.Encrypt("original");
        var bytes = Convert.FromBase64String(cipher);
        bytes[20] ^= 0xFF;
        var tampered = Convert.ToBase64String(bytes);

        var act = () => _sut.Decrypt(tampered);
        act.Should().Throw<Exception>();
    }
}
