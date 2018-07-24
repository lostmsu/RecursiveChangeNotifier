namespace ThomasJaworski.ComponentModel
{
    using System;
    using System.Collections.Specialized;
    using System.ComponentModel;

    public abstract class ChangeListener : INotifyPropertyChanged, INotifyCollectionChanged, IDisposable
    {
        #region *** Members ***
        protected string PropertyName;
        #endregion


        #region *** Abstract Members ***
        protected abstract void Unsubscribe();
        #endregion


        #region *** INotifyPropertyChanged Members and Invoker ***
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        protected virtual void RaiseCollectionChanged(
            INotifyCollectionChanged collection, NotifyCollectionChangedEventArgs args) =>
            this.CollectionChanged?.Invoke(collection, args);
        #endregion


        #region *** Disposable Pattern ***

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                Unsubscribe();
            }
        }

        ~ChangeListener()
        {
            Dispose(false);
        }

        #endregion


        #region *** Factory ***
        public static ChangeListener Create(INotifyPropertyChanged value)
        {
            return Create(value, null);
        }

        public static ChangeListener Create(INotifyPropertyChanged value, string propertyName)
        {
            var trackableCollection = value as INotifyCollectionChanged;
            if (trackableCollection != null)
            {
                return new CollectionChangeListener(trackableCollection, propertyName);
            }
            return value != null ? new ChildChangeListener(value, propertyName) : null;
        }
        #endregion

        #region Debugging
        static volatile bool debugTracing = false;
        public static bool DebugTracing {
            get => debugTracing;
            set => debugTracing = value;
        }
        #endregion
    }
}
