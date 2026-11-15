using FlowERP.Identity.Application;
using FlowERP.Identity.Application.Roles;
using FlowERP.Identity.Application.UserRoles;
using FlowERP.Identity.Application.Users;
using FlowERP.Identity.Domain.Roles;
using FlowERP.Identity.Domain.UserRoles;
using FlowERP.Identity.Domain.Users;
using Moq;
using Xunit;

namespace FlowERP.Identity.UnitTests;

public sealed class AssignRoleServiceTests
{
    // Tell xUnit to discover this method as a test.
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task AssignAsync_WhenUserAndRoleExist_SavesOnlyNewAssignments(bool alreadyAssigned)
    {
        // Simulate an existing user and role, with or without an existing assignment.
        var user = new User(Guid.NewGuid(), "user@flowerp.com", "password-hash");
        var role = new Role(Guid.NewGuid(), "Manager");
        var users = new Mock<IUserRepository>();
        var roles = new Mock<IRoleRepository>();
        var assignments = new Mock<IUserRoleRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();

        users.Setup(repository => repository.GetByIdAsync(user.Id, default)).ReturnsAsync(user);
        roles.Setup(repository => repository.GetByIdAsync(role.Id, default)).ReturnsAsync(role);
        assignments.Setup(repository => repository.ExistsAsync(user.Id, role.Id, default))
            .ReturnsAsync(alreadyAssigned);

        var service = new AssignRoleService(users.Object, roles.Object, assignments.Object, unitOfWork.Object);

        if (alreadyAssigned)
        {
            await Assert.ThrowsAsync<UserRoleAlreadyAssignedException>(() => service.AssignAsync(user.Id, role.Id));
            assignments.Verify(repository => repository.AddAsync(
                It.IsAny<UserRole>(), It.IsAny<CancellationToken>()), Times.Never());
            unitOfWork.VerifyNoOtherCalls();
            return;
        }

        await service.AssignAsync(user.Id, role.Id);

        assignments.Verify(repository => repository.AddAsync(
            It.Is<UserRole>(assignment => assignment.UserId == user.Id && assignment.RoleId == role.Id),
            default), Times.Once());
        unitOfWork.Verify(work => work.SaveChangesAsync(default), Times.Once());
    }
}
