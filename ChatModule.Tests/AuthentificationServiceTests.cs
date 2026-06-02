using System;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using Xunit;
using ChatAndEvents.Data.ChatData.domain;
using ChatAndEvents.Data.ChatData.repositories;
using ChatAndEvents.Data.ChatData.services;

namespace ChatModule.Tests
{
    public class AuthentificationServiceTests
    {
        private readonly Mock<IUserRepository> _mockUserRepo;
        private readonly AuthenticationService _authService;

        public AuthentificationServiceTests()
        {
            _mockUserRepo = new Mock<IUserRepository>();
            _authService = new AuthenticationService(_mockUserRepo.Object);
        }

        [Fact]
        public async Task LoginAsync_ValidCredentials_ReturnsUserObject()
        {
            var password = "CorrectPass123!";
            var hash = BCrypt.Net.BCrypt.HashPassword(password);
            var user = new User { Username = "TestUser", PasswordHash = hash };
            _mockUserRepo.Setup(r => r.GetByUsernameAsync("TestUser")).ReturnsAsync(user);

            var result = await _authService.LoginAsync("TestUser", password);

            Assert.Equal("TestUser", result?.Username);
        }

        [Fact]
        public async Task LoginAsync_UserAlreadyOnline_CallsUpdateAsync()
        {
            var password = "CorrectPass123!";
            var hash = BCrypt.Net.BCrypt.HashPassword(password);
            var user = new User { Username = "OnlineUser", PasswordHash = hash, Status = UserStatus.Online };
            _mockUserRepo.Setup(r => r.GetByUsernameAsync("OnlineUser")).ReturnsAsync(user);

            await _authService.LoginAsync("OnlineUser", password);

            _mockUserRepo.Verify(r => r.UpdateAsync(It.Is<User>(u => u.Status == UserStatus.Online)), Times.Once);
        }

        [Theory]
        [InlineData("", "Pass123!")]
        [InlineData("User", "")]
        [InlineData(null, null)]
        public async Task LoginAsync_EmptyFields_ReturnsNull(string u, string p)
        {
            var result = await _authService.LoginAsync(u, p);
            Assert.Null(result);
        }

        [Fact]
        public async Task LoginAsync_UserMissing_ReturnsNull()
        {
            _mockUserRepo.Setup(r => r.GetByUsernameAsync(It.IsAny<string>())).ReturnsAsync((User?)null);
            var result = await _authService.LoginAsync("Unknown", "AnyPass");
            Assert.Null(result);
        }

        [Fact]
        public async Task LoginAsync_WrongPassword_ReturnsNull()
        {
            var user = new User { PasswordHash = BCrypt.Net.BCrypt.HashPassword("Real") };
            _mockUserRepo.Setup(r => r.GetByUsernameAsync("User")).ReturnsAsync(user);

            var result = await _authService.LoginAsync("User", "Wrong");

            Assert.Null(result);
        }
        [Fact]
        public async Task RegisterAsync_ValidInput_CallsCreateAsync()
        {
            _mockUserRepo.Setup(r => r.GetByUsernameAsync(It.IsAny<string>())).ReturnsAsync((User?)null);
            _mockUserRepo.Setup(r => r.GetByEmailAsync(It.IsAny<string>())).ReturnsAsync((User?)null);

            await _authService.RegisterAsync("NewUser", "test@test.com", "SecurePass123!", "+12345678", DateTime.Now.AddYears(-20), null);

            _mockUserRepo.Verify(r => r.CreateAsync(It.IsAny<User>()), Times.Once);
        }

        [Fact]
        public async Task RegisterAsync_UsernameTaken_ThrowsInvalidOperationException()
        {
            _mockUserRepo.Setup(r => r.GetByUsernameAsync("Taken")).ReturnsAsync(new User());
            
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _authService.RegisterAsync("Taken", "u@test.com", "Pass123!", "+12345678", DateTime.Now.AddYears(-20), null));
        }

        [Fact]
        public async Task RegisterAsync_EmailTaken_ThrowsInvalidOperationException()
        {
            _mockUserRepo.Setup(r => r.GetByUsernameAsync(It.IsAny<string>())).ReturnsAsync((User?)null);
            _mockUserRepo.Setup(r => r.GetByEmailAsync("taken@test.com")).ReturnsAsync(new User());

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _authService.RegisterAsync("Unique", "taken@test.com", "Pass123!", "+12345678", DateTime.Now.AddYears(-20), null));
        }

        [Theory]
        [InlineData("abc", "v@t.com", "Pass123!", "+12345678", -20)]
        [InlineData("Valid", "v@t.com", "short", "+12345678", -20)]
        [InlineData("Valid", "not-email", "Pass123!", "+12345678", -20)] 
        [InlineData("Valid", "v@t.com", "Pass123!", "+123", -20)]
        [InlineData("Valid", "v@t.com", "Pass123!", "+12345678", 1)] 
        [InlineData("Valid", "user name@t.com", "Pass123!", "+12345678", -20)] 
        public async Task RegisterAsync_InvalidInputs_ThrowsInvalidOperationException(string u, string e, string p, string ph, int years)
        {
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _authService.RegisterAsync(u, e, p, ph, DateTime.Today.AddYears(years), null));
        }

        [Fact]
        public async Task ChangePasswordAsync_ValidRequest_CallsUpdatePassword()
        {
            var user = new User { Id = Guid.NewGuid(), Email = "user@test.com" };
            _mockUserRepo.Setup(r => r.GetByEmailAsync(user.Email)).ReturnsAsync(user);

            await _authService.ChangePasswordAsync(user.Email, "NewSecurePass123!");

            _mockUserRepo.Verify(r => r.UpdatePasswordAsync(user.Id, It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task ChangePasswordAsync_UserNotFound_ThrowsInvalidOperationException()
        {
            _mockUserRepo.Setup(r => r.GetByEmailAsync(It.IsAny<string>())).ReturnsAsync((User?)null);
            await Assert.ThrowsAsync<InvalidOperationException>(() => _authService.ChangePasswordAsync("missing@t.com", "Pass123!"));
        }

        [Theory]
        [InlineData("bad-email", "ValidPass123!")]
        [InlineData("valid@t.com", "weak")]
        public async Task ChangePasswordAsync_InvalidInput_ThrowsInvalidOperationException(string email, string pass)
        {
            await Assert.ThrowsAsync<InvalidOperationException>(() => _authService.ChangePasswordAsync(email, pass));
        }
        [Theory]
        [InlineData("@test.com")]
        [InlineData("user@")]
        [InlineData(" @test.com")]
        [InlineData("user@ ")]
        public async Task RegisterAsync_EmailMissingParts_ReturnsFalse(string badEmail)
        { 
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _authService.RegisterAsync("ValidUser", badEmail, "Pass123!", "+123456789", DateTime.Today.AddYears(-20), null));
        }
        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]
        public async Task RegisterAsync_PhoneEmptyOrNull_ReturnsFalse(string? badPhone)
        {
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _authService.RegisterAsync("ValidUser", "valid@email.com", "Pass123!", badPhone!, DateTime.Today.AddYears(-20), null));
        }
    }
}