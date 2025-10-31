using HomeEnergyApi.Models;
using Moq;

public class IUserRepositoryTests
{

    [Fact]
    public void CanCreateValueEncryptor()
    {
        var mock = new Mock<IUserRepository>();
        mock.Setup(x => x.UserIsAdmin("Admin")).Returns(true);
        mock.CallBase = true;

        var Result = mock.Object.UserIsAdmin("Admin");

        Assert.True(Result, "IUserRepository.UserIsAdmin did not return true for 'Admin' role");
        //Assert.False(userRoleTest, "IUserRepository.UserIsAdmin did not return false for 'User' role");
    }
}
