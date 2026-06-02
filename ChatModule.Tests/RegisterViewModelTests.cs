using System;
using System.Threading.Tasks;
using ChatAndEvents.Data.ChatData.services;
using ChatModule.Models;
using ChatModule.ViewModels;
using Moq;
using Xunit;

namespace ChatModule.Tests
{
    public class RegisterViewModelTests
    {
        private readonly Mock<IAuthenticationService> _mockAuth;
        private readonly RegisterViewModel _viewModel;

        public RegisterViewModelTests()
        {
            _mockAuth = new Mock<IAuthenticationService>();
            _viewModel = new RegisterViewModel(_mockAuth.Object);
        }


        [Fact]
        public void Constructor_NullService_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new RegisterViewModel(null!));
        }

        [Fact]
        public void Username_Set_UpdatesProperty()
        {
            _viewModel.Username = "TestUser";
            Assert.Equal("TestUser", _viewModel.Username);
        }

        [Fact]
        public void Email_Set_UpdatesProperty()
        {
            _viewModel.Email = "test@test.com";
            Assert.Equal("test@test.com", _viewModel.Email);
        }

        [Fact]
        public void Password_Set_UpdatesProperty()
        {
            _viewModel.Password = "Pass123!";
            Assert.Equal("Pass123!", _viewModel.Password);
        }

        [Fact]
        public void Phone_Set_UpdatesProperty()
        {
            _viewModel.Phone = "+1234567";
            Assert.Equal("+1234567", _viewModel.Phone);
        }

        [Fact]
        public void Birthday_Set_UpdatesProperty()
        {
            var date = new DateTime(2000, 1, 1);
            _viewModel.Birthday = date;
            Assert.Equal(date, _viewModel.Birthday);
        }

        [Fact]
        public void ErrorMessage_Set_UpdatesProperty()
        {
            _viewModel.ErrorMessage = "Test Error";
            Assert.Equal("Test Error", _viewModel.ErrorMessage);
        }

        [Fact]
        public async Task RegisterAsync_OnSuccess_ClearsErrorMessage()
        {
            _viewModel.ErrorMessage = "Initial Error";
            SetupSuccess();

            await _viewModel.RegisterCommand.ExecuteAsync(null);

            Assert.Null(_viewModel.ErrorMessage);
        }

        [Fact]
        public async Task RegisterAsync_OnSuccess_SetsIsLoadingToFalse()
        {
            SetupSuccess();

            await _viewModel.RegisterCommand.ExecuteAsync(null);

            Assert.False(_viewModel.IsLoading);
        }

        [Fact]
        public async Task RegisterAsync_WithEvent_InvokesRegisterSucceeded()
        {
            SetupSuccess();
            bool eventRaised = false;
            _viewModel.RegisterSucceeded += (id, name) => { eventRaised = true; return Task.CompletedTask; };

            await _viewModel.RegisterCommand.ExecuteAsync(null);

            Assert.True(eventRaised);
        }

        [Fact]
        public async Task RegisterAsync_WithoutEvent_InvokesNavigation()
        {
            SetupSuccess();
            bool navigated = false;
            _viewModel.NavigateToLoginRequested += () => navigated = true;

            await _viewModel.RegisterCommand.ExecuteAsync(null);

            Assert.True(navigated);
        }


        [Fact]
        public async Task RegisterAsync_OnException_SetsErrorMessage()
        {
            var expectedMessage = "Network Error";
            _mockAuth.Setup(s => s.RegisterAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime?>(), It.IsAny<string>()))
                     .ThrowsAsync(new Exception(expectedMessage));

            await _viewModel.RegisterCommand.ExecuteAsync(null);

            Assert.Equal(expectedMessage, _viewModel.ErrorMessage);
        }

        [Fact]
        public async Task RegisterAsync_OnException_StopsLoading()
        {
            _mockAuth.Setup(s => s.RegisterAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime?>(), It.IsAny<string>()))
                     .ThrowsAsync(new Exception("Fail"));

            await _viewModel.RegisterCommand.ExecuteAsync(null);

            Assert.False(_viewModel.IsLoading);
        }

        [Fact]
        public async Task BackToLoginCommand_InvokesNavigationEvent()
        {
            bool navigated = false;
            _viewModel.NavigateToLoginRequested += () => navigated = true;

            await _viewModel.BackToLoginCommand.ExecuteAsync(null);

            Assert.True(navigated);
        }

        private void SetupSuccess()
        {
            _mockAuth.Setup(s => s.RegisterAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime?>(), It.IsAny<string>()))
                     .ReturnsAsync(new User { Id = Guid.NewGuid(), Username = "User" });
        }
    }
}