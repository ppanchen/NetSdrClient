using NetSdrClientApp.Messages;

namespace NetSdrClientAppTests
{
    public class NetSdrMessageHelperTests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void GetControlItemMessageTest()
        {
            //Arrange
            var type = NetSdrMessageHelper.MsgTypes.Ack;
            var code = NetSdrMessageHelper.ControlItemCodes.ReceiverState;
            int parametersLength = 7500;

            //Act
            byte[] msg = NetSdrMessageHelper.GetControlItemMessage(type, code, new byte[parametersLength]);

            var headerBytes = msg.Take(2);
            var codeBytes = msg.Skip(2).Take(2);
            var parametersBytes = msg.Skip(4);

            var num = BitConverter.ToUInt16(headerBytes.ToArray());
            var actualType = (NetSdrMessageHelper.MsgTypes)(num >> 13);
            var actualLength = num - ((int)actualType << 13);
            var actualCode = BitConverter.ToInt16(codeBytes.ToArray());

            //Assert
            Assert.That(headerBytes.Count(), Is.EqualTo(2));
            Assert.That(msg.Length, Is.EqualTo(actualLength));
            Assert.That(type, Is.EqualTo(actualType));

            Assert.That(actualCode, Is.EqualTo((short)code));

            Assert.That(parametersBytes.Count(), Is.EqualTo(parametersLength));
        }

        [Test]
        public void GetDataItemMessageTest()
        {
            //Arrange
            var type = NetSdrMessageHelper.MsgTypes.DataItem2;
            int parametersLength = 7500;

            //Act
            byte[] msg = NetSdrMessageHelper.GetDataItemMessage(type, new byte[parametersLength]);

            var headerBytes = msg.Take(2);
            var parametersBytes = msg.Skip(2);

            var num = BitConverter.ToUInt16(headerBytes.ToArray());
            var actualType = (NetSdrMessageHelper.MsgTypes)(num >> 13);
            var actualLength = num - ((int)actualType << 13);

            //Assert
            Assert.That(headerBytes.Count(), Is.EqualTo(2));
            Assert.That(msg.Length, Is.EqualTo(actualLength));
            Assert.That(type, Is.EqualTo(actualType));

            Assert.That(parametersBytes.Count(), Is.EqualTo(parametersLength));
        }

        [Test]
        public void TranslateMessage_DataItem_ReturnsTrue()
        {
            // Arrange
            int msgHeaderLength = 2;
            int msgSequenceNumberLength = 2;
            var type = NetSdrMessageHelper.MsgTypes.DataItem0;
            byte[] body = new byte[3];
            ushort sequenceNumber = 42;

            byte[] seqBytes = BitConverter.GetBytes(sequenceNumber);
            byte[] msgHeader = BitConverter.GetBytes(
                (ushort)((int)type << 13 | msgHeaderLength + msgSequenceNumberLength + body.Length)
            );
            byte[] msg = msgHeader.Concat(seqBytes).Concat(body).ToArray();

            // Act
            bool success = NetSdrMessageHelper.TranslateMessage(
                msg, out var parsedType, out var parsedCode, out var parsedSeq, out var parsedBody
            );

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(success, Is.True);
                Assert.That(parsedType, Is.EqualTo(type));
                Assert.That(parsedCode, Is.EqualTo(NetSdrMessageHelper.ControlItemCodes.None));
                Assert.That(parsedSeq, Is.EqualTo(sequenceNumber));
                Assert.That(parsedBody, Is.EqualTo(body));
            });
        }

        //TODO: add more NetSdrMessageHelper tests
    }
}