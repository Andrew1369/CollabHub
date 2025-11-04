using System.Linq;
using CollabHub.Controllers;
using Microsoft.AspNetCore.Authorization;
using Xunit;

namespace CollabHub.Tests
{
    public class AuthorizationAttributesTests
    {
        [Fact]
        public void TodoController_IsProtectedByAuthorizeAttribute()
        {
            var attr = typeof(TodoController)
                .GetCustomAttributes(typeof(AuthorizeAttribute), inherit: true);

            Assert.NotEmpty(attr);
        }

        [Fact]
        public void TodoApiController_IsProtectedByAuthorizeAttribute()
        {
            var attr = typeof(CollabHub.Controllers.Api.TodoApiController)
                .GetCustomAttributes(typeof(AuthorizeAttribute), inherit: true);

            Assert.NotEmpty(attr);
        }

        [Fact]
        public void AccountController_Plans_IsProtectedByAuthorizeAttribute()
        {
            var method = typeof(AccountController)
                .GetMethod("Plans");

            Assert.NotNull(method);

            var attr = method!
                .GetCustomAttributes(typeof(AuthorizeAttribute), inherit: true)
                .Cast<AuthorizeAttribute>()
                .ToArray();

            Assert.NotEmpty(attr);
        }
    }
}
