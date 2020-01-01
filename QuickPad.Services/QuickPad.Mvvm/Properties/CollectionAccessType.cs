using System;

namespace QuickPad.Mvvm.Properties
{
    /// <summary>
    /// Provides a value for the <see cref="CollectionAccessAttribute"/> to define
    /// how the collection method invocation affects the contents of the collection.
    /// </summary>
    [Flags]
    public enum CollectionAccessType
    {
        /// <summary>Method does not use or modify content of the collection.</summary>
        None = 0,
        /// <summary>Method only reads content of the collection but does not modify it.</summary>
        Read = 1,
        /// <summary>Method can change content of the collection but does not add new elements.</summary>
        ModifyExistingContent = 2,
        /// <summary>Method can add new elements to the collection.</summary>
        UpdatedContent = ModifyExistingContent | 4
    }
}