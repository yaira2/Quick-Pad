using System;

namespace QuickPad.Mvvm.Properties
{
    /// <summary>
    /// ASP.NET MVC attribute. If applied to a parameter, indicates that the parameter is
    /// an MVC controller. If applied to a method, the MVC controller name is calculated
    /// implicitly from the context. Use this attribute for custom wrappers similar to
    /// <c>System.Web.Mvc.Html.ChildActionExtensions.RenderAction(HtmlHelper, String, String)</c>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Method | AttributeTargets.Field | AttributeTargets.Property)]
    public sealed class AspMvcControllerAttribute : Attribute
    {
        public AspMvcControllerAttribute() { }

        public AspMvcControllerAttribute([NotNull] string anonymousProperty)
        {
            AnonymousProperty = anonymousProperty;
        }

        [CanBeNull] public string AnonymousProperty { get; }
    }
}