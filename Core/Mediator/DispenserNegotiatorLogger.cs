using System;

namespace Filuet.Hardware.Dispensers.Core.Mediator
{
    public class DispenserNegotiatorLogger
    {
        public event EventHandler<string> onSendCommand;
        public event EventHandler<string> onResponse;
    }
}
