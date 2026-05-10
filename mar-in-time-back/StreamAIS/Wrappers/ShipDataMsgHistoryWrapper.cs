using AisCommunication.Shared;

namespace StreamAIS.Wrappers
{
    internal class ShipDataMsgHistoryWrapper : AisMsgHistoryWrapper
    {
        public ShipDataMsgHistoryWrapper(AISResult aisMessage) : base(aisMessage)
        {
            MessageTypeId = 5;
        }
    }
}
