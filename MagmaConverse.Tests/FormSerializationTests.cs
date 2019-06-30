using System.IO;
using MagmaConverse.Data;
using Magmasystems.Framework.Serialization;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MagmaConverse.Tests
{
    [TestClass]
    public class FormSerializationTests
    {
        [TestCategory("Serialization")]
        [TestMethod]
        public void TestFormCreateRequestDeserialization()
        {
            string inputFileName = @"./DIYOnboardingForm.json";
            Assert.IsTrue(File.Exists(inputFileName), "The Json input file does not exist");

            string json = File.ReadAllText(inputFileName);
            var request = Json.Deserialize<FormCreateRequest>(json);

            Assert.IsNotNull(request);
            Assert.IsNotNull(request.Forms);
            Assert.IsTrue(request.Forms.Count == 1);

            Assert.IsNotNull(request.ReferenceData);
            Assert.IsTrue(request.ReferenceData.Count >= 1);
            var states = request.ReferenceData[0];
            Assert.IsTrue(states.Name == "USStates");
            Assert.IsNotNull(states.SortedDictionary);
            Assert.IsTrue(states.SortedDictionary.Count == 52);
        }
    }
}
