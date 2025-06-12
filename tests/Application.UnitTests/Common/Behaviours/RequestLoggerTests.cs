using ServerGame.Application.Common.Behaviours;
using ServerGame.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using ServerGame.Application.Accounts.Commands.CreateAccount;
using ServerGame.Application.ApplicationUsers.Services;
using ServerGame.Domain.ValueObjects;
using ServerGame.Domain.ValueObjects.Accounts;

namespace ServerGame.Application.UnitTests.Common.Behaviours;

public class RequestLoggerTests
{
    private Mock<ILogger<CreateAccountCommand>> _logger = null!;
    private Mock<IUser> _user = null!;
    private Mock<IIdentityService> _identityService = null!;

    [SetUp]
    public void Setup()
    {
        _logger = new Mock<ILogger<CreateAccountCommand>>();
        _user = new Mock<IUser>();
        _identityService = new Mock<IIdentityService>();
    }

    [Test]
    public async Task ShouldCallGetUserNameAsyncOnceIfAuthenticated()
    {
        _user.Setup(x => x.Id).Returns(Guid.NewGuid().ToString());

        var requestLogger = new LoggingBehaviour<CreateAccountCommand>(_logger.Object, _user.Object, _identityService.Object);

        await requestLogger.Process(new CreateAccountCommand(Username.Create("Rodorfo"), Email.Create("rodorfo@gmail.com")), CancellationToken.None);

        _identityService.Verify(i => i.GetUserNameAsync(It.IsAny<string>()), Times.Once);
    }

    [Test]
    public async Task ShouldNotCallGetUserNameAsyncOnceIfUnauthenticated()
    {
        var requestLogger = new LoggingBehaviour<CreateAccountCommand>(_logger.Object, _user.Object, _identityService.Object);

        await requestLogger.Process(new CreateAccountCommand(Username.Create("Rodorfo"), Email.Create("rodorfo@gmail.com")), CancellationToken.None);

        _identityService.Verify(i => i.GetUserNameAsync(It.IsAny<string>()), Times.Never);
    }
}
