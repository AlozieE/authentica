using Authentica.BLL.Services;

namespace Authentica_app.Tests;

[TestClass]
public class PasswordGeneratorServiceTests
{
    private PasswordGeneratorService _service = null!;

    [TestInitialize]
    public void Setup()
    {
        _service = new PasswordGeneratorService();
    }

    [TestMethod]
    public void Generate_ReturnsPassword_WithRequestedLength()
    {
        var password = _service.Generate(20);

        Assert.AreEqual(20, password.Length);
    }

    [TestMethod]
    public void Generate_ReturnsPassword_WithMinimumLength()
    {
        var password = _service.Generate(8);

        Assert.AreEqual(8, password.Length);
    }

    [TestMethod]
    public void Generate_ReturnsPassword_WithMaximumLength()
    {
        var password = _service.Generate(128);

        Assert.AreEqual(128, password.Length);
    }

    [TestMethod]
    public void Generate_ContainsUppercaseLetter()
    {
        var password = _service.Generate(16);

        Assert.IsTrue(password.Any(char.IsUpper), "Password should contain at least one uppercase letter.");
    }

    [TestMethod]
    public void Generate_ContainsLowercaseLetter()
    {
        var password = _service.Generate(16);

        Assert.IsTrue(password.Any(char.IsLower), "Password should contain at least one lowercase letter.");
    }

    [TestMethod]
    public void Generate_ContainsDigit()
    {
        var password = _service.Generate(16);

        Assert.IsTrue(password.Any(char.IsDigit), "Password should contain at least one digit.");
    }

    [TestMethod]
    public void Generate_ContainsSymbol()
    {
        const string symbols = "!@#$%^&*()_+-=[]{}|;:,.<>?";
        var password = _service.Generate(16);

        Assert.IsTrue(password.Any(c => symbols.Contains(c)), "Password should contain at least one symbol.");
    }

    [TestMethod]
    public void Generate_ReturnsDifferentPasswords_OnSuccessiveCalls()
    {
        var first  = _service.Generate(16);
        var second = _service.Generate(16);

        Assert.AreNotEqual(first, second);
    }

    [TestMethod]
    public void Generate_Throws_WhenLengthBelowMinimum()
    {
        try
        {
            _service.Generate(7);
            Assert.Fail("Expected ArgumentOutOfRangeException was not thrown.");
        }
        catch (ArgumentOutOfRangeException) { }
    }

    [TestMethod]
    public void Generate_Throws_WhenLengthAboveMaximum()
    {
        try
        {
            _service.Generate(129);
            Assert.Fail("Expected ArgumentOutOfRangeException was not thrown.");
        }
        catch (ArgumentOutOfRangeException) { }
    }
}
