namespace ThomasJaworski.ComponentModel;

using System;
using System.ComponentModel;

public class NestedPropertyChangedEventArgs : PropertyChangedEventArgs {
    public NestedPropertyChangedEventArgs(string fullPath, object @object, string propertyName) : base(propertyName) {
        this.FullPath = fullPath ?? throw new ArgumentNullException(nameof(fullPath));
        this.Object = @object ?? throw new ArgumentNullException(nameof(@object));
    }
    /// <summary>
    /// Object, whose property changed
    /// </summary>
    public object Object { get; }
    /// <summary>
    /// Full path to the property from the <see cref="Root"/> of the object hierarchy
    /// </summary>
    public string FullPath { get; }
}
