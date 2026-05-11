using System.Threading.Tasks;
using BelKhidmah.Models.TokenAuth;
using BelKhidmah.Web.Controllers;
using Shouldly;
using Xunit;

namespace BelKhidmah.Web.Tests.Controllers
{
    public class HomeController_Tests: BelKhidmahWebTestBase
    {
        [Fact]
        public async Task Index_Test()
        {
            await AuthenticateAsync(null, new AuthenticateModel
            {
                UserNameOrEmailAddress = "admin",
                Password = "123qwe"
            });

            //Act
            var response = await GetResponseAsStringAsync(
                GetUrl<HomeController>(nameof(HomeController.Index))
            );

            //Assert
            response.ShouldNotBeNullOrEmpty();
        }
    }
}