namespace ThomasJaworski.ComponentModel
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;

    public class ChildChangeListener : ChangeListener
    {
        #region *** Members ***
        private readonly INotifyPropertyChanged value;
        private readonly Type type;
        private readonly Dictionary<string, ChangeListener> childListeners = new Dictionary<string, ChangeListener>();
        #endregion


        #region *** Constructors ***
        public ChildChangeListener(INotifyPropertyChanged instance)
        {
            if (instance == null)
                throw new ArgumentNullException(nameof(instance));

            Debug.WriteLineIf(DebugTracing, $"creating {nameof(ChildChangeListener)} for {instance}");

            value = instance;
            type = value.GetType();

            Subscribe();
        }

        public ChildChangeListener(INotifyPropertyChanged instance, string propertyName)
            : this(instance)
        {
            PropertyName = propertyName;
        }
        #endregion


        #region *** Private Methods ***
        private void Subscribe()
        {
            if (value is INotifyNestedPropertyChanged nested)
                nested.PropertyChanged += value_NestedPropertyChanged;
            else
                value.PropertyChanged += value_PropertyChanged;

            foreach (var property in type.GetTypeInfo().DeclaredProperties)
            {
                if (!IsPubliclyReadable(property))
                    continue;
                if (!IsNotifier(property.GetValue(obj: this.value)))
                    continue;

                ResetChildListener(property.Name);
            }
        }

        private static bool IsPubliclyReadable(PropertyInfo prop) => (prop.GetMethod?.IsPublic ?? false) && !prop.GetMethod.IsStatic;
        static bool IsNotifier(object value) => (value is INotifyCollectionChanged) || (value is INotifyPropertyChanged);

        /// <summary>
        /// Resets known (must exist in children collection) child event handlers
        /// </summary>
        /// <param name="propertyName">Name of known child property</param>
        private void ResetChildListener(string propertyName)
        {
            ChangeListener listener;
            // Unsubscribe if existing
            if (childListeners.TryGetValue(propertyName, out listener)
                && listener != null)
            {
                listener.PropertyChanged -= child_PropertyChanged;

                // Should unsubscribe all events
                listener.Dispose();
                listener = null;
                childListeners.Remove(propertyName);
            }

            PropertyInfo property = this.type.GetProperty(propertyName);
            if (property == null)
                throw new InvalidOperationException(
                    $"Was unable to get '{propertyName}' property information from Type '{type.Name}'");

            object newValue = property.GetValue(value, null);

            // Only recreate if there is a new value
            if (newValue != null)
            {
                if (newValue is INotifyCollectionChanged)
                {
                    listener = childListeners[propertyName] =
                        new CollectionChangeListener(newValue as INotifyCollectionChanged, propertyName);
                }
                else if (newValue is INotifyPropertyChanged)
                {
                    listener = childListeners[propertyName] =
                        new ChildChangeListener(newValue as INotifyPropertyChanged, propertyName);
                }
                else
                {
                    Debug.WriteLineIf(DebugTracing, $"not listening to {propertyName} as {newValue} is {newValue.GetType()}");
                }

                if (listener != null) {
                    listener.PropertyChanged += child_PropertyChanged;
                    listener.CollectionChanged += child_CollectionChanged;
                }
            }
        }
        #endregion


        #region *** Event Handler ***
        void child_PropertyChanged(object sender, NestedPropertyChangedEventArgs e)
        {
            RaisePropertyChanged(fullPath: e.FullPath, e.Object, e.PropertyName);
        }

        void child_CollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            this.RaiseCollectionChanged((INotifyCollectionChanged)sender, args);
        }

        void value_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // First, reset child on change, if required...
            ResetChildListener(e.PropertyName);

            // ...then, notify about it
            RaisePropertyChanged(fullPath: e.PropertyName, value, propertyName: e.PropertyName);
        }

        void value_NestedPropertyChanged(object sender, NestedPropertyChangedEventArgs e)
        {
            // First, reset child on change, if required...
            ResetChildListener(e.PropertyName);

            // ...then, notify about it
            RaisePropertyChanged(fullPath: e.FullPath, e.Object, propertyName: e.PropertyName);
        }

        protected override void RaisePropertyChanged(string fullPath, object @object, string propertyName)
        {
            // Special Formatting
            base.RaisePropertyChanged($"{PropertyName}{(PropertyName != null ? "." : null)}{fullPath}", @object, propertyName);
        }
        #endregion


        #region *** Overrides ***
        /// <summary>
        /// Release all child handlers and self handler
        /// </summary>
        protected override void Unsubscribe()
        {
            if (value is INotifyNestedPropertyChanged nested)
                nested.PropertyChanged -= value_NestedPropertyChanged;
            else
                value.PropertyChanged -= value_PropertyChanged;

            foreach (var binderKey in childListeners.Keys)
            {
                if (childListeners[binderKey] != null)
                    childListeners[binderKey].Dispose();
            }

            childListeners.Clear();

            Debug.WriteLineIf(DebugTracing, $"ChildChangeListener '{PropertyName}' unsubscribed");
        }
        #endregion
    }
}
