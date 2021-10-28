﻿namespace Filuet.Hardware.Dispensers.SDK.Jofemar.VisionEsPlus.Models
{
    public class EspBeltAddress
    {
        public int Tray { get; internal set; }

        public int Belt { get; internal set; }

        public static implicit operator EspBeltAddress(string address)
        {
            if (address == null || !address.Contains("/"))
                return null;

            string[] blocks = address.Split(new char[] { '/' }, System.StringSplitOptions.RemoveEmptyEntries);

            if (blocks.Length != 3)
                return null;

            if (!int.TryParse(blocks[1], out int tray) ||
                !int.TryParse(blocks[2], out int belt))
                return null;

            return new EspBeltAddress { Tray = tray, Belt = belt };
        }
    }
}