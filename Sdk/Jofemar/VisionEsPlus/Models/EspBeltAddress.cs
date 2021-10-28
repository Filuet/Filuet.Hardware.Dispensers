﻿using Filuet.Hardware.Dispensers.Abstractions.Models;

namespace Filuet.Hardware.Dispensers.SDK.Jofemar.VisionEsPlus.Models
{
    public class EspBeltAddress
    {
        public int Machine { get; internal set; }

        public int Tray { get; internal set; }

        public int Belt { get; internal set; }

        public static implicit operator EspBeltAddress(DispenseAddress address)
        {
            if (address.Address == null || !address.Address.Contains("/"))
                return null;

            string[] blocks = address.Address.Split(new char[] { '/' }, System.StringSplitOptions.RemoveEmptyEntries);

            if (blocks.Length != 4)
                return null;

            if (!int.TryParse(blocks[1], out int machine) || 
                !int.TryParse(blocks[1], out int tray) ||
                !int.TryParse(blocks[2], out int belt))
                return null;

            return new EspBeltAddress { Machine = machine, Tray = tray, Belt = belt };
        }
    }
}