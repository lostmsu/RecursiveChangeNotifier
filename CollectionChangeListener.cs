namespace ThomasJaworski.ComponentModel
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Diagnostics;

    public class CollectionChangeListener : ChangeListener
    {
        #region *** Members ***
        private readonly INotifyCollectionChanged value;
        private readonly Dictionary<INotifyPropertyChanged, ChangeListener> collectionListeners = new Dictionary<INotifyPropertyChanged, ChangeListener>();
        #endregion


        #region *** Constructors ***
        public CollectionChangeListener(INotifyCollectionChanged collection, string propertyName)
        {
            value = collection;
            PropertyName = propertyName;

            Subscribe();
        }
        #endregion


        #region *** Private Methods ***
        private void Subscribe()
        {
            value.CollectionChanged += value_CollectionChanged;

            foreach (INotifyPropertyChanged item in (IEnumerable)value)
            {
                ResetChildListener(item);
            }
        }

        private void ResetChildListener(INotifyPropertyChanged item)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            RemoveItem(item);

            // Add new
            var trackableCollection = item as INotifyCollectionChanged;
            var listener = trackableCollection != null
                ? (ChangeListener)new CollectionChangeListener(trackableCollection, PropertyName)
                : new ChildChangeListener(item);

            listener.PropertyChanged += listener_PropertyChanged;
            collectionListeners.Add(item, listener);
        }

        private void RemoveItem(INotifyPropertyChanged item)
        {
            // Remove old
            if (collectionListeners.ContainsKey(item))
            {
                collectionListeners[item].PropertyChanged -= listener_PropertyChanged;

                collectionListeners[item].Dispose();
                collectionListeners.Remove(item);
            }
        }


        private void ClearCollection()
        {
            foreach (var key in collectionListeners.Keys)
            {
                collectionListeners[key].Dispose();
            }

            collectionListeners.Clear();
        }
        #endregion


        #region *** Event handlers ***
        void value_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                ClearCollection();
            }
            else
            {
                // Don't care about e.Action, if there are old items, Remove them...
                if (e.OldItems != null)
                {
                    foreach (INotifyPropertyChanged item in e.OldItems)
                        RemoveItem(item);
                }

                // ...add new items as well
                if (e.NewItems != null)
                {
                    foreach (INotifyPropertyChanged item in e.NewItems)
                        ResetChildListener(item);
                }
            }
        }


        void listener_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // ...then, notify about it
            // ReSharper disable once ExplicitCallerInfoArgument
            RaisePropertyChanged($"{PropertyName}{(PropertyName != null ? "[]." : null)}{e.PropertyName}");
        }
        #endregion


        #region *** Overrides ***
        /// <summary>
        /// Releases all collection item handlers and self handler
        /// </summary>
        protected override void Unsubscribe()
        {
            ClearCollection();

            value.CollectionChanged -= value_CollectionChanged;

            Debug.WriteLine("CollectionChangeListener unsubscribed");
        }
        #endregion
    }
}
