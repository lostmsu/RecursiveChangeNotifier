[![NuGet version](https://badge.fury.io/nu/RecursiveChangeNotifier.svg)](https://badge.fury.io/nu/RecursiveChangeNotifier)

```csharp
var listener = ChangeListener.Create(myViewModel);
listener.PropertyChanged += 
    new PropertyChangedEventHandler(listener_PropertyChanged);
```