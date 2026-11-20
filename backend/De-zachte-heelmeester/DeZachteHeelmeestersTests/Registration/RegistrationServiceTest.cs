using De_zachte_heelmeester.Data;
using De_zachte_heelmeester.Models;
using De_zachte_heelmeester.Repositories;
using De_zachte_heelmeester.Services;
using Microsoft.AspNetCore.Identity;
using Moq;

namespace DeZachteHeelmeestersTests.Registration;

[TestClass]
public class RegistrationServiceTest
{
    private Mock<IPatientRepository> _mockPatientRepository = null!;
    private Mock<IUserRepository> _mockUserRepository = null!;
    private RegistrationService _registrationService = null!;

    [TestInitialize]
    public void Setup()
    {
        _mockPatientRepository = new Mock<IPatientRepository>();
        _mockUserRepository = new Mock<IUserRepository>();
        _registrationService = new RegistrationService(_mockPatientRepository.Object, _mockUserRepository.Object);
    }
    
    [TestMethod]
    [TestProperty("LOG-01", "AC1")]
    public async Task RegisterAsync_NewBsn_CreatesAccountAndPatient()
    {
        // Arrange
        var request = new RegisterRequest { Bsn = "111222333", Email = "test1@test.nl", Password = "!Test2026" };
        _mockPatientRepository.Setup(r => r.GetByBsnAsync("111222333")).ReturnsAsync((Patient?)null);
        
        // Act
        var result = await _registrationService.RegisterAsync(request);

        // Assert
        Assert.AreEqual(0, result.Count);
        _mockUserRepository.Verify(r => r.CreateAsync("test1@test.nl", "!Test2026"), Times.Once);
        _mockPatientRepository.Verify(r => r.AddAsync(It.IsAny<Patient>()), Times.Once);
    }
    
    [TestMethod]
    [TestProperty("LOG-01", "AC2")]
    public async Task RegisterAsync_PatientWithoutAccount_LinksAccount()
    {
        // Arrange
        var request = new RegisterRequest { Bsn = "111222333", Email = "test1@test.nl", Password = "!Test2026" };
        var patient = new Patient { BSN = "111222333", UserId = null };
        _mockPatientRepository.Setup(r => r.GetByBsnAsync("111222333")).ReturnsAsync(patient);

        // Act
        var result = await _registrationService.RegisterAsync(request);

        // Assert
        Assert.AreEqual(0, result.Count);
        _mockPatientRepository.Verify(r => r.UpdateAsync(It.IsAny<Patient>()), Times.Never);
        _mockPatientRepository.Verify(r => r.AddAsync(It.IsAny<Patient>()), Times.Never);
        _mockUserRepository.Verify(r => r.CreateAsync("test1@test.nl", "!Test2026"), Times.Once);
    }
    
    [TestMethod]
    [TestProperty("LOG-01", "AC3")]
    public async Task RegisterAsync_InvalidPassword_CreatesNoAccount()
    {
        // Arrange
        var request = new RegisterRequest { Bsn = "111222333", Email = "test1@test.nl", Password = "test" };

        // Act
        var result = await _registrationService.RegisterAsync(request);

        // Assert
        Assert.IsTrue(result.Count > 0);
        _mockUserRepository.Verify(r => r.CreateAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }
    
    [TestMethod]
    [TestProperty("LOG-01", "AC4")]
    public async Task RegisterAsync_BsnAlreadyHasAccount_ReturnsError()
    {
        // Arrange
        var request = new RegisterRequest { Bsn = "111222333", Email = "test1@test.nl", Password = "!Test2026" };
        var patient = new Patient { BSN = "111222333", UserId = "user-1" };
        _mockPatientRepository.Setup(r => r.GetByBsnAsync("111222333")).ReturnsAsync(patient);

        // Act
        var result = await _registrationService.RegisterAsync(request);

        // Assert
        Assert.IsTrue(result.Count > 0);
        _mockUserRepository.Verify(r => r.CreateAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }
    
    [TestMethod]
    [TestProperty("LOG-01", "BSN")]
    public async Task RegisterAsync_InvalidBsn_ReturnsError()
    {
        // Arrange
        var request = new RegisterRequest { Bsn = "123456789", Email = "test1@test.nl", Password = "!Test2026" };

        // Act
        var result = await _registrationService.RegisterAsync(request);

        // Assert
        Assert.IsTrue(result.Count > 0);
        _mockUserRepository.Verify(r => r.CreateAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }
}