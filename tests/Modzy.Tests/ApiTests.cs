using System.Collections.Generic;
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
        public void CanGetAllModels()
        {
            ApiClient c = new ApiClient();
            var models = c.GetModelsListing().Result;
            Assert.NotNull(c.ApiKey);
            Assert.NotNull(c.BaseUrl);
            Assert.NotEmpty(models);
            List<Model> models_ = new List<Model>();
            foreach (var model in models)
            {
                models_.Add(c.GetModel(model.ModelId).Result);
            }
            Assert.NotEmpty(models_);
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

        [Fact]
        public void CanRunModelWithText()
        {
            ApiClient c = new ApiClient();
            var m = c.GetModel("ed542963de").Result;
            var j = c.RunModelWithText(m, "1.0.1", "input.txt", "The Knicks suck and I hate them.").Result;
            var r = c.WaitUntilComplete(j).Result;
            Assert.NotNull(j);
        }

        [Fact]
        public void CanGetResultsDunamic()
        {
            ApiClient c = new ApiClient();
            var m = c.GetModel("ed542963de").Result;
            var j = c.RunModelWithText(m, "1.0.1", "input.txt", "The Knicks suck and I hate them.").Result;
            var r = c.GetResultsDynamic(j.JobIdentifier).Result;
            Assert.NotNull(j);
        }
    }
}