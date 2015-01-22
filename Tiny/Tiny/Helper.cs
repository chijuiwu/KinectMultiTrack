using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tiny
{
    internal class Helper
    {
        public static int Count(IEnumerable<Object> objects)
        {
            int total = 0;
            foreach (Object obj in objects)
            {
                total++;
            }
            return total;
        }
    }
}
