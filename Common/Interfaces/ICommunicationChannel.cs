namespace Filuet.Hardware.Dispensers.Common.Interfaces
{
    public interface ICommunicationChannel
    {
        byte[] SendCommand(byte[] data);
    }
}