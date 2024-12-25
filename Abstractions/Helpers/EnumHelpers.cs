using Filuet.Hardware.Dispensers.Abstractions.Enums;
using Microsoft.Extensions.Logging;

namespace Filuet.Hardware.Dispensers.Abstractions.Helpers
{
    public static class EnumHelpers
    {
        public static LogLevel ToLogLevel(this DispenserStateSeverity severity) {
            switch (severity) {
                case DispenserStateSeverity.Normal:
                case DispenserStateSeverity.NeedToWait:
                    return LogLevel.Information;
                case DispenserStateSeverity.MaintenanceService:
                case DispenserStateSeverity.Inoperable:
                    return LogLevel.Warning;
                case DispenserStateSeverity.MaintenanceRequired:
                    return LogLevel.Error;
                default: return LogLevel.Information;
            }
        }
    }
}