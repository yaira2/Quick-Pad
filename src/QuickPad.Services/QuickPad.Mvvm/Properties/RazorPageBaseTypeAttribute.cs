using System;

namespace QuickPad.Mvvm.Properties
{
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public sealed class RazorPageBaseTypeAttribute : Attribute
    {
        public RazorPageBaseTypeAttribute([NotNull] string baseType)
        {
            BaseType = baseType;
        }
        public RazorPageBaseTypeAttribute([NotNull] string baseType, string pageName)
        {
            BaseType = baseType;
            PageName = pageName;
        }

        [NotNull] public string BaseType { get; }
        [CanBeNull] public string PageName { get; }
    }
}