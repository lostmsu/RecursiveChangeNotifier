namespace ThomasJaworski.ComponentModel;
public interface INotifyNestedPropertyChanged {
    event PropertyChangedEventHandler<NestedPropertyChangedEventArgs> PropertyChanged;
}
