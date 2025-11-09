using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BookProject.Areas.Identity.Pages.Account;
using BookProject.Constants;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace BookProject.Tests
{
    public class RegisterModelTests
    {
        [Fact]
        public async Task OnPostAsync_UserRegistrationSuccessful_ReturnsLocalRedirect()
        {
            var mockUserEmailStore = new Mock<IUserEmailStore<IdentityUser>>();
            var mockUserManager = new Mock<UserManager<IdentityUser>>(
                mockUserEmailStore.As<IUserStore<IdentityUser>>().Object,
                null, null, null, null, null, null, null, null);

            var mockSignInManager = new Mock<SignInManager<IdentityUser>>(
                mockUserManager.Object,
                Mock.Of<IHttpContextAccessor>(),
                Mock.Of<IUserClaimsPrincipalFactory<IdentityUser>>(),
                null, null, null, null);

            var mockLogger = new Mock<ILogger<RegisterModel>>();
            var mockEmailSender = new Mock<IEmailSender>();

            mockUserManager.Setup(x => x.CreateAsync(It.IsAny<IdentityUser>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success);

            mockUserManager.Setup(x => x.AddToRoleAsync(It.IsAny<IdentityUser>(), Roles.User.ToString()))
                .ReturnsAsync(IdentityResult.Success);

            mockUserManager.Setup(x => x.GetUserIdAsync(It.IsAny<IdentityUser>()))
                .ReturnsAsync("TestUserId");

            mockUserManager.Setup(x => x.GenerateEmailConfirmationTokenAsync(It.IsAny<IdentityUser>()))
                .ReturnsAsync("TestEmailToken");

            mockSignInManager.Setup(x => x.SignInAsync(It.IsAny<IdentityUser>(), It.IsAny<bool>(), null))
                .Returns(Task.CompletedTask);

            mockUserEmailStore.Setup(x => x.SetEmailAsync(It.IsAny<IdentityUser>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            mockUserEmailStore.Setup(x => x.SetUserNameAsync(It.IsAny<IdentityUser>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var registerModel = new RegisterModel(
                mockUserManager.Object,
                mockUserEmailStore.As<IUserStore<IdentityUser>>().Object,
                mockSignInManager.Object,
                mockLogger.Object,
                mockEmailSender.Object)
            {
                Input = new RegisterModel.InputModel
                {
                    Email = "test@example.com",
                    Password = "Password123!",
                    ConfirmPassword = "Password123!"
                }
            };
            var result = await registerModel.OnPostAsync("/Home") as LocalRedirectResult;

            Assert.NotNull(result);
            Assert.Equal("/Home", result.Url);

            mockUserManager.Verify(x => x.CreateAsync(It.IsAny<IdentityUser>(), It.IsAny<string>()), Times.Once);
            mockUserManager.Verify(x => x.AddToRoleAsync(It.IsAny<IdentityUser>(), Roles.User.ToString()), Times.Once);
            mockEmailSender.Verify(x => x.SendEmailAsync(
                It.IsAny<string>(),
                "Confirm your email",
                It.Is<string>(body => body.Contains("clicking here"))
            ), Times.Once);
        }

        [Fact]
        public async Task OnPostAsync_InvalidModelState_ReturnsPage()
        {
            var mockUserEmailStore = new Mock<IUserEmailStore<IdentityUser>>();
            var mockUserManager = new Mock<UserManager<IdentityUser>>(
                mockUserEmailStore.As<IUserStore<IdentityUser>>().Object,
                null, null, null, null, null, null, null, null);

            var mockSignInManager = new Mock<SignInManager<IdentityUser>>(
                mockUserManager.Object,
                Mock.Of<IHttpContextAccessor>(),
                Mock.Of<IUserClaimsPrincipalFactory<IdentityUser>>(),
                null, null, null, null);

            var mockLogger = new Mock<ILogger<RegisterModel>>();
            var mockEmailSender = new Mock<IEmailSender>();

            var registerModel = new RegisterModel(
                mockUserManager.Object,
                mockUserEmailStore.As<IUserStore<IdentityUser>>().Object,
                mockSignInManager.Object,
                mockLogger.Object,
                mockEmailSender.Object);

            registerModel.ModelState.AddModelError("Test", "Invalid model state");
            var result = await registerModel.OnPostAsync("/Home");

            Assert.IsType<PageResult>(result);
        }
    }
}
