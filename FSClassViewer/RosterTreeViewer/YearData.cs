using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RosterTreeViewer
{
    public class YearData
    {
        public string Year { get; set; }
        public ObservableCollection<MonthData> Months { get; set; } = new ObservableCollection<MonthData>();
    }
}
