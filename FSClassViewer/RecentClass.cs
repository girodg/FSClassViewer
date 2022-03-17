using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FSClassViewer
{
    public class RecentClass : ModelBase
    {
		private string _fileName = string.Empty;
		private string _filePath = string.Empty;

        public string FileName
		{
			get { return _fileName; }
			set
			{
				_fileName = value;
				OnPropertyChanged();
			}
		}
		public string FilePath
		{
			get { return _filePath; }
			set
			{
				_filePath = value;
				OnPropertyChanged();
			}
		}
	}
}
