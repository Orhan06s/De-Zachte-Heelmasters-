using De_zachte_heelmeester.Validators;

namespace DeZachteHeelmeestersTests.Registration;

[TestClass]
public class BsnValidatorTest
{
    [TestMethod]
    [TestProperty("LOG-01", "BSN")]
    public void IsValid_ValidBsn_ReturnsTrue()
    {
        // Arrange
        var bsn = "111222333";

        // Act
        var result = BsnValidator.IsValid(bsn);

        // Assert
        Assert.IsTrue(result);
    }
    
    [TestMethod]
    [TestProperty("LOG-01", "BSN")]
    public void IsValid_InvalidBsn_ReturnsFalse()
    {
        // Arrange
        var bsn = "123456789";

        // Act
        var result = BsnValidator.IsValid(bsn);

        // Assert
        Assert.IsFalse(result);
    }
    
    [TestMethod]
    [TestProperty("LOG-01", "BSN")]
    public void IsValid_TooShort_ReturnsFalse()
    {
        // Arrange
        var bsn = "11122233";

        // Act
        var result = BsnValidator.IsValid(bsn);

        // Assert
        Assert.IsFalse(result);
    }
    
    [TestMethod]
    [TestProperty("LOG-01", "BSN")]
    public void IsValid_ContainsLetter_ReturnsFalse()
    {
        // Arrange
        var bsn = "11122233a";

        // Act
        var result = BsnValidator.IsValid(bsn);

        // Assert
        Assert.IsFalse(result);
    }
}