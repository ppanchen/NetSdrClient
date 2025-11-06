using EchoTspServer.Infrastructure;
using NUnit.Framework;

namespace EchoTspServer.Tests
{
    [TestFixture]
    public class ConsoleLoggerTests
    {
        [Test]
        public void Info_WritesToConsole()
        {
            var logger = new ConsoleLogger();
            Assert.DoesNotThrow(() => logger.Info("Test info message"));
        }

        [Test]
        public void Error_WritesToConsole()
        {
            var logger = new ConsoleLogger();
            Assert.DoesNotThrow(() => logger.Error("Test error message"));
        }
    }
}
