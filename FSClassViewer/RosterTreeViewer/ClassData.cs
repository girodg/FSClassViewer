using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RosterTreeViewer
{
    public class ClassData
    {
        public ObservableCollection<string> Rosters { get; set; } = new ObservableCollection<string>();

        public string ClassName { get; set; }
    }

    public class ClassInformation
    {
        public string ClassCode { get; set; }
        public string Shortcut { get; set; }
        public string ClassType { get; set; }
    }
}
