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
        _mockVaultService = new Mock<IVaultService>();

        var store = new Mock<IUserStore<IdentityUser>>();
#pragma warning disable CS8625
        _mockUserManager = new Mock<UserManager<IdentityUser>>(
            store.Object, null, null, null, null, null, null, null, null);
#pragma warning restore CS8625

        _mockUserManager.Setup(u => u.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns(UserId);

        _controller = new VaultController(_mockVaultService.Object, _mockUserManager.Object);
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
        var dto = new VaultCreateDto { Name = "Nieuwe Vault" };
        _mockVaultService.Setup(s => s.CreateVault(UserId, It.IsAny<VaultCreateDto>())).Returns(Task.CompletedTask);

        await _controller.Create(dto);

        _mockVaultService.Verify(s => s.CreateVault(UserId, It.IsAny<VaultCreateDto>()), Times.Once);
    }

    [TestMethod]
    public async Task Edit_ReturnsNotFound_IfVaultBelongsToAnotherUser()
    {
        _mockVaultService.Setup(s => s.GetById(1, UserId)).ReturnsAsync((Vault?)null);

        var result = await _controller.Edit(1);

        Assert.IsInstanceOfType<NotFoundResult>(result);
    }

    [TestMethod]
    public async Task Delete_ReturnsNotFound_IfVaultBelongsToAnotherUser()
    {
        _mockVaultService.Setup(s => s.GetById(1, UserId)).ReturnsAsync((Vault?)null);

        var result = await _controller.Delete(1);

        Assert.IsInstanceOfType<NotFoundResult>(result);
    }

    [TestMethod]
    public async Task Delete_RedirectsToIndex_AfterSuccessfulDelete()
    {
        var vault = new Vault { VaultId = 1, Name = "Mijn Vault", UserId = UserId };
        _mockVaultService.Setup(s => s.GetById(1, UserId)).ReturnsAsync(vault);
        _mockVaultService.Setup(s => s.DeleteVault(vault)).Returns(Task.CompletedTask);

        var result = await _controller.Delete(1);

        var redirect = result as RedirectToActionResult;
        Assert.IsNotNull(redirect);
        Assert.AreEqual("Index", redirect.ActionName);
    }
}
