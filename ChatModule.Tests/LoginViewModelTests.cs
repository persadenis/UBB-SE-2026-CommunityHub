using System;
using System.Threading.Tasks;
using ChatAndEvents.Data.ChatData.domain;
using ChatAndEvents.Data.ChatData.services;
using ChatModule.viewModels;
using Moq;
using Xunit;

namespace ChatModule.Tests
{
    public class LoginViewModelTests
    {
        private readonly Mock<IAuthenticationService> _mockAuth;
        private readonly LoginViewModel _viewModel;

        public LoginViewModelTests()
        {
            _mockAuth = new Mock<IAuthenticationService>();
            _viewModel = new LoginViewModel(_mockAuth.Object);
        }


        [Fact]
        public void Constructor_NullService_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new LoginViewModel(null!));
        }

        [Fact]
        public void Username_Set_UpdatesProperty()
        {
            _viewModel.Username = "User";
            Assert.Equal("User", _viewModel.Username);
        }

        [Fact]
        public void Password_Set_UpdatesProperty()
        {
            _viewModel.Password = "Pass";
            Assert.Equal("Pass", _viewModel.Password);
        }

        [Fact]
        public void ErrorMessage_Set_UpdatesProperty()
        {
            _viewModel.ErrorMessage = "Error";
            Assert.Equal("Error", _viewModel.ErrorMessage);
        }

        [Fact]
        public async Task LoginAsync_ValidUser_InvokesLoginSucceeded()
        {
            var user = new User { Id = Guid.NewGuid(), Username = "LoggedUser" };
            _mockAuth.Setup(s => s.LoginAsync(It.IsAny<string>(), It.IsAny<string>()))
                     .ReturnsAsync(user);

            bool eventRaised = false;
            _viewModel.LoginSucceeded += (id, name) => { eventRaised = true; return Task.CompletedTask; };

            await _viewModel.LoginCommand.ExecuteAsync(null);

            Assert.True(eventRaised);
        }

        [Fact]
        public async Task LoginAsync_ValidUser_SetsIsLoadingToFalse()
        {
            _mockAuth.Setup(s => s.LoginAsync(It.IsAny<string>(), It.IsAny<string>()))
                     .ReturnsAsync(new User());

            await _viewModel.LoginCommand.ExecuteAsync(null);

            Assert.False(_viewModel.IsLoading);
        }

        [Fact]
        public async Task LoginAsync_InvalidUser_SetsCredentialsErrorMessage()
        {
            _mockAuth.Setup(s => s.LoginAsync(It.IsAny<string>(), It.IsAny<string>()))
                     .ReturnsAsync((User?)null);

            await _viewModel.LoginCommand.ExecuteAsync(null);

            Assert.Equal("Invalid credentials", _viewModel.ErrorMessage);
        }

        [Fact]
        public async Task LoginAsync_OnException_SetsExceptionMessage()
        {
            _mockAuth.Setup(s => s.LoginAsync(It.IsAny<string>(), It.IsAny<string>()))
                     .ThrowsAsync(new Exception("Network Failure"));

            await _viewModel.LoginCommand.ExecuteAsync(null);

            Assert.Equal("Network Failure", _viewModel.ErrorMessage);
        }

        [Fact]
        public async Task GoToRegisterCommand_InvokesRegisterRequested()
        {
            bool raised = false;
            _viewModel.RegisterRequested += () => raised = true;

            await _viewModel.GoToRegisterCommand.ExecuteAsync(null);

            Assert.True(raised);
        }

        [Fact]
        public async Task ForgotPasswordCommand_InvokesForgotPasswordRequested()
        {
            bool raised = false;
            _viewModel.ForgotPasswordRequested += () => raised = true;

            await _viewModel.ForgotPasswordCommand.ExecuteAsync(null);

            Assert.True(raised);
        }
    }
}