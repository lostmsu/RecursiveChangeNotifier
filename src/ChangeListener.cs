namespace ThomasJaworski.ComponentModel
{
    using System;
    using System.Collections.Specialized;
    using System.ComponentModel;

    public abstract class ChangeListener : INotifyPropertyChanged, IDisposable
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
    }
}
