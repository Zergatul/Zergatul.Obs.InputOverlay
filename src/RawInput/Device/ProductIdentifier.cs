using System.Collections.Generic;

namespace Zergatul.Obs.InputOverlay.RawInput.Device
{
    public static class ProductIdentifier
    {
        private static readonly Dictionary<int, Dictionary<int, string>> _vendors = new Dictionary<int, Dictionary<int, string>>
        {
            // Microsoft Corporation
            [0x045E] = new Dictionary<int,string>
            {
                [0x0202] = "Xbox Controller",
                [0x0285] = "Xbox Controller S",
                [0x0289] = "Xbox Controller S",
                [0x028E] = "Xbox360 Controller",
                [0x028F] = "Xbox360 Wireless Controller",
                [0x02D1] = "Xbox One Controller",
                [0x02DD] = "Xbox One Controller (Firmware 2015)",
                [0x02E3] = "Xbox One Elite Controller",
                [0x02EA] = "Xbox One S Controller",
                [0x02FD] = "Xbox One S Controller [Bluetooth]",
                [0x0B12] = "Xbox Wireless Controller (Model 1914)",
                [0x0B13] = "Xbox Wireless Controller (Model 1914)",
            },

            // Sony Group Corporation
            [0x054C] = new Dictionary<int, string>
            {
                [0x0268] = "PlayStation 3 Controller",
                [0x05C4] = "DualShock 4",
                [0x0BA0] = "DualShock 4 Wireless Adapter",
                [0x0CDA] = "PlayStation Classic Controller",
                [0x0CE6] = "DualSense Wireless Controller"
            },
        };

        public static string Get(int vendorId, int productId)
        {
            if (_vendors.TryGetValue(vendorId, out var products))
            {
                if (products.TryGetValue(productId, out string product))
                {
                    return product;
                }
            }

            return "Unknown";
        }
    }
}