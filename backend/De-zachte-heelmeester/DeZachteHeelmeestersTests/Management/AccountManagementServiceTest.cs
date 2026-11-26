using Microsoft.VisualStudio.TestTools.UnitTesting;
using De_zachte_heelmeester.Models;
using De_zachte_heelmeester.Repositories;
using De_zachte_heelmeester.Services;
using Moq;

namespace DeZachteHeelmeestersTests.Management;

[TestClass]
public class AccountManagementServiceTest
{
    private Mock<IUserRepository> _mockUserRepository = null!;
    private Mock<IEmployeeRepository> _mockEmployeeRepository = null!;
    private AccountManagementService _service = null!;

    [TestInitialize]
    public void Setup()
    {
        _mockUserRepository = new Mock<IUserRepository>();
        _mockEmployeeRepository = new Mock<IEmployeeRepository>();
        _service = new AccountManagementService(_mockUserRepository.Object, _mockEmployeeRepository.Object);
    }

    [TestMethod]
    [TestProperty("BEH-01", "AC1")]
    public async Task CreateEmployeeAccountAsync_ValidGpAccount_CreatesAccountWithGpRole()
    {
        // Arrange
        var request = new CreateAccountRequest
        {
            Name = "Dr. Bakker",
            Email = "huisarts@praktijk.nl",
            Password = "!Test2026",
            Role = "Huisarts"
        };

        _mockUserRepository.Setup(r => r.GetByEmailAsync(request.Email)).ReturnsAsync((User?)null);
        _mockUserRepository.Setup(r => r.CreateAsync(request.Email, request.Password)).ReturnsAsync("user-gp-1");

        // Act
        var errors = await _service.CreateEmployeeAccountAsync(request);

        // Assert
        Assert.AreEqual(0, errors.Count);
        _mockUserRepository.Verify(r => r.AddToRoleAsync("user-gp-1", "Huisarts"), Times.Once);
        _mockEmployeeRepository.Verify(r => r.AddAsync(It.Is<Employee>(e => e.UserId == "user-gp-1")), Times.Once);
    }

    [TestMethod]
    [TestProperty("BEH-01", "AC2")]
    public async Task CreateEmployeeAccountAsync_SpecialistWithMultipleSpecializations_SavesAllSpecializations()
    {
        // Arrange
        var request = new CreateAccountRequest
        {
            Name = "Dr. De Vries",
            Email = "devries@ziekenhuis.nl",
            Password = "!Test2026",
            Role = "Specialist",
            Specializations = new List<string> { "Cardiologie", "Interne Geneeskunde" }
        };

        _mockUserRepository.Setup(r => r.GetByEmailAsync(request.Email)).ReturnsAsync((User?)null);
        _mockUserRepository.Setup(r => r.CreateAsync(request.Email, request.Password)).ReturnsAsync("user-spec-1");

        // Act
        var errors = await _service.CreateEmployeeAccountAsync(request);

        // Assert
        Assert.AreEqual(0, errors.Count);
        _mockUserRepository.Verify(r => r.AddToRoleAsync("user-spec-1", "Specialist"), Times.Once);
        _mockEmployeeRepository.Verify(r => r.AddAsync(It.Is<Employee>(e =>
            e.UserId == "user-spec-1" &&
            e.Specialization.Contains("Cardiologie") &&
            e.Specialization.Contains("Interne Geneeskunde"))), Times.Once);
    }

    [TestMethod]
    [TestProperty("BEH-01", "AC3")]
    public async Task ChangeUserRoleAsync_AdminStaffToSpecialist_ReplacesOldRoleWithSpecialist()
    {
        // Arrange
        var user = new User { Id = "user-emp-1", Email = "medewerker@ziekenhuis.nl" };
        var existingEmployee = new Employee { EmployeeId = 5, UserId = "user-emp-1", Specialization = "AdministratiefMedewerker" };

        _mockUserRepository.Setup(r => r.GetByIdAsync("user-emp-1")).ReturnsAsync(user);
        _mockUserRepository.Setup(r => r.GetRolesAsync("user-emp-1")).ReturnsAsync(new List<string> { "AdministratiefMedewerker" });
        _mockEmployeeRepository.Setup(r => r.GetByUserIdAsync("user-emp-1")).ReturnsAsync(existingEmployee);

        // Act
        var errors = await _service.ChangeUserRoleAsync("user-emp-1", "Specialist", new List<string> { "Neurologie" });

        // Assert
        Assert.AreEqual(0, errors.Count);
        _mockUserRepository.Verify(r => r.RemoveFromRolesAsync("user-emp-1", It.Is<IEnumerable<string>>(roles => roles.Contains("AdministratiefMedewerker"))), Times.Once);
        _mockUserRepository.Verify(r => r.AddToRoleAsync("user-emp-1", "Specialist"), Times.Once);
        _mockEmployeeRepository.Verify(r => r.UpdateAsync(It.Is<Employee>(e => e.Specialization == "Neurologie")), Times.Once);
    }

    [TestMethod]
    [TestProperty("BEH-01", "AC4")]
    public async Task CreateEmployeeAccountAsync_ExistingEmail_ReturnsErrorAndCreatesNoSecondAccount()
    {
        // Arrange
        var request = new CreateAccountRequest
        {
            Name = "Dr. Dubbel",
            Email = "bestaat@ziekenhuis.nl",
            Password = "!Test2026",
            Role = "Huisarts"
        };

        _mockUserRepository.Setup(r => r.GetByEmailAsync("bestaat@ziekenhuis.nl")).ReturnsAsync(new User { Id = "bestaand-1", Email = "bestaat@ziekenhuis.nl" });

        // Act
        var errors = await _service.CreateEmployeeAccountAsync(request);

        // Assert
        Assert.IsTrue(errors.Count > 0);
        _mockUserRepository.Verify(r => r.CreateAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [TestMethod]
    [TestProperty("BEH-01", "AC5")]
    public async Task ChangeUserRoleAsync_ExistingPatientBecomesSpecialist_KeepsPatientRoleAndAddsSpecialist()
    {
        // Arrange
        var patientUser = new User { Id = "user-pat-1", Email = "patient.arts@ziekenhuis.nl" };
        _mockUserRepository.Setup(r => r.GetByIdAsync("user-pat-1")).ReturnsAsync(patientUser);
        _mockUserRepository.Setup(r => r.GetRolesAsync("user-pat-1")).ReturnsAsync(new List<string> { "Patient" });
        _mockEmployeeRepository.Setup(r => r.GetByUserIdAsync("user-pat-1")).ReturnsAsync((Employee?)null);

        // Act
        var errors = await _service.ChangeUserRoleAsync("user-pat-1", "Specialist", new List<string> { "Chirurgie" });

        // Assert
        Assert.AreEqual(0, errors.Count);
        _mockUserRepository.Verify(r => r.RemoveFromRolesAsync(It.IsAny<string>(), It.IsAny<IEnumerable<string>>()), Times.Never);
        _mockUserRepository.Verify(r => r.AddToRoleAsync("user-pat-1", "Specialist"), Times.Once);
        _mockEmployeeRepository.Verify(r => r.AddAsync(It.Is<Employee>(e => e.UserId == "user-pat-1")), Times.Once);
    }
}