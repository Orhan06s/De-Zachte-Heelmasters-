using De_zachte_heelmeester.Models;
using De_zachte_heelmeester.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DeZachteHeelmeestersTests.Authentication
{
    [TestClass]
    public class LOG05_RoleRecognitionTests
    {
        // Simple in-memory stub to avoid external mocking dependencies
        private class FakeUserRoleProvider : IUserRoleProvider
        {
            private readonly IList<string> _roles;

            public FakeUserRoleProvider(params string[] roles)
            {
                _roles = roles.ToList();
            }

            public Task<IList<string>> GetRolesAsync(User user)
            {
                return Task.FromResult(_roles);
            }
        }

        // TC-LOG-11 (AC1): Patient logs in and is routed to the patient start page
        [TestMethod]
        [TestCategory("LOG-05")]
        public async Task ResolveLoginRoles_WhenUserIsPatient_ReturnsPatientStartPage()
        {
            // Arrange
            var user = new User { Email = "patient@test.nl" };
            var service = new RoleLoginService(new FakeUserRoleProvider("Patient"));

            // Act
            var result = await service.ResolveLoginRolesAsync(user);

            // Assert
            Assert.IsTrue(result.IsSuccess);
            Assert.IsFalse(result.RequiresRoleSelection);
            Assert.AreEqual("Patient", result.ActiveRole);
            Assert.AreEqual("/patient", result.RedirectPath);
        }

        // TC-LOG-12 (AC2): Specialist logs in via the same screen and is routed to the specialist start page
        [TestMethod]
        [TestCategory("LOG-05")]
        public async Task ResolveLoginRoles_WhenUserIsSpecialist_ReturnsSpecialistStartPage()
        {
            // Arrange
            var user = new User { Email = "specialist@test.nl" };
            var service = new RoleLoginService(new FakeUserRoleProvider("Specialist"));

            // Act
            var result = await service.ResolveLoginRolesAsync(user);

            // Assert
            Assert.IsTrue(result.IsSuccess);
            Assert.IsFalse(result.RequiresRoleSelection);
            Assert.AreEqual("Specialist", result.ActiveRole);
            Assert.AreEqual("/specialist", result.RedirectPath);
        }

        // TC-LOG-13 (AC3): User with multiple roles must choose a role before continuing
        [TestMethod]
        [TestCategory("LOG-05")]
        public async Task ResolveLoginRoles_WhenUserHasMultipleRoles_RequiresRoleSelection()
        {
            // Arrange
            var user = new User { Email = "dubbel@test.nl" };
            var service = new RoleLoginService(new FakeUserRoleProvider("Specialist", "Patient"));

            // Act
            var result = await service.ResolveLoginRolesAsync(user);

            // Assert
            Assert.IsTrue(result.IsSuccess);
            Assert.IsTrue(result.RequiresRoleSelection);
            Assert.AreEqual(2, result.AvailableRoles.Count);
            Assert.AreEqual("/rol-kiezen", result.RedirectPath);
        }

        // TC-LOG-13b (AC3): Selecting a valid assigned role routes to that role's start page
        [TestMethod]
        [TestCategory("LOG-05")]
        public async Task SelectRole_WhenUserHasChosenRole_ReturnsSelectedRoleStartPage()
        {
            // Arrange
            var user = new User { Email = "dubbel@test.nl" };
            var service = new RoleLoginService(new FakeUserRoleProvider("Specialist", "Patient"));

            // Act
            var result = await service.SelectRoleAsync(user, "Specialist");

            // Assert
            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual("Specialist", result.ActiveRole);
            Assert.AreEqual("/specialist", result.RedirectPath);
        }

        // TC-LOG-14 (AC4): Account without any role is denied login with a clear error message
        [TestMethod]
        [TestCategory("LOG-05")]
        public async Task ResolveLoginRoles_WhenUserHasNoRole_FailsWithRoleError()
        {
            // Arrange
            var user = new User { Email = "zonderrol@test.nl" };
            var service = new RoleLoginService(new FakeUserRoleProvider());

            // Act
            var result = await service.ResolveLoginRolesAsync(user);

            // Assert
            Assert.IsFalse(result.IsSuccess);
            Assert.IsNull(result.ActiveRole);
            Assert.IsNotNull(result.ErrorMessage);
            Assert.IsTrue(result.ErrorMessage.Contains("rol", StringComparison.OrdinalIgnoreCase));
        }
    }
}