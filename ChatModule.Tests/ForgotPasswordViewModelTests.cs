using System;
using System.Threading.Tasks;
using ChatAndEvents.Data.ChatData.services;
using ChatModule.ViewModels;
using Moq;
using Xunit;

namespace ChatModule.Tests
{
    public class ForgotPasswordViewModelTests
    {
        private readonly Mock<IAuthenticationService> _mockAuth;
        private readonly ForgotPasswordViewModel _viewModel;

        public ForgotPasswordViewModelTests()
        {
            _mockAuth = new Mock<IAuthenticationService>();
            _viewModel = new ForgotPasswordViewModel(_mockAuth.Object);
        }

        [Fact]
        public void Constructor_NullService_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new ForgotPasswordViewModel(null!));
        }

        [Fact]
        public void Email_Set_UpdatesProperty()
        {
            _viewModel.Email = "test@test.com";
            Assert.Equal("test@test.com", _viewModel.Email);
        }

        [Fact]
        public void NewPassword_Set_UpdatesProperty()
        {
            _viewModel.NewPassword = "NewPass123!";
            Assert.Equal("NewPass123!", _viewModel.NewPassword);
        }

        [Fact]
        public void ErrorMessage_Set_UpdatesProperty()
        {
            _viewModel.ErrorMessage = "Error";
            Assert.Equal("Error", _viewModel.ErrorMessage);
        }

        [Fact]
        public void SuccessMessage_Set_UpdatesProperty()
        {
            _viewModel.SuccessMessage = "Success";
            Assert.Equal("Success", _viewModel.SuccessMessage);
        }

        [Fact]
        public async Task SubmitAsync_OnSuccess_SetsSuccessMessage()
        {
            _viewModel.Email = "user@test.com";
            _viewModel.NewPassword = "Pass123!";
            _mockAuth.Setup(s => s.ChangePasswordAsync(It.IsAny<string>(), It.IsAny<string>()))
                     .Returns(Task.CompletedTask);

            await _viewModel.SubmitCommand.ExecuteAsync(null);

            Assert.Equal("Password updated", _viewModel.SuccessMessage);
        }

        [Fact]
        public async Task SubmitAsync_OnSuccess_ClearsOldErrorMessage()
        {
            _viewModel.ErrorMessage = "Old Error";
            _mockAuth.Setup(s => s.ChangePasswordAsync(It.IsAny<string>(), It.IsAny<string>()))
                     .Returns(Task.CompletedTask);

            await _viewModel.SubmitCommand.ExecuteAsync(null);

            Assert.Null(_viewModel.ErrorMessage);
        }

        [Fact]
        public async Task SubmitAsync_OnException_SetsErrorMessage()
        {
            var expectedError = "Invalid Email";
            _mockAuth.Setup(s => s.ChangePasswordAsync(It.IsAny<string>(), It.IsAny<string>()))
                     .ThrowsAsync(new Exception(expectedError));

            await _viewModel.SubmitCommand.ExecuteAsync(null);

            Assert.Equal(expectedError, _viewModel.ErrorMessage);
        }

        [Fact]
        public async Task SubmitAsync_OnException_ClearsOldSuccessMessage()
        {
            _viewModel.SuccessMessage = "Old Success";
            _mockAuth.Setup(s => s.ChangePasswordAsync(It.IsAny<string>(), It.IsAny<string>()))
                     .ThrowsAsync(new Exception("Fail"));

            await _viewModel.SubmitCommand.ExecuteAsync(null);

            Assert.Null(_viewModel.SuccessMessage);
        }

        [Fact]
        public async Task BackToLoginCommand_InvokesNavigationEvent()
        {
            bool navigated = false;
            _viewModel.NavigateToLoginRequested += () => navigated = true;

            await _viewModel.BackToLoginCommand.ExecuteAsync(null);

            Assert.True(navigated);
        }
    }
}