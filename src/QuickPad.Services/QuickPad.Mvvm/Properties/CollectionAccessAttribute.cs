using System;

namespace QuickPad.Mvvm.Properties
{
    /// <summary>
    /// Indicates how method, constructor invocation, or property access
    /// over collection type affects the contents of the collection.
    /// Use <see cref="CollectionAccessType"/> to specify the access type.
    /// </summary>
    /// <remarks>
    /// Using this attribute only makes sense if all collection methods are marked with this attribute.
    /// </remarks>
    /// <example><code>
    /// public class MyStringCollection : List&lt;string&gt;
    /// {
    ///   [CollectionAccess(CollectionAccessType.Read)]
    ///   public string GetFirstString()
    ///   {
    ///     return this.ElementAt(0);
    ///   }
    /// }
    /// class Test
    /// {
    ///   public void Foo()
    ///   {
    ///     // Warning: Contents of the collection is never updated
    ///     var col = new MyStringCollection();
    ///     string x = col.GetFirstString();
    ///   }
    /// }
    /// </code></example>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor | AttributeTargets.Property)]
    public sealed class CollectionAccessAttribute : Attribute
    {
        public CollectionAccessAttribute(CollectionAccessType collectionAccessType)
        {
            CollectionAccessType = collectionAccessType;
        }

        public CollectionAccessType CollectionAccessType { get; }
    }
}