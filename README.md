```csharp
var listener = ChangeListener.Create(myViewModel);
listener.PropertyChanged += 
    new PropertyChangedEventHandler(listener_PropertyChanged);
```