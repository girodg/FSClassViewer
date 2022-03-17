using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RosterTreeViewer
{
    public class MonthData
    {
        public string Month { get; set; }
        public ObservableCollection<ClassData> Classes { get; set; } = new ObservableCollection<ClassData>();
    }
}
