using System.Threading.Tasks;
using BookProject.Areas.Identity.Pages.Account.Manage;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Moq;
using Xunit;

namespace BookProject.Tests
{
    public class IndexModelTests
    {
        [Fact]
        public async Task OnGetAsync_UserExists_LoadsUserData()
        {
            var mockUserManager = new Mock<UserManager<IdentityUser>>(
                Mock.Of<IUserStore<IdentityUser>>(),
                null, null, null, null, null, null, null, null);
            var mockSignInManager = new Mock<SignInManager<IdentityUser>>(
                mockUserManager.Object,
                Mock.Of<IHttpContextAccessor>(),
                Mock.Of<IUserClaimsPrincipalFactory<IdentityUser>>(),
                null, null, null, null);

            var user = new IdentityUser { UserName = "TestUser", PhoneNumber = "123456789" };

            mockUserManager.Setup(x => x.GetUserAsync(It.IsAny<System.Security.Claims.ClaimsPrincipal>())).ReturnsAsync(user);
            mockUserManager.Setup(x => x.GetUserNameAsync(user)).ReturnsAsync(user.UserName);
            mockUserManager.Setup(x => x.GetPhoneNumberAsync(user)).ReturnsAsync(user.PhoneNumber);

            var model = new IndexModel(mockUserManager.Object, mockSignInManager.Object)
            {
                PageContext = new PageContext { HttpContext = new DefaultHttpContext { User = new System.Security.Claims.ClaimsPrincipal() } }
            };

            var result = await model.OnGetAsync();

            Assert.IsType<PageResult>(result);
            Assert.Equal("TestUser", model.Username);
            Assert.Equal("123456789", model.Input.PhoneNumber);
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

            var model = new IndexModel(mockUserManager.Object, mockSignInManager.Object)
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
            var mockSignInManager = new Mock<SignInManager<IdentityUser>>(
                mockUserManager.Object,
                Mock.Of<IHttpContextAccessor>(),
                Mock.Of<IUserClaimsPrincipalFactory<IdentityUser>>(),
                null, null, null, null);

            var user = new IdentityUser();

            mockUserManager.Setup(x => x.GetUserAsync(It.IsAny<System.Security.Claims.ClaimsPrincipal>())).ReturnsAsync(user);

            var model = new IndexModel(mockUserManager.Object, mockSignInManager.Object)
            {
                Input = new IndexModel.InputModel { PhoneNumber = "123456789" },
                PageContext = new PageContext { HttpContext = new DefaultHttpContext { User = new System.Security.Claims.ClaimsPrincipal() } }
            };

            model.ModelState.AddModelError("Input.PhoneNumber", "Invalid phone number");

            var result = await model.OnPostAsync();

            Assert.IsType<PageResult>(result);
        }

        [Fact]
        public async Task OnPostAsync_PhoneNumberUpdatedSuccessfully_ReturnsRedirect()
        {
            var mockUserManager = new Mock<UserManager<IdentityUser>>(
                Mock.Of<IUserStore<IdentityUser>>(),
                null, null, null, null, null, null, null, null);
            var mockSignInManager = new Mock<SignInManager<IdentityUser>>(
                mockUserManager.Object,
                Mock.Of<IHttpContextAccessor>(),
                Mock.Of<IUserClaimsPrincipalFactory<IdentityUser>>(),
                null, null, null, null);

            var user = new IdentityUser { PhoneNumber = "123456789" };

            mockUserManager.Setup(x => x.GetUserAsync(It.IsAny<System.Security.Claims.ClaimsPrincipal>())).ReturnsAsync(user);
            mockUserManager.Setup(x => x.GetPhoneNumberAsync(user)).ReturnsAsync("123456789");
            mockUserManager.Setup(x => x.SetPhoneNumberAsync(user, "987654321")).ReturnsAsync(IdentityResult.Success);

            var model = new IndexModel(mockUserManager.Object, mockSignInManager.Object)
            {
                Input = new IndexModel.InputModel { PhoneNumber = "987654321" },
                PageContext = new PageContext { HttpContext = new DefaultHttpContext { User = new System.Security.Claims.ClaimsPrincipal() } }
            };

            var result = await model.OnPostAsync();

            var redirectResult = Assert.IsType<RedirectToPageResult>(result);
            Assert.Equal("Your profile has been updated", model.StatusMessage);
        }

        [Fact]
        public async Task OnPostAsync_SetPhoneNumberFails_ReturnsRedirectWithError()
        {
            var mockUserManager = new Mock<UserManager<IdentityUser>>(
                Mock.Of<IUserStore<IdentityUser>>(),
                null, null, null, null, null, null, null, null);
            var mockSignInManager = new Mock<SignInManager<IdentityUser>>(
                mockUserManager.Object,
                Mock.Of<IHttpContextAccessor>(),
                Mock.Of<IUserClaimsPrincipalFactory<IdentityUser>>(),
                null, null, null, null);

            var user = new IdentityUser { PhoneNumber = "123456789" };

            mockUserManager.Setup(x => x.GetUserAsync(It.IsAny<System.Security.Claims.ClaimsPrincipal>())).ReturnsAsync(user);
            mockUserManager.Setup(x => x.GetPhoneNumberAsync(user)).ReturnsAsync("123456789");
            mockUserManager.Setup(x => x.SetPhoneNumberAsync(user, "987654321")).ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Error setting phone number." }));

            var model = new IndexModel(mockUserManager.Object, mockSignInManager.Object)
            {
                Input = new IndexModel.InputModel { PhoneNumber = "987654321" },
                PageContext = new PageContext { HttpContext = new DefaultHttpContext { User = new System.Security.Claims.ClaimsPrincipal() } }
            };

            var result = await model.OnPostAsync();

            var redirectResult = Assert.IsType<RedirectToPageResult>(result);
            Assert.Equal("Unexpected error when trying to set phone number.", model.StatusMessage);
        }
    }
}
