using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExampleBot
{
    public static class Utils
    {
        public static byte[] HexToBytes(string hexString)
        {
            if ((hexString.Length & 1) != 0)
            {
                throw new ArgumentException("Input must have even number of characters");
            }
            byte[] ret = new byte[hexString.Length / 2];
            for (int i = 0; i < ret.Length; i++)
            {
                int high = hexString[i * 2];
                int low = hexString[i * 2 + 1];
                high = (high & 0xf) + ((high & 0x40) >> 6) * 9;
                low = (low & 0xf) + ((low & 0x40) >> 6) * 9;

                ret[i] = (byte)((high << 4) | low);
            }

            return ret;
        }
    }
}
