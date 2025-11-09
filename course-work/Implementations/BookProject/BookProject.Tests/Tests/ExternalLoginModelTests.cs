using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BookProject.Areas.Identity.Pages.Account.Manage;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Moq;
using Xunit;

namespace BookProject.Tests
{
    public class ExternalLoginsModelTests
    {
        [Fact]
        public async Task OnGetAsync_UserExists_LoadsLoginsAndAuthenticationSchemes()
        {
            var mockUserManager = new Mock<UserManager<IdentityUser>>(
                Mock.Of<IUserStore<IdentityUser>>(),
                null, null, null, null, null, null, null, null);
            var mockSignInManager = new Mock<SignInManager<IdentityUser>>(
                mockUserManager.Object,
                Mock.Of<IHttpContextAccessor>(),
                Mock.Of<IUserClaimsPrincipalFactory<IdentityUser>>(),
                null, null, null, null);

            var user = new IdentityUser();
            var logins = new List<UserLoginInfo> { new UserLoginInfo("Provider1", "Key1", "Display1") };
            var schemes = new List<AuthenticationScheme>
            {
                new AuthenticationScheme("Provider1", "Provider1", typeof(IAuthenticationHandler)),
                new AuthenticationScheme("Provider2", "Provider2", typeof(IAuthenticationHandler))
            };

            mockUserManager.Setup(x => x.GetUserAsync(It.IsAny<System.Security.Claims.ClaimsPrincipal>())).ReturnsAsync(user);
            mockUserManager.Setup(x => x.GetLoginsAsync(user)).ReturnsAsync(logins);
            mockSignInManager.Setup(x => x.GetExternalAuthenticationSchemesAsync()).ReturnsAsync(schemes);

            var model = new ExternalLoginsModel(mockUserManager.Object, mockSignInManager.Object, Mock.Of<IUserStore<IdentityUser>>())
            {
                PageContext = new PageContext { HttpContext = new DefaultHttpContext { User = new System.Security.Claims.ClaimsPrincipal() } }
            };

            var result = await model.OnGetAsync();

            Assert.IsType<PageResult>(result);
            Assert.Single(model.CurrentLogins);
            Assert.Single(model.OtherLogins);
            Assert.Equal("Provider1", model.CurrentLogins[0].LoginProvider);
            Assert.Equal("Provider2", model.OtherLogins[0].Name);
        }

