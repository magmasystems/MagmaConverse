using Microsoft.VisualStudio.TestTools.UnitTesting;

// ReSharper disable once CheckNamespace
namespace MagmaConverse.Models.Tests
{
    [TestClass()]
    public class SBSFormModelTests
    {
        private SBSFormModel TheModel { get; set; }

        [TestInitialize]
        public void InitializeTest()
        {
            this.TheModel = SBSFormModel.Instance;
        }

        [TestMethod()]
        public void MaterializeFormTest()
        {

        }

        [TestMethod()]
        public void AddFieldTest()
        {

        }

        [TestMethod()]
        public void InsertFieldTest()
        {

        }

        [TestMethod()]
        public void InsertFieldTest1()
        {

        }

        [TestMethod()]
        public void GetFieldTest()
        {

        }

        [TestMethod()]
        public void DeleteFieldTest()
        {

        }

        [TestMethod()]
        public void ClearFieldsTest()
        {

        }

        [TestMethod()]
        public void InstanceExistsTest()
        {

        }

        [TestMethod()]
        public void GetFormInstanceTest()
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