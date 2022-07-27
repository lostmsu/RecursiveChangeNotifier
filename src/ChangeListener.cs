namespace ThomasJaworski.ComponentModel
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.ComponentModel;

    using ChangedHandler = PropertyChangedEventHandler<NestedPropertyChangedEventArgs>;

    public abstract class ChangeListener : INotifyNestedPropertyChanged, INotifyPropertyChanged, INotifyCollectionChanged, IDisposable
    {
        #region *** Members ***
        protected string PropertyName;
        #endregion


        #region *** Abstract Members ***
        protected abstract void Unsubscribe();
        #endregion


        #region *** INotifyPropertyChanged Members and Invoker ***
        public event ChangedHandler PropertyChanged;
        private event PropertyChangedEventHandler LegacyPropertyChanged;
        event PropertyChangedEventHandler INotifyPropertyChanged.PropertyChanged{
            add => LegacyPropertyChanged += value;
            remove => LegacyPropertyChanged -= value;
        }

        protected virtual void RaisePropertyChanged(string fullPath, object @object, string propertyName)
        {
            var args = new NestedPropertyChangedEventArgs(fullPath, @object, propertyName);
            PropertyChanged?.Invoke(this, args);
            LegacyPropertyChanged?.Invoke(this, args);
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
