using AisCommunication.Shared;

namespace StreamAIS.Wrappers
{
    internal class SafetyMsgHistoryWrapper : AisMsgHistoryWrapper
    {
        public SafetyMsgHistoryWrapper(AISResult aisMessage) : base(aisMessage)
        {
            MessageTypeId = 12;
        }
    }
}
