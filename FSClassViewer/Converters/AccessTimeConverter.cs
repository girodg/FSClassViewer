using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace FSClassViewer
{
    class AccessTimeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null || string.IsNullOrEmpty(value.ToString()))
            {
                return null;
            }
            string lastAccess = value as string;
            if(lastAccess.Equals(@"N\A", StringComparison.InvariantCultureIgnoreCase))
            {
                return lastAccess;
            }
            if(DateTime.TryParse(lastAccess, out DateTime access))
            {
                TimeSpan ts = DateTime.Now - access;
                string ago = string.Empty;
                if(ts.Days>0)
                {
                    string plural = (ts.Days > 1) ? "days" : "day";
                    ago = $"{ts.Days} {plural} ago";
                }
                else if(ts.Hours > 0)
                {
                    string plural = (ts.Hours > 1) ? "hours" : "hour";
                    ago = $"{ts.Hours} {plural} ago";
                }
                else if (ts.Minutes > 0)
                {
                    string plural = (ts.Minutes > 1) ? "minutes" : "minute";
                    ago = $"{ts.Minutes} {plural} ago";
                }
                else 
                {
                    string plural = (ts.Seconds > 1) ? "seconds" : "second";
                    ago = $"{ts.Seconds} {plural} ago";
                }
                return $"{lastAccess}\n{ago}";
            }
            return @"N\A";
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
