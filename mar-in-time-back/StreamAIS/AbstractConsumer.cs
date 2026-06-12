using Microsoft.Extensions.Configuration;

namespace StreamAIS
{
    internal abstract class AbstractConsumer
    {
        protected AbstractConsumer(IConfiguration configuration) { }

        public abstract Task ConsumingLoop();
    }
}
