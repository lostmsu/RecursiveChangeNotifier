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
            int @int;
            public ObservableCollection<string> Strings { get; } = new ObservableCollection<string>();
            public int Int {
                get => this.@int;
                set {
                    if (value == this.@int)
                        return;

                    this.@int = value;
                    this.OnPropertyChanged();
                }
            }
        }

        class NestedClass : NotifyPropertyChangedBase {
            SimpleClassWithObservableCollection nested;
            
            public SimpleClassWithObservableCollection Nested {
                get => this.nested;
                set {
                    if (value == this.nested)
                        return;

                    this.nested = value;
                    this.OnPropertyChanged();
                }
            }
        }

        class DeeperObservableCollection : NotifyPropertyChangedBase
        {
            public ObservableCollection<SimpleClassWithObservableCollection> Nested { get; } = new ObservableCollection<SimpleClassWithObservableCollection>();
        }

        [TestMethod]
        public void ObservableCollections()
        {
            var collectionContainer = new SimpleClassWithObservableCollection();
            using (var changeNotifier = ChangeListener.Create(collectionContainer)) {
                string detectedProperty = null;
                string detectedPath = null;
                INotifyCollectionChanged detectedCollection = null;
                string[] detectedNewItems = null;
                changeNotifier.PropertyChanged += (sender, args) => {
                    detectedProperty = args.PropertyName;
                    detectedPath = args.FullPath;
                };
                changeNotifier.CollectionChanged += (collection, args) => {
                    detectedCollection = (INotifyCollectionChanged)collection;
                    detectedNewItems = args.NewItems.Cast<string>().ToArray();
                };
                collectionContainer.Strings.Add("A string");
                Assert.IsNull(detectedProperty);
                Assert.IsNull(detectedPath);
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
                string detectedPath = null;
                INotifyCollectionChanged detectedCollection = null;
                IList detectedNewItems = null;
                changeNotifier.PropertyChanged += (sender, args) => {
                    detectedProperty = args.PropertyName;
                    detectedPath = args.FullPath;
                };
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

                nested.Int = 42;
                Assert.AreEqual($"{nameof(DeeperObservableCollection.Nested)}[].{nameof(SimpleClassWithObservableCollection.Int)}", detectedPath);
                Assert.AreEqual(nameof(SimpleClassWithObservableCollection.Int), detectedProperty);
            }
        }

        [TestMethod]
        public void NestedObject() {
            var root = new NestedClass {
                Nested = new(),
            };
            using var changeNotifier = ChangeListener.Create(root);
            NestedPropertyChangedEventArgs detected = null;
            changeNotifier.PropertyChanged += (sender, args) => {
                detected = args;
            };

            root.Nested.Int = 42;

            Assert.IsNotNull(detected);
            Assert.AreEqual($"{nameof(NestedClass.Nested)}.{nameof(SimpleClassWithObservableCollection.Int)}", detected.FullPath);
            Assert.AreEqual(nameof(SimpleClassWithObservableCollection.Int), detected.PropertyName);
            Assert.AreSame(root.Nested, detected.Object);
        }
    }
}
