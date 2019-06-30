using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable once CheckNamespace
namespace MagmaConverse.Models.Tests
{
    [TestClass()]
    public class SBSFormTemplateModelTests
    {
        private SBSFormDefinitionModel TheModel { get; set; }

        [TestInitialize]
        public void InitializeTest()
        {
            this.TheModel = SBSFormDefinitionModel.Instance;
        }

        [TestMethod()]
        public void CreateFormTemplateTest()
        {

        }

        [TestMethod()]
        public void GetByIdTest()
        {

        }

        [TestMethod()]
        public void GetByNameTest()
        {

        }

        [TestMethod()]
        public void ChangeFormTest()
        {

        }

        [TestMethod()]
        public void ChangeFormPropertiesTest()
        {

        }

        [TestMethod()]
        [TestCategory("Database")]
        [TestCategory("Models")]
        public void LoadFromDatabaseTest()
        {
            var rc = this.TheModel.LoadFromDatabase();
            Assert.IsTrue(rc, "The call to LoadFromDatabase failed");
        }

        [TestMethod()]
        [TestCategory("Database")]
        [TestCategory("Models")]
        public void SaveToDatabaseTest()
        {

        }
    }
}