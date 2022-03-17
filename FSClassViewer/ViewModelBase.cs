using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace FSClassViewer
{
    public class ViewModelBase : INotifyPropertyChanged, IDisposable
    {
        #region Fields
        protected object _myContext;
        private bool _disposed;
        #endregion

        #region Properties
        public string HelpContext { get; set; }
        public object MyContext
        {
            get
            {
                return _myContext;
            }
            set
            {
                if (_myContext != value)
                {
                    _myContext = value;
                    OnPropertyChanged();
                }
            }
        }
        #endregion

        public ViewModelBase()
        {
            Initializer();
        }

        protected bool IsValidContext(object param)
        {
            return param == MyContext;
        }

        protected void Initializer()
        {
            HelpContext = string.Empty;
        }

        public virtual void PersistSettings()
        {
        }

        #region Property Change
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propName = null)
        {
            PropertyChangedEventHandler ph = this.PropertyChanged;
            if (ph != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
            }
        }
        #endregion

        #region Disposable
        ~ViewModelBase()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public virtual void Dispose(bool disposing)
        {
            if (_disposed) return;
            if (disposing)
            {
            }
            _disposed = true;
        }
        #endregion
    }
}
