using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using fiap.Domain.Entities;
using fiap.Services;
using Moq;
using Xunit;

namespace fiap.Tests.Service
{
    public class SecretManagerServiceTests
    {
        [Fact]
        public async Task ObterSecretDbConnect_Test()
        {
            var _logger = new Mock<Serilog.ILogger>();
            var mockSecretsManager = new Mock<IAmazonSecretsManager>();

            var secretValueResponse = new GetSecretValueResponse
            {
                SecretString = "{\"username\":\"teste\",\"password\":\"teste\",\"engine\":\"teste\",\"host\":\"teste\",\"port\":\"teste\",\"dbInstanceIdentifier\":\"teste\"}"
            };

            mockSecretsManager
                .Setup(sm => sm.GetSecretValueAsync(It.IsAny<GetSecretValueRequest>(), default))
                .ReturnsAsync(secretValueResponse);

            var secretsManagerClient = mockSecretsManager.Object;

            var response = await secretsManagerClient.GetSecretValueAsync(new GetSecretValueRequest
            {
                SecretId = "segredo"
            });

            var secret = new SecretManagerService(_logger.Object, mockSecretsManager.Object);
            var result = await secret.ObterSecret<SecretDbConnect>("123456");

            // Assert
            Assert.NotNull(result);
        }

    }
}
