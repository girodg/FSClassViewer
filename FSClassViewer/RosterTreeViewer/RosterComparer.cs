using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RosterTreeViewer
{
    public class RosterComparer : IComparer<string>
    {
        public int Compare(string x, string y)
        {
            int start = x.LastIndexOf(Path.DirectorySeparatorChar) + 1;
            //compare the first parts: C202011 vs C202012
            string month1 = x.Substring(start, 7);
            string month2 = y.Substring(start, 7);

            int monthComp = month1.CompareTo(month2);
            if (monthComp != 0)
                return -monthComp;

            string rest1 = x.Substring(8);
            string rest2 = y.Substring(8);
            return rest1.CompareTo(rest2);
        }
    }
}
