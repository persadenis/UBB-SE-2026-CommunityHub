using Xunit;

namespace ChatModule.Tests
{
    public class SanityCheckTests
    {
        [Fact]
        public void TestEnvironment_IsWorking()
        {
            // Arrange
            int a = 2;
            int b = 2;

            // Act
            int result = a + b;

            // Assert
            Assert.Equal(4, result);
        }
    }
}