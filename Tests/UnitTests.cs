namespace Tests
{
    using AutoMapper;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using OnlyV.Mappings;

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
