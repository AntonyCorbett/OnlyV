using AutoMapper;
using OnlyV.Mappings;

namespace Tests
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
    public class UnitTests
    {
        [TestMethod]
        public void TestMappings()
        {
            Mapper.Reset();
            Mapper.Initialize(cfg => cfg.AddProfile<MappingProfile>());
            Mapper.AssertConfigurationIsValid();
        }
    }
}
