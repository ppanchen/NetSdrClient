using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetSdrClientApp
{
    public static class NetSdrMessage
    {
        private const short _maxMessageLength = 8191;

        public enum MsgTypes
        {
            SetControlItem,
            CurrentControlItem,
            ControlItemRange,
            Ack,
            DataItem0,
            DataItem1,
            DataItem2,
            DataItem3
        }

        public enum ControlItemCodes
        {
            None = 0,
            ReceiverState = 0x0018,
            ReceiverFrequency = 0x0020
        }

        public static byte[] GetControlItemMessage(MsgTypes type, ControlItemCodes itemCode, byte[] parameters)
        {
            return GetMessage(type, itemCode, parameters);
        }

        public static byte[] GetDataItemMessage(MsgTypes type, byte[] parameters)
        {
            return GetMessage(type, ControlItemCodes.None, parameters);
        }

        private static byte[] GetMessage(MsgTypes type, ControlItemCodes itemCode, byte[] parameters)
        {
            var itemCodeBytes = Array.Empty<byte>();
            if (itemCode != ControlItemCodes.None)
            {
                itemCodeBytes = BitConverter.GetBytes((ushort)itemCode);
            }

            var headerBytes = GetHeader(type, itemCodeBytes.Length + parameters.Length);

            List<byte> msg = new List<byte>();
            msg.AddRange(headerBytes);
            msg.AddRange(itemCodeBytes);
            msg.AddRange(parameters);

            return msg.ToArray();
        }

        private static byte[] GetHeader(MsgTypes type, int msgLength)
        {
            int lengthWithHeader = msgLength + 2;

            if (msgLength < 0 || lengthWithHeader > _maxMessageLength)
            {
                throw new ArgumentException("Message length exceeds allowed value");
            }

            return BitConverter.GetBytes((ushort)(lengthWithHeader + ((int)type << 13)));
        }

    }
}
