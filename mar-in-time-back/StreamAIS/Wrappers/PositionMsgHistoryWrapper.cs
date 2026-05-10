using AisCommunication.Shared;

namespace StreamAIS.Wrappers
{
    internal class PositionMsgHistoryWrapper : AisMsgHistoryWrapper
    {
        public PositionMsgHistoryWrapper(AISResult aisMessage) : base(aisMessage)
        {
            MessageTypeId = 1;
        }
    }
}
