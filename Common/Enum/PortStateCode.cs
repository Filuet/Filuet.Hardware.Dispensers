namespace Filuet.Hardware.Dispensers.Common.Enums
{
    /// <summary>
    /// Port state codes
    /// </summary>
    public enum PortStateCode
    {
        Success = 0,
        PortClosed = 1,
        Timeout = 2,
        /// <summary>
        /// Checksum error
        /// </summary>
        CrcFailed = 3,
        PortDoesNotExists = 4,
        Failure = 5
    }
}