using System;

namespace Filuet.Hardware.Dispensers.Abstractions.Enums
{
    [Flags]
    public enum DispenserStateSeverity
    {
        Normal = 0,
        /// <summary>
        /// Maintenance required, but can still work
        /// </summary>
        MaintenanceRequired = 2,
        /// <summary>
        /// Repair or urgent service required
        /// </summary>
        Inoperable = 1,
        /// <summary>
        /// The device is busy. Wait for some time
        /// </summary>
        NeedToWait = 3,
        /// <summary>
        /// Under maintenance
        /// </summary>
        MaintenanceService = 4
    }
}