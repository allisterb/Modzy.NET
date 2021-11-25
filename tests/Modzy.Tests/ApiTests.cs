
using Microsoft.Extensions.Configuration;
using Xunit;


using Modzy;
namespace Modzy.Tests
{
    public class ApiTests
    {
        public ApiTests()
        {
            Runtime.Configuration = new ConfigurationBuilder()
            .AddUserSecrets("c0697968-04fe-49d7-a785-aaa817e38935")
            .Build();

        }

        [Fact]
        public void CanGetConfig()
        {
            Assert.NotNull(Runtime.Config("MODZY_BASE_URL"));
            Assert.NotNull(Runtime.Config("MODZY_API_KEY"));
        }

        [Fact]
        public void CanConstructClient()
        {
            ApiClient c = new ApiClient();
            Assert.NotNull(c.ApiKey);
            Assert.NotNull(c.BaseUrl);
        }

        [Fact]
        public void CanGetModels()
        {
            ApiClient c = new ApiClient();
            var models = c.GetAllModels().Result;
            Assert.NotNull(c.ApiKey);
            Assert.NotNull(c.BaseUrl);
            Assert.NotEmpty(models);
        }

        [Fact]
        public void CanGetModel()
        {
            ApiClient c = new ApiClient();
            var model = c.GetModel("z8qms2pgvx").Result;
            Assert.NotNull(model);
        }

        [Fact]
        public void CanGetModelVersions()
        {
            ApiClient c = new ApiClient();
            var model = c.GetModel("z8qms2pgvx").Result;
            var v = c.GetModelVersions(model.ModelId).Result;
            Assert.NotNull(model);
            Assert.NotNull(v);
        }

        [Fact]
        public void CanGetSampleModelInput()
        {
            ApiClient c = new ApiClient();
            var s = c.GetModelSampleInput("z8qms2pgvx", "4.1.0").Result;
            Assert.NotNull(s);
        }
    }
}