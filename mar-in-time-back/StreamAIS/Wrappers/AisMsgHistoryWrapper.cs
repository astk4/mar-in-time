using AisCommunication.Shared;

namespace StreamAIS.Wrappers
{
    internal abstract class AisMsgHistoryWrapper
    {
        public byte MessageTypeId { get; protected set; }

        protected AISResult aisMessage;
        protected AisMsgHistoryWrapper(AISResult aisMessage) => this.aisMessage = aisMessage;

        public static AisMsgHistoryWrapper GetInstance(AISResult aisMessage)
        {
            if (aisMessage.Position != null)
            {
                return new PositionMsgHistoryWrapper(aisMessage);
            }
            if (aisMessage.ShipData != null)
            {
                return new ShipDataMsgHistoryWrapper(aisMessage);
            }
            if (aisMessage.Safety != null)
            {
                return new SafetyMsgHistoryWrapper(aisMessage);
            }
            return null;
        }
    }
}
