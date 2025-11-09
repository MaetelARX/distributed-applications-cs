using System;
using System.Text;
using System.Threading.Tasks;
using BookProject.Areas.Identity.Pages.Account;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Moq;
using Xunit;

namespace BookProject.Tests
{
    public class ResetPasswordModelTests
    {
        [Fact]
        public void OnGet_ValidCode_ReturnsPage()
        {
            var mockUserManager = new Mock<UserManager<IdentityUser>>(
                Mock.Of<IUserStore<IdentityUser>>(),
                null, null, null, null, null, null, null, null);

            var resetPasswordModel = new ResetPasswordModel(mockUserManager.Object);

            var code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes("test-code"));
            var result = resetPasswordModel.OnGet(code);

            Assert.IsType<PageResult>(result);
            Assert.Equal("test-code", resetPasswordModel.Input.Code);
        }

        [Fact]
        public void OnGet_NullCode_ReturnsBadRequest()
        {
            var mockUserManager = new Mock<UserManager<IdentityUser>>(
                Mock.Of<IUserStore<IdentityUser>>(),
                null, null, null, null, null, null, null, null);

            var resetPasswordModel = new ResetPasswordModel(mockUserManager.Object);

            var result = resetPasswordModel.OnGet(null);

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task OnPostAsync_InvalidModelState_ReturnsPage()
        {
            var mockUserManager = new Mock<UserManager<IdentityUser>>(
                Mock.Of<IUserStore<IdentityUser>>(),
                null, null, null, null, null, null, null, null);

            var resetPasswordModel = new ResetPasswordModel(mockUserManager.Object)
            {
                Input = new ResetPasswordModel.InputModel
                {
                    Email = "test@example.com",
                    Password = "Password123!",
                    ConfirmPassword = "Password123!",
                    Code = "test-code"
                }
            };

            resetPasswordModel.ModelState.AddModelError("Test", "Invalid model state");

            var result = await resetPasswordModel.OnPostAsync();

            Assert.IsType<PageResult>(result);
        }

        [Fact]
        public async Task OnPostAsync_UserNotFound_RedirectsToConfirmation()
        {
            var mockUserManager = new Mock<UserManager<IdentityUser>>(
                Mock.Of<IUserStore<IdentityUser>>(),
                null, null, null, null, null, null, null, null);

            mockUserManager.Setup(x => x.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync((IdentityUser)null);

            var resetPasswordModel = new ResetPasswordModel(mockUserManager.Object)
            {
                Input = new ResetPasswordModel.InputModel
                {
                    Email = "test@example.com",
                    Password = "Password123!",
                    ConfirmPassword = "Password123!",
                    Code = "test-code"
                }
            };

            var result = await resetPasswordModel.OnPostAsync();

            var redirectResult = Assert.IsType<RedirectToPageResult>(result);
            Assert.Equal("./ResetPasswordConfirmation", redirectResult.PageName);
        }

        [Fact]
        public async Task OnPostAsync_ResetPasswordSucceeded_RedirectsToConfirmation()
        {
            var mockUserManager = new Mock<UserManager<IdentityUser>>(
                Mock.Of<IUserStore<IdentityUser>>(),
                null, null, null, null, null, null, null, null);

            var user = new IdentityUser { Email = "test@example.com" };

            mockUserManager.Setup(x => x.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync(user);
            mockUserManager.Setup(x => x.ResetPasswordAsync(user, It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success);

            var resetPasswordModel = new ResetPasswordModel(mockUserManager.Object)
            {
                Input = new ResetPasswordModel.InputModel
                {
                    Email = "test@example.com",
                    Password = "Password123!",
                    ConfirmPassword = "Password123!",
                    Code = "test-code"
                }
            };

            var result = await resetPasswordModel.OnPostAsync();

            var redirectResult = Assert.IsType<RedirectToPageResult>(result);
            Assert.Equal("./ResetPasswordConfirmation", redirectResult.PageName);
        }

        [Fact]
        public async Task OnPostAsync_ResetPasswordFailed_ReturnsPageWithErrors()
        {
            var mockUserManager = new Mock<UserManager<IdentityUser>>(
                Mock.Of<IUserStore<IdentityUser>>(),
                null, null, null, null, null, null, null, null);

            var user = new IdentityUser { Email = "test@example.com" };

            mockUserManager.Setup(x => x.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync(user);
            mockUserManager.Setup(x => x.ResetPasswordAsync(user, It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Password reset failed." }));

            var resetPasswordModel = new ResetPasswordModel(mockUserManager.Object)
            {
                Input = new ResetPasswordModel.InputModel
                {
                    Email = "test@example.com",
                    Password = "Password123!",
                    ConfirmPassword = "Password123!",
                    Code = "test-code"
                }
            };

            var result = await resetPasswordModel.OnPostAsync();

            Assert.IsType<PageResult>(result);
            Assert.True(resetPasswordModel.ModelState.ContainsKey(string.Empty));
            Assert.Contains("Password reset failed.", resetPasswordModel.ModelState[string.Empty].Errors[0].ErrorMessage);
        }
    }
}