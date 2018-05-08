namespace Tests
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using ThomasJaworski.ComponentModel;

    [TestClass]
    public class NotificationTests
    {
        class NotifyPropertyChangedBase : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler PropertyChanged;
            protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
            {
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        class SimpleClassWithObservableCollection: NotifyPropertyChangedBase
        {
            public ObservableCollection<string> Strings { get; } = new ObservableCollection<string>();
        }

        class DeeperObservableCollection : NotifyPropertyChangedBase
        {
            public ObservableCollection<SimpleClassWithObservableCollection> Nested { get; } = new ObservableCollection<SimpleClassWithObservableCollection>();
        }

        [TestMethod]
        public void ObserbableCollections()
        {
            var collectionContainer = new SimpleClassWithObservableCollection();
            using (var changeNotifier = ChangeListener.Create(collectionContainer)) {
                string detectedProperty = null;
                INotifyCollectionChanged detectedCollection = null;
                string[] detectedNewItems = null;
                changeNotifier.PropertyChanged += (sender, args) => detectedProperty = args.PropertyName;
                changeNotifier.CollectionChanged += (collection, args) => {
                    detectedCollection = (INotifyCollectionChanged)collection;
                    detectedNewItems = args.NewItems.Cast<string>().ToArray();
                };
                collectionContainer.Strings.Add("A string");
                Assert.IsNull(detectedProperty);
                Assert.AreSame(collectionContainer.Strings, detectedCollection);
                CollectionAssert.AreEquivalent(collectionContainer.Strings, detectedNewItems);
            }
        }

        [TestMethod]
        public void DeeperObservableCollections() {
            var nested = new SimpleClassWithObservableCollection();
            var outer = new DeeperObservableCollection();
            using (var changeNotifier = ChangeListener.Create(outer)) {
                string detectedProperty = null;
                INotifyCollectionChanged detectedCollection = null;
                IList detectedNewItems = null;
                changeNotifier.PropertyChanged += (sender, args) => detectedProperty = args.PropertyName;
                int collectionChanges = 0;
                changeNotifier.CollectionChanged += (collection, args) => {
                    detectedCollection = (INotifyCollectionChanged)collection;
                    detectedNewItems = args.NewItems;
                    collectionChanges++;
                };

                outer.Nested.Add(nested);

                Assert.AreSame(outer.Nested, detectedCollection);
                CollectionAssert.AreEquivalent(outer.Nested, detectedNewItems);
                Assert.AreEqual(1, collectionChanges);

                nested.Strings.Add("A string");

                Assert.AreSame(nested.Strings, detectedCollection);
                CollectionAssert.AreEquivalent(nested.Strings, detectedNewItems);
                Assert.AreEqual(2, collectionChanges);

                Assert.IsNull(detectedProperty);
            }
        }
    }
}
