using System;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;
using BookProject.Areas.Identity.Pages.Account.Manage;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;
using Xunit;

namespace BookProject.Tests
{
    public class EmailModelTests
    {
        [Fact]
        public async Task OnGetAsync_UserLoadedSuccessfully_ReturnsPage()
        {
            var mockUserEmailStore = new Mock<IUserEmailStore<IdentityUser>>();
            var mockUserManager = new Mock<UserManager<IdentityUser>>(
                mockUserEmailStore.Object,
                null, null, null, null, null, null, null, null);
            var mockSignInManager = new Mock<SignInManager<IdentityUser>>(
                mockUserManager.Object,
                Mock.Of<IHttpContextAccessor>(),
                Mock.Of<IUserClaimsPrincipalFactory<IdentityUser>>(),
                null, null, null, null);
            var mockEmailSender = new Mock<IEmailSender>();

            mockUserManager.Setup(x => x.GetUserAsync(It.IsAny<System.Security.Claims.ClaimsPrincipal>()))
                .ReturnsAsync(new IdentityUser { Email = "test@example.com" });
            mockUserManager.Setup(x => x.GetEmailAsync(It.IsAny<IdentityUser>()))
                .ReturnsAsync("test@example.com");
            mockUserManager.Setup(x => x.IsEmailConfirmedAsync(It.IsAny<IdentityUser>()))
                .ReturnsAsync(true);

            var emailModel = new EmailModel(mockUserManager.Object, mockSignInManager.Object, mockEmailSender.Object)
            {
                TempData = new Mock<ITempDataDictionary>().Object
            };

            var result = await emailModel.OnGetAsync();

            Assert.IsType<PageResult>(result);
            Assert.Equal("test@example.com", emailModel.Email);
            Assert.True(emailModel.IsEmailConfirmed);
        }

        [Fact]
        public async Task OnPostChangeEmailAsync_ValidInput_SendsConfirmationEmail()
        {
            var mockUserEmailStore = new Mock<IUserEmailStore<IdentityUser>>();
            var mockUserManager = new Mock<UserManager<IdentityUser>>(
                mockUserEmailStore.Object,
                null, null, null, null, null, null, null, null);
            var mockSignInManager = new Mock<SignInManager<IdentityUser>>(
                mockUserManager.Object,
                Mock.Of<IHttpContextAccessor>(),
                Mock.Of<IUserClaimsPrincipalFactory<IdentityUser>>(),
                null, null, null, null);
            var mockEmailSender = new Mock<IEmailSender>();

            var user = new IdentityUser { Email = "test@example.com" };
            var httpContext = new DefaultHttpContext
            {
                User = new System.Security.Claims.ClaimsPrincipal()
            };

            mockUserManager.Setup(x => x.GetUserAsync(It.IsAny<System.Security.Claims.ClaimsPrincipal>()))
                .ReturnsAsync(user);
            mockUserManager.Setup(x => x.GetEmailAsync(It.IsAny<IdentityUser>()))
                .ReturnsAsync("old@example.com");
            mockUserManager.Setup(x => x.GenerateChangeEmailTokenAsync(It.IsAny<IdentityUser>(), It.IsAny<string>()))
                .ReturnsAsync("TestToken");

            var emailModel = new EmailModel(mockUserManager.Object, mockSignInManager.Object, mockEmailSender.Object)
            {
                Input = new EmailModel.InputModel { NewEmail = "new@example.com" },
                TempData = new Mock<ITempDataDictionary>().Object
            };

            emailModel.PageContext = new PageContext { HttpContext = httpContext };

            var result = await emailModel.OnPostChangeEmailAsync();

            Assert.IsType<RedirectToPageResult>(result);
            mockEmailSender.Verify(x => x.SendEmailAsync(
                "new@example.com",
                "Confirm your email",
                It.Is<string>(s => s.Contains("clicking here"))),
                Times.Once);
        }
        [Fact]
        public async Task OnPostSendVerificationEmailAsync_SendsVerificationEmail()
        {
            var mockUserEmailStore = new Mock<IUserEmailStore<IdentityUser>>();
            var mockUserManager = new Mock<UserManager<IdentityUser>>(
                mockUserEmailStore.Object,
                null, null, null, null, null, null, null, null);
            var mockSignInManager = new Mock<SignInManager<IdentityUser>>(
                mockUserManager.Object,
                Mock.Of<IHttpContextAccessor>(),
                Mock.Of<IUserClaimsPrincipalFactory<IdentityUser>>(),
                null, null, null, null);
            var mockEmailSender = new Mock<IEmailSender>();

            var user = new IdentityUser { Email = "test@example.com" };
            var httpContext = new DefaultHttpContext
            {
                User = new System.Security.Claims.ClaimsPrincipal()
            };

            mockUserManager.Setup(x => x.GetUserAsync(It.IsAny<System.Security.Claims.ClaimsPrincipal>()))
                .ReturnsAsync(user);
            mockUserManager.Setup(x => x.GetEmailAsync(It.IsAny<IdentityUser>()))
                .ReturnsAsync("test@example.com");
            mockUserManager.Setup(x => x.GenerateEmailConfirmationTokenAsync(It.IsAny<IdentityUser>()))
                .ReturnsAsync("TestToken");

            var emailModel = new EmailModel(mockUserManager.Object, mockSignInManager.Object, mockEmailSender.Object)
            {
                TempData = new Mock<ITempDataDictionary>().Object
            };

            emailModel.PageContext = new PageContext { HttpContext = httpContext };

            var result = await emailModel.OnPostSendVerificationEmailAsync();

            Assert.IsType<RedirectToPageResult>(result);
            mockEmailSender.Verify(x => x.SendEmailAsync(
                "test@example.com",
                "Confirm your email",
                It.Is<string>(s => s.Contains("clicking here"))),
                Times.Once);
        }
    }
}