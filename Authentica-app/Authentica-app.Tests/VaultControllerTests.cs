using Authentica.BLL.DTOs;
using Authentica.BLL.Interfaces;
using Authentica.BLL.Models;
using Authentica_app.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;

namespace Authentica_app.Tests;

[TestClass]
public class VaultControllerTests
{
    private Mock<IVaultService> _mockVaultService = null!;
    private Mock<UserManager<IdentityUser>> _mockUserManager = null!;
    private VaultController _controller = null!;
    private const string UserId = "user1";
    
    [TestInitialize]
    public void Setup()
    {
        // Maakt een mock van de VaultService, zodat er geen echte service/database nodig is.
        _mockVaultService = new Mock<IVaultService>();

        // Maakt een mock UserManager aan om de ingelogde gebruiker na te bootsen.
        var store = new Mock<IUserStore<IdentityUser>>();
#pragma warning disable CS8625
        _mockUserManager = new Mock<UserManager<IdentityUser>>(
            store.Object, null, null, null, null, null, null, null, null);
#pragma warning restore CS8625

        // Zorgt ervoor dat de controller altijd "user1" terugkrijgt als ingelogde gebruiker.
        _mockUserManager.Setup(u => u.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns(UserId);

        // Maakt de controller aan met de gemockte dependencies.
        _controller = new VaultController(_mockVaultService.Object, _mockUserManager.Object);

        // Zet een neppe ingelogde gebruiker in de HttpContext.
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(
                    new[] { new Claim(ClaimTypes.NameIdentifier, UserId) }, "mock"))
            }
        };
    }

    [TestMethod]
    public async Task Index_ReturnsOnlyCurrentUsersVaults()
    {
        // Test of de Index-pagina alleen de vaults van de ingelogde gebruiker teruggeeft.
        var vaults = new List<Vault>
        {
            new() { VaultId = 1, Name = "Persoonlijk", UserId = UserId },
            new() { VaultId = 2, Name = "Werk", UserId = UserId }
        };
        _mockVaultService.Setup(s => s.GetUserVaults(UserId)).ReturnsAsync(vaults);

        var result = await _controller.Index();

        var viewResult = result as ViewResult;
        Assert.IsNotNull(viewResult);
        Assert.AreEqual(vaults, viewResult.Model);
    }

    [TestMethod]
    public async Task Create_AddsVaultForCurrentUser()
    {
        // Test of een nieuwe vault wordt aangemaakt voor de ingelogde gebruiker.
        var dto = new VaultCreateDto { Name = "Nieuwe Vault" };
        _mockVaultService.Setup(s => s.CreateVault(UserId, It.IsAny<VaultCreateDto>())).Returns(Task.CompletedTask);

        await _controller.Create(dto);

        _mockVaultService.Verify(s => s.CreateVault(UserId, It.IsAny<VaultCreateDto>()), Times.Once);
    }

    [TestMethod]
    public async Task Edit_ReturnsNotFound_IfVaultBelongsToAnotherUser()
    {
        // Test of Edit NotFound teruggeeft als de vault niet bestaat of niet van deze gebruiker is.
        _mockVaultService.Setup(s => s.GetById(1, UserId)).ReturnsAsync((Vault?)null);

        var result = await _controller.Edit(1);

        Assert.IsInstanceOfType<NotFoundResult>(result);
    }

    [TestMethod]
    public async Task Delete_ReturnsNotFound_IfVaultBelongsToAnotherUser()
    {
        // Test of Delete NotFound teruggeeft als de vault niet bestaat of niet van deze gebruiker is.
        _mockVaultService.Setup(s => s.GetById(1, UserId)).ReturnsAsync((Vault?)null);

        var result = await _controller.Delete(1);

        Assert.IsInstanceOfType<NotFoundResult>(result);
    }

    [TestMethod]
    public async Task Delete_RedirectsToIndex_AfterSuccessfulDelete()
    {
        // Test of de gebruiker na succesvol verwijderen teruggaat naar de Index-pagina.
        var vault = new Vault { VaultId = 1, Name = "Mijn Vault", UserId = UserId };
        _mockVaultService.Setup(s => s.GetById(1, UserId)).ReturnsAsync(vault);
        _mockVaultService.Setup(s => s.DeleteVault(vault)).Returns(Task.CompletedTask);

        var result = await _controller.Delete(1);

        var redirect = result as RedirectToActionResult;
        Assert.IsNotNull(redirect);
        Assert.AreEqual("Index", redirect.ActionName);
    }
}

