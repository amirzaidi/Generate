using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Generate
{
    static class Extentions
    {
        internal static byte[] AsciiBytes(this string In)
        {
            return Encoding.ASCII.GetBytes(In);
        }
    }
}
