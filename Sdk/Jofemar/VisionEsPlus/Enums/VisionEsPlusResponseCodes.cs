using System;
using System.Collections.Generic;
using System.Text;

namespace Filuet.Hardware.Dispensers.SDK.Jofemar.VisionEsPlus.Enums
{
    public enum VisionEsPlusResponseCodes : byte
    {
        Unknown = 0,
        Ok = 0x06,
        Ready = 0x30,
        Busy = 0x31,
        InvalidTrayRequested = 0x32,
        InvalidChannelRequested = 0x33,
        EmptyChannel = 0x34,
        FaultInElevatorMotor = 0x35,
        FaultInElevatorBeltOrPhotosensors = 0x36,
        FaultInCabinetsPhototransistors = 0x37,
        NoChannelsDetected = 0x38,
        FaultInProductDetector = 0x39,
        FaultIn485Bus = 0x41,
        ProductUnderTheElevator = 0x42,
        ErrorWhenElevatorApproachingToAPosition = 0x43,
        FaultInKeyboard = 0x44,
        EepromWritingError = 0x45,
        FaultCommunicationWithTemperatureControl = 0x46,
        ThermometerDisconnected = 0x47,
        ThermometerProgrammingLost = 0x48,
        ThermometerFaulty = 0x49,
        ChannelsPowerConsumptionDetectorFaulty = 0x4a,
        ElevatorDoesNotFoundChannelOrTray = 0x4b,
        ElevatorDoesNotFoundDeliveryProductPosition = 0x4c,
        InteriorOfTheElevatorBlocked = 0x4d,
        ErrorInTesterOfProductDetector = 0x4e,
        WaitingForProductToBeRemoved = 0x4f,
        ProductExpiredBecauseOfTemperature = 0x50
    }
}
