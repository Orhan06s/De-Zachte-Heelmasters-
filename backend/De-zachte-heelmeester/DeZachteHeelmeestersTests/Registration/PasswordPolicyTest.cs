using De_zachte_heelmeester.Validators;

namespace DeZachteHeelmeestersTests.Registration;

[TestClass]
public class PasswordPolicyTest
{
    [TestMethod]
    [TestProperty( "LOG-01", "AC3")]
    public void Validate_TooShort_ReturnsTooShortError()
    {
        // Arrange
        var password = "!Te2026";

        // Act
        var result = PasswordPolicy.Validate(password);

        // Assert
        Assert.IsTrue(result.Contains("Gebruik minimaal 8 tekens."));
    }

    [TestMethod]
    [TestProperty("LOG-01", "AC3")]
    public void Validate_NoUppercase_ReturnsUppercaseError()
    {
        // Arrange
        var password = "!test2026";
        
        // Act
        var result = PasswordPolicy.Validate(password);
        
        // Assert
        Assert.IsTrue(result.Contains("Gebruik minimaal één hoofdletter."));
    }

    [TestMethod]
    [TestProperty("LOG-01", "AC3")]
    public void Validate_NoSpecialCharacter_ReturnsSpecialCharacterError()
    {
        // Arrange
        var password = "Test2026";
        
        // Act
        var result = PasswordPolicy.Validate(password);
        
        // Assert
        Assert.IsTrue(result.Contains("Gebruik minimaal één speciaal teken."));
    }
    
    [TestMethod]
    [TestProperty("LOG-01", "AC3")]
    public void Validate_NoLowercase_ReturnsLowercaseError()
    {
        // Arrange
        var password = "!TEST2026";

        // Act
        var result = PasswordPolicy.Validate(password);

        // Assert
        Assert.IsTrue(result.Contains("Gebruik minimaal één kleine letter."));
    }
    
    [TestMethod]
    [TestProperty("LOG-01", "AC3")]
    public void Validate_NoDigit_ReturnsDigitError()
    {
        // Arrange
        var password = "!Testwoord";

        // Act
        var result = PasswordPolicy.Validate(password);

        // Assert
        Assert.IsTrue(result.Contains("Gebruik minimaal één cijfer."));
    }
    
    [TestMethod]
    [TestProperty( "LOG-01", "AC3")]
    public void Validate_ValidPassword_ReturnsNoErrors()
    {
        // Arrange
        var password = "!Test2026";

        // Act
        var result = PasswordPolicy.Validate(password);

        // Assert
        Assert.AreEqual(0, result.Count);
    }
}