using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Jint;
using Jint.Runtime.Debugger;

namespace MagmaConverse.Tests
{
    [TestClass]
    public class JintTests
    {
        [TestMethod]
        public void TestExpression1()
        {
            /*
             * ${value} = ${field:password1}
             * ${value} <= 50
             */

            var engine = new Engine(options =>
            {
                options.DebugMode(true);
            });
            engine.Step += (sender, information) =>
            {
                Console.WriteLine(information.ToString());
                return StepMode.Over;
            };


            engine.SetValue("x", 10);
            engine.SetValue("y", 50);
            var jsValue = engine.Execute("x <= y").GetCompletionValue();

            Assert.IsNotNull(jsValue, "jsValue is null");
            Assert.IsTrue(jsValue.IsBoolean(), "jsValue is not boolean");
            Assert.IsTrue(jsValue.AsBoolean(), "10 <= 50 should be true");

            engine.SetValue("y", 10);
            engine.SetValue("x", 50);
            jsValue = engine.Execute("x <= y").GetCompletionValue();
            var dotnetObj = jsValue.ToObject();

            Assert.IsNotNull(jsValue, "jsValue is null");
            Assert.IsTrue(jsValue.IsBoolean(), "jsValue is not boolean");
            Assert.IsFalse(jsValue.AsBoolean(), "50 <= 10 should be false");
            Assert.IsNotNull(dotnetObj);
            Assert.IsTrue(dotnetObj is bool);
        }
    }
}