        [Fact]
        public async Task OnGetAsync_UserNotFound_ReturnsNotFound()
        {
            var mockUserManager = new Mock<UserManager<IdentityUser>>(
                Mock.Of<IUserStore<IdentityUser>>(),
                null, null, null, null, null, null, null, null);
            var mockSignInManager = new Mock<SignInManager<IdentityUser>>(
                mockUserManager.Object,
                Mock.Of<IHttpContextAccessor>(),
                Mock.Of<IUserClaimsPrincipalFactory<IdentityUser>>(),
                null, null, null, null);

            mockUserManager.Setup(x => x.GetUserAsync(It.IsAny<System.Security.Claims.ClaimsPrincipal>())).ReturnsAsync((IdentityUser)null);

            var model = new ExternalLoginsModel(mockUserManager.Object, mockSignInManager.Object, Mock.Of<IUserStore<IdentityUser>>())
            {
                PageContext = new PageContext { HttpContext = new DefaultHttpContext { User = new System.Security.Claims.ClaimsPrincipal() } }
            };

            var result = await model.OnGetAsync();

            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task OnPostRemoveLoginAsync_LoginRemovedSuccessfully_ReturnsRedirect()
        {
            var mockUserManager = new Mock<UserManager<IdentityUser>>(
                Mock.Of<IUserStore<IdentityUser>>(),
                null, null, null, null, null, null, null, null);
            var mockSignInManager = new Mock<SignInManager<IdentityUser>>(
                mockUserManager.Object,
                Mock.Of<IHttpContextAccessor>(),
                Mock.Of<IUserClaimsPrincipalFactory<IdentityUser>>(),
                null, null, null, null);

            var user = new IdentityUser();

            mockUserManager.Setup(x => x.GetUserAsync(It.IsAny<System.Security.Claims.ClaimsPrincipal>())).ReturnsAsync(user);
            mockUserManager.Setup(x => x.RemoveLoginAsync(user, "Provider", "Key")).ReturnsAsync(IdentityResult.Success);
            mockSignInManager.Setup(x => x.RefreshSignInAsync(user)).Returns(Task.CompletedTask);

            var model = new ExternalLoginsModel(mockUserManager.Object, mockSignInManager.Object, Mock.Of<IUserStore<IdentityUser>>())
            {
                PageContext = new PageContext { HttpContext = new DefaultHttpContext { User = new System.Security.Claims.ClaimsPrincipal() } }
            };

            var result = await model.OnPostRemoveLoginAsync("Provider", "Key");

            var redirectResult = Assert.IsType<RedirectToPageResult>(result);
            Assert.Null(model.StatusMessage);
        }

        [Fact]
        public async Task OnPostRemoveLoginAsync_LoginRemovalFails_ReturnsRedirectWithError()
        {
            var mockUserManager = new Mock<UserManager<IdentityUser>>(
                Mock.Of<IUserStore<IdentityUser>>(),
                null, null, null, null, null, null, null, null);
            var mockSignInManager = new Mock<SignInManager<IdentityUser>>(
                mockUserManager.Object,
                Mock.Of<IHttpContextAccessor>(),
                Mock.Of<IUserClaimsPrincipalFactory<IdentityUser>>(),
                null, null, null, null);

            var user = new IdentityUser();

            mockUserManager.Setup(x => x.GetUserAsync(It.IsAny<System.Security.Claims.ClaimsPrincipal>())).ReturnsAsync(user);
            mockUserManager.Setup(x => x.RemoveLoginAsync(user, "Provider", "Key")).ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Removal failed." }));

            var model = new ExternalLoginsModel(mockUserManager.Object, mockSignInManager.Object, Mock.Of<IUserStore<IdentityUser>>())
            {
                PageContext = new PageContext { HttpContext = new DefaultHttpContext { User = new System.Security.Claims.ClaimsPrincipal() } }
            };

            var result = await model.OnPostRemoveLoginAsync("Provider", "Key");

            var redirectResult = Assert.IsType<RedirectToPageResult>(result);
            Assert.Equal("The external login was not removed.", model.StatusMessage);
        }

        [Fact]
        public async Task OnPostLinkLoginAsync_RedirectsToChallengeResult()
        {
            var mockUserManager = new Mock<UserManager<IdentityUser>>(
                Mock.Of<IUserStore<IdentityUser>>(),
                null, null, null, null, null, null, null, null);
            var mockSignInManager = new Mock<SignInManager<IdentityUser>>(
                mockUserManager.Object,
                Mock.Of<IHttpContextAccessor>(),
                Mock.Of<IUserClaimsPrincipalFactory<IdentityUser>>(),
                null, null, null, null);

            mockUserManager.Setup(x => x.GetUserId(It.IsAny<System.Security.Claims.ClaimsPrincipal>())).Returns("UserId");

            var model = new ExternalLoginsModel(mockUserManager.Object, mockSignInManager.Object, Mock.Of<IUserStore<IdentityUser>>())
            {
                PageContext = new PageContext { HttpContext = new DefaultHttpContext { User = new System.Security.Claims.ClaimsPrincipal() } }
            };

            var result = await model.OnPostLinkLoginAsync("Provider");

            var challengeResult = Assert.IsType<ChallengeResult>(result);
        }

        [Fact]
        public async Task OnGetLinkLoginCallbackAsync_LoginAddedSuccessfully_ReturnsRedirect()
        {
            var mockUserManager = new Mock<UserManager<IdentityUser>>(
                Mock.Of<IUserStore<IdentityUser>>(),
                null, null, null, null, null, null, null, null);
            var mockSignInManager = new Mock<SignInManager<IdentityUser>>(
                mockUserManager.Object,
                Mock.Of<IHttpContextAccessor>(),
                Mock.Of<IUserClaimsPrincipalFactory<IdentityUser>>(),
                null, null, null, null);

            var user = new IdentityUser();
            var loginInfo = new ExternalLoginInfo(new System.Security.Claims.ClaimsPrincipal(), "Provider", "Key", "Display");

            mockUserManager.Setup(x => x.GetUserAsync(It.IsAny<System.Security.Claims.ClaimsPrincipal>())).ReturnsAsync(user);
            mockUserManager.Setup(x => x.GetUserIdAsync(user)).ReturnsAsync("UserId");
            mockSignInManager.Setup(x => x.GetExternalLoginInfoAsync("UserId")).ReturnsAsync(loginInfo);
            mockUserManager.Setup(x => x.AddLoginAsync(user, loginInfo)).ReturnsAsync(IdentityResult.Success);

            var model = new ExternalLoginsModel(mockUserManager.Object, mockSignInManager.Object, Mock.Of<IUserStore<IdentityUser>>())
            {
                PageContext = new PageContext { HttpContext = new DefaultHttpContext { User = new System.Security.Claims.ClaimsPrincipal() } }
            };

            var result = await model.OnGetLinkLoginCallbackAsync();

            var redirectResult = Assert.IsType<RedirectToPageResult>(result);
            Assert.Equal("The external login was added.", model.StatusMessage);
        }
    }
}
