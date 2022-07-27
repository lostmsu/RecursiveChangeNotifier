namespace ThomasJaworski.ComponentModel;

using System.ComponentModel;

public delegate void PropertyChangedEventHandler<in TArgs>(object sender, TArgs e)
    where TArgs : PropertyChangedEventArgs;
