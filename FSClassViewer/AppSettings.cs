using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FSClassViewer
{
    public class AppSettings : ApplicationSettingsBase
    {
        public AppSettings()
            : base("SampleCode")
        {
        }

        [UserScopedSetting]
        public string LastFilePath
        {
            get
            {
                try
                {
                    if (this["LastFilePath"] != null)
                    {
                        return ((string)this["LastFilePath"]);
                    }
                }
                catch (Exception)
                {
                    return string.Empty;
                }
                return string.Empty;
            }
            set
            {
                this["LastFilePath"] = value;
            }
        }


        [UserScopedSetting]
        public ObservableCollection<RecentClass> RecentClasses
        {
            get
            {
                try
                {
                    if (this["RecentClasses"] != null)
                    {
                        return ((ObservableCollection<RecentClass>)this["RecentClasses"]);
                    }
                }
                catch (Exception)
                {
                    return new ObservableCollection<RecentClass>();
                }
                return new ObservableCollection<RecentClass>();
            }
            set
            {
                this["RecentClasses"] = value;
            }
        }

        [UserScopedSetting]
        public string RosterRootPath
        {
            get
            {
                try
                {
                    if (this["RosterRootPath"] != null)
                    {
                        return ((string)this["RosterRootPath"]);
                    }
                }
                catch (Exception)
                {
                    return string.Empty;
                }
                return string.Empty;
            }
            set
            {
                this["RosterRootPath"] = value;
            }
        }

        [UserScopedSetting]
        public string GradingRootPath
        {
            get
            {
                try
                {
                    if (this["GradingRootPath"] != null)
                    {
                        return ((string)this["GradingRootPath"]);
                    }
                }
                catch (Exception)
                {
                    return string.Empty;
                }
                return string.Empty;
            }
            set
            {
                this["GradingRootPath"] = value;
            }
        }

        [UserScopedSetting]
        public string ContactMessage
        {
            get
            {
                try
                {
                    if (this["ContactMessage"] != null)
                    {
                        return ((string)this["ContactMessage"]);
                    }
                }
                catch (Exception)
                {
                    return string.Empty;
                }
                return string.Empty;
            }
            set
            {
                this["ContactMessage"] = value;
            }
        }

        [UserScopedSetting]
        public string ContactList
        {
            get
            {
                try
                {
                    if (this["ContactList"] != null)
                    {
                        return ((string)this["ContactList"]);
                    }
                }
                catch (Exception)
                {
                    return string.Empty;
                }
                return string.Empty;
            }
            set
            {
                this["ContactList"] = value;
            }
        }
    }
}
