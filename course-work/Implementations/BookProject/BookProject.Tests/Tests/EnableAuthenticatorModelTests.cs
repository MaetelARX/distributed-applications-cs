using System;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using BookProject.Areas.Identity.Pages.Account.Manage;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace BookProject.Tests
{
    public class EnableAuthenticatorModelTests
    {
        [Fact]
        public async Task OnGetAsync_UserExists_LoadsSharedKeyAndQrCodeUri()
        {
            var mockUserManager = new Mock<UserManager<IdentityUser>>(
                Mock.Of<IUserStore<IdentityUser>>(),
                null, null, null, null, null, null, null, null);
            var mockLogger = new Mock<ILogger<EnableAuthenticatorModel>>();
            var mockUrlEncoder = new Mock<UrlEncoder>();

            var user = new IdentityUser { Email = "test@example.com" };
            mockUserManager.Setup(x => x.GetUserAsync(It.IsAny<System.Security.Claims.ClaimsPrincipal>())).ReturnsAsync(user);
            mockUserManager.Setup(x => x.GetAuthenticatorKeyAsync(user)).ReturnsAsync("test-key");

            var model = new EnableAuthenticatorModel(mockUserManager.Object, mockLogger.Object, mockUrlEncoder.Object)
            {
                PageContext = new PageContext { HttpContext = new DefaultHttpContext { User = new System.Security.Claims.ClaimsPrincipal() } }
            };

            var result = await model.OnGetAsync();

            Assert.IsType<PageResult>(result);
            Assert.Equal("test-key", model.SharedKey);
            Assert.NotNull(model.AuthenticatorUri);
        }

        [Fact]
        public async Task OnGetAsync_UserNotFound_ReturnsNotFound()
        {
            var mockUserManager = new Mock<UserManager<IdentityUser>>(
                Mock.Of<IUserStore<IdentityUser>>(),
                null, null, null, null, null, null, null, null);
            var mockLogger = new Mock<ILogger<EnableAuthenticatorModel>>();
            var mockUrlEncoder = new Mock<UrlEncoder>();

            mockUserManager.Setup(x => x.GetUserAsync(It.IsAny<System.Security.Claims.ClaimsPrincipal>())).ReturnsAsync((IdentityUser)null);

            var model = new EnableAuthenticatorModel(mockUserManager.Object, mockLogger.Object, mockUrlEncoder.Object)
            {
                PageContext = new PageContext { HttpContext = new DefaultHttpContext { User = new System.Security.Claims.ClaimsPrincipal() } }
            };

            var result = await model.OnGetAsync();

            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task OnPostAsync_InvalidModelState_ReturnsPage()
        {
            var mockUserManager = new Mock<UserManager<IdentityUser>>(
                Mock.Of<IUserStore<IdentityUser>>(),
                null, null, null, null, null, null, null, null);
            var mockLogger = new Mock<ILogger<EnableAuthenticatorModel>>();
            var mockUrlEncoder = new Mock<UrlEncoder>();

            var user = new IdentityUser { Email = "test@example.com" };
            mockUserManager.Setup(x => x.GetUserAsync(It.IsAny<System.Security.Claims.ClaimsPrincipal>())).ReturnsAsync(user);

            var model = new EnableAuthenticatorModel(mockUserManager.Object, mockLogger.Object, mockUrlEncoder.Object)
            {
                Input = new EnableAuthenticatorModel.InputModel { Code = "123456" },
                PageContext = new PageContext { HttpContext = new DefaultHttpContext { User = new System.Security.Claims.ClaimsPrincipal() } }
            };

            model.ModelState.AddModelError("Input.Code", "Invalid code");

            var result = await model.OnPostAsync();

            Assert.IsType<PageResult>(result);
        }

        [Fact]
        public async Task OnPostAsync_VerificationCodeInvalid_ReturnsPageWithError()
        {
            var mockUserManager = new Mock<UserManager<IdentityUser>>(
                Mock.Of<IUserStore<IdentityUser>>(),
                null, null, null, null, null, null, null, null);
            var mockLogger = new Mock<ILogger<EnableAuthenticatorModel>>();
            var mockUrlEncoder = new Mock<UrlEncoder>();

            var user = new IdentityUser { Email = "test@example.com" };
            mockUserManager.Setup(x => x.GetUserAsync(It.IsAny<System.Security.Claims.ClaimsPrincipal>())).ReturnsAsync(user);
            mockUserManager.Setup(x => x.VerifyTwoFactorTokenAsync(user, It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(false);

            var model = new EnableAuthenticatorModel(mockUserManager.Object, mockLogger.Object, mockUrlEncoder.Object)
            {
                Input = new EnableAuthenticatorModel.InputModel { Code = "123456" },
                PageContext = new PageContext { HttpContext = new DefaultHttpContext { User = new System.Security.Claims.ClaimsPrincipal() } }
            };

            var result = await model.OnPostAsync();

            Assert.IsType<PageResult>(result);
            Assert.True(model.ModelState.ContainsKey("Input.Code"));
            Assert.Contains("Verification code is invalid.", model.ModelState["Input.Code"].Errors[0].ErrorMessage);
        }

        [Fact]
        public async Task OnPostAsync_VerificationCodeValid_GeneratesRecoveryCodesAndRedirects()
        {
            var mockUserManager = new Mock<UserManager<IdentityUser>>(
                Mock.Of<IUserStore<IdentityUser>>(),
                null, null, null, null, null, null, null, null);
            var mockLogger = new Mock<ILogger<EnableAuthenticatorModel>>();
            var mockUrlEncoder = new Mock<UrlEncoder>();

            var user = new IdentityUser { Email = "test@example.com" };
            mockUserManager.Setup(x => x.GetUserAsync(It.IsAny<System.Security.Claims.ClaimsPrincipal>())).ReturnsAsync(user);
            mockUserManager.Setup(x => x.VerifyTwoFactorTokenAsync(user, It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(true);
            mockUserManager.Setup(x => x.CountRecoveryCodesAsync(user)).ReturnsAsync(0);
            mockUserManager.Setup(x => x.GenerateNewTwoFactorRecoveryCodesAsync(user, 10)).ReturnsAsync(new[] { "code1", "code2" });

            var model = new EnableAuthenticatorModel(mockUserManager.Object, mockLogger.Object, mockUrlEncoder.Object)
            {
                Input = new EnableAuthenticatorModel.InputModel { Code = "123456" },
                PageContext = new PageContext { HttpContext = new DefaultHttpContext { User = new System.Security.Claims.ClaimsPrincipal() } }
            };

            var result = await model.OnPostAsync();

            var redirectResult = Assert.IsType<RedirectToPageResult>(result);
            Assert.Equal("./ShowRecoveryCodes", redirectResult.PageName);
            Assert.NotNull(model.RecoveryCodes);
            Assert.Contains("code1", model.RecoveryCodes);
            Assert.Contains("code2", model.RecoveryCodes);
        }
    }
}
