
using BookProject.Constants;
using BookProject.Data;
using Microsoft.AspNetCore.Identity;
using Moq;

namespace BookProject.Tests.Tests
{
    public class DbSeederTests
    {
        [Fact]
        public async Task SeedDefaultAsync_ShouldSeedReolesAndAdminUser()
        {
            var roleStore = new Mock<IRoleStore<IdentityRole>>();
            var roleManager = new Mock<RoleManager<IdentityRole>>(roleStore.Object, null, null, null, null);

            roleManager.Setup(r => r.RoleExistsAsync(Roles.Admin.ToString())).ReturnsAsync(false);
            roleManager.Setup(r => r.RoleExistsAsync(Roles.User.ToString())).ReturnsAsync(false);
            roleManager.Setup(r => r.CreateAsync(It.IsAny<IdentityRole>())).ReturnsAsync(IdentityResult.Success);

            var userStore = new Mock<IUserStore<IdentityUser>>();
            var userManager = new Mock<UserManager<IdentityUser>>(userStore.Object, null, null, null, null, null, null, null, null);

            userManager.Setup(u => u.FindByEmailAsync(It.IsAny<string>())).ReturnsAsync((IdentityUser)null);
            userManager.Setup(u => u.CreateAsync(It.IsAny<IdentityUser>(), It.IsAny<string>())).ReturnsAsync(IdentityResult.Success);
            userManager.Setup(u => u.AddToRoleAsync(It.IsAny<IdentityUser>(), Roles.Admin.ToString())).ReturnsAsync(IdentityResult.Success);

            var serviceProvider = new Mock<IServiceProvider>();
            serviceProvider.Setup(s => s.GetService(typeof(RoleManager<IdentityRole>))).Returns(roleManager.Object);
            serviceProvider.Setup(s => s.GetService(typeof(UserManager<IdentityUser>))).Returns(userManager.Object);

            await DbSeeder.SeedDefaultDataAsync(serviceProvider.Object);

            roleManager.Verify(r => r.RoleExistsAsync(Roles.Admin.ToString()), Times.Once);
            roleManager.Verify(r => r.RoleExistsAsync(Roles.User.ToString()), Times.Once);
            roleManager.Verify(r => r.CreateAsync(It.Is<IdentityRole>(role => role.Name == Roles.Admin.ToString())), Times.Once);
            roleManager.Verify(r => r.CreateAsync(It.Is<IdentityRole>(role => role.Name == Roles.User.ToString())), Times.Once);
            userManager.Verify(u => u.FindByEmailAsync("admin@gmail.com"), Times.Once);
            userManager.Verify(u => u.CreateAsync(It.Is<IdentityUser>(user => user.Email == "admin@gmail.com"), "Admin@123"), Times.Once);
            userManager.Verify(u => u.AddToRoleAsync(It.IsAny<IdentityUser>(), Roles.Admin.ToString()), Times.Once);
        }
    }
}
