﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.Threading.Tasks;
using FakeItEasy;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Entities.Apps.Commands;
using Squidex.Domain.Apps.Entities.Apps.Services;
using Squidex.Domain.Apps.Entities.TestHelpers;
using Squidex.Infrastructure;
using Squidex.Shared.Users;
using Xunit;

#pragma warning disable SA1310 // Field names must not contain underscore

namespace Squidex.Domain.Apps.Entities.Apps.Guards
{
    public class GuardAppContributorsTests
    {
        private readonly IUser user1 = A.Fake<IUser>();
        private readonly IUser user2 = A.Fake<IUser>();
        private readonly IUser user3 = A.Fake<IUser>();
        private readonly IUserResolver users = A.Fake<IUserResolver>();
        private readonly IAppLimitsPlan appPlan = A.Fake<IAppLimitsPlan>();
        private readonly AppContributors contributors_0 = AppContributors.Empty;
        private readonly Roles roles = Roles.CreateDefaults("my-app");

        public GuardAppContributorsTests()
        {
            A.CallTo(() => user1.Id).Returns("1");
            A.CallTo(() => user2.Id).Returns("2");
            A.CallTo(() => user3.Id).Returns("3");

            A.CallTo(() => users.FindByIdOrEmailAsync("1")).Returns(user1);
            A.CallTo(() => users.FindByIdOrEmailAsync("2")).Returns(user2);
            A.CallTo(() => users.FindByIdOrEmailAsync("3")).Returns(user3);

            A.CallTo(() => users.FindByIdOrEmailAsync("1@email.com")).Returns(user1);
            A.CallTo(() => users.FindByIdOrEmailAsync("2@email.com")).Returns(user2);
            A.CallTo(() => users.FindByIdOrEmailAsync("3@email.com")).Returns(user3);

            A.CallTo(() => users.FindByIdOrEmailAsync("notfound"))
                .Returns(Task.FromResult<IUser>(null));

            A.CallTo(() => appPlan.MaxContributors)
                .Returns(10);
        }

        [Fact]
        public Task CanAssign_should_throw_exception_if_contributor_id_is_null()
        {
            var command = new AssignContributor();

            return ValidationAssert.ThrowsAsync(() => GuardAppContributors.CanAssign(contributors_0, command, users, appPlan, roles),
                new ValidationError("Contributor id is required.", "ContributorId"));
        }

        [Fact]
        public Task CanAssign_should_throw_exception_if_role_not_valid()
        {
            var command = new AssignContributor { ContributorId = "1", Role = "Invalid" };

            return ValidationAssert.ThrowsAsync(() => GuardAppContributors.CanAssign(contributors_0, command, users, appPlan, roles),
                new ValidationError("Role is not valid.", "Role"));
        }

        [Fact]
        public Task CanAssign_should_throw_exception_if_user_already_exists_with_same_role()
        {
            var command = new AssignContributor { ContributorId = "1", Role = Role.Owner };

            var contributors_1 = contributors_0.Assign("1", Role.Owner);

            return ValidationAssert.ThrowsAsync(() => GuardAppContributors.CanAssign(contributors_1, command, users, appPlan, roles),
                new ValidationError("Contributor has already this role.", "Role"));
        }

        [Fact]
        public Task CanAssign_should_throw_exception_if_user_not_found()
        {
            var command = new AssignContributor { ContributorId = "notfound", Role = Role.Owner };

            return Assert.ThrowsAsync<DomainObjectNotFoundException>(() => GuardAppContributors.CanAssign(contributors_0, command, users, appPlan, roles));
        }

        [Fact]
        public Task CanAssign_should_throw_exception_if_user_is_actor()
        {
            var command = new AssignContributor { ContributorId = "3", Role = Role.Editor, Actor = new RefToken("user", "3") };

            return Assert.ThrowsAsync<DomainForbiddenException>(() => GuardAppContributors.CanAssign(contributors_0, command, users, appPlan, roles));
        }

        [Fact]
        public Task CanAssign_should_throw_exception_if_contributor_max_reached()
        {
            A.CallTo(() => appPlan.MaxContributors)
                .Returns(2);

            var command = new AssignContributor { ContributorId = "3" };

            var contributors_1 = contributors_0.Assign("1", Role.Owner);
            var contributors_2 = contributors_1.Assign("2", Role.Editor);

            return ValidationAssert.ThrowsAsync(() => GuardAppContributors.CanAssign(contributors_2, command, users, appPlan, roles),
                new ValidationError("You have reached the maximum number of contributors for your plan."));
        }

        [Fact]
        public async Task CanAssign_assign_if_if_user_added_by_email()
        {
            var command = new AssignContributor { ContributorId = "1@email.com" };

            await GuardAppContributors.CanAssign(contributors_0, command, users, appPlan, roles);

            Assert.Equal("1", command.ContributorId);
        }

        [Fact]
        public Task CanAssign_should_not_throw_exception_if_user_found()
        {
            var command = new AssignContributor { ContributorId = "1" };

            return GuardAppContributors.CanAssign(contributors_0, command, users, appPlan, roles);
        }

        [Fact]
        public Task CanAssign_should_not_throw_exception_if_contributor_has_another_role()
        {
            var command = new AssignContributor { ContributorId = "1", Role = Role.Developer };

            var contributors_1 = contributors_0.Assign("1", Role.Editor);

            return GuardAppContributors.CanAssign(contributors_1, command, users, appPlan, roles);
        }

        [Fact]
        public Task CanAssign_should_not_throw_exception_if_contributor_max_reached_but_role_changed()
        {
            A.CallTo(() => appPlan.MaxContributors)
                .Returns(2);

            var command = new AssignContributor { ContributorId = "1", Role = Role.Developer };

            var contributors_1 = contributors_0.Assign("1", Role.Editor);
            var contributors_2 = contributors_1.Assign("2", Role.Editor);

            return GuardAppContributors.CanAssign(contributors_2, command, users, appPlan, roles);
        }

        [Fact]
        public void CanRemove_should_throw_exception_if_contributor_id_is_null()
        {
            var command = new RemoveContributor();

            ValidationAssert.Throws(() => GuardAppContributors.CanRemove(contributors_0, command),
                new ValidationError("Contributor id is required.", "ContributorId"));
        }

        [Fact]
        public void CanRemove_should_throw_exception_if_contributor_not_found()
        {
            var command = new RemoveContributor { ContributorId = "1" };

            Assert.Throws<DomainObjectNotFoundException>(() => GuardAppContributors.CanRemove(contributors_0, command));
        }

        [Fact]
        public void CanRemove_should_throw_exception_if_contributor_is_only_owner()
        {
            var command = new RemoveContributor { ContributorId = "1" };

            var contributors_1 = contributors_0.Assign("1", Role.Owner);
            var contributors_2 = contributors_1.Assign("2", Role.Editor);

            ValidationAssert.Throws(() => GuardAppContributors.CanRemove(contributors_2, command),
                new ValidationError("Cannot remove the only owner."));
        }

        [Fact]
        public void CanRemove_should_not_throw_exception_if_contributor_not_only_owner()
        {
            var command = new RemoveContributor { ContributorId = "1" };

            var contributors_1 = contributors_0.Assign("1", Role.Owner);
            var contributors_2 = contributors_1.Assign("2", Role.Owner);

            GuardAppContributors.CanRemove(contributors_2, command);
        }
    }
}
