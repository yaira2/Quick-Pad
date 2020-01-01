using System;

namespace QuickPad.Mvvm.Properties
{
    /// <summary>
    /// ASP.NET MVC attribute. Indicates that the marked parameter is an MVC template.
    /// Use this attribute for custom wrappers similar to
    /// <c>System.ComponentModel.DataAnnotations.UIHintAttribute(System.String)</c>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Field | AttributeTargets.Property)]
    public sealed class AspMvcTemplateAttribute : Attribute { }
}