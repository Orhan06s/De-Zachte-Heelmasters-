using De_zachte_heelmeester.Validators;

namespace DeZachteHeelmeestersTests.Registration;

[TestClass]
public class PasswordPolicyTest
{
    [TestMethod]
    [TestProperty( "LOG-01", "AC3")]
    public void Validate_InvalidPassword_ReturnsErrors()
    {
        // Arrange
        var password = "test";

        // Act
        var result = PasswordPolicy.Validate(password);

        // Assert
        Assert.IsTrue(result.Count > 0);
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