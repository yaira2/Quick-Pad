using System;
using System.Collections.Generic;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Markup;
using Windows.UI.Xaml.Media;
using System.Linq;
using QuickPad.Mvvm.Models;
using QuickPad.UI;
using QuickPad.UI.Common;

namespace QuickPad.UI.Converters
{
    public static class Converter
    {
        public static TextWrapping BoolToTextWrap(bool input)
        {
            return input ? TextWrapping.Wrap : TextWrapping.NoWrap;
        }

        /// <summary>
        /// Use to convert on XAML & x:Bind from bool (or Boolean) to Visibility
        /// </summary>
        /// <param name="input">Any boolean input</param>
        /// <returns>Visibility if true return Visible otherwise return Collapsed</returns>
        public static Visibility BoolToVisibility(bool input)
        {
            return input ? Visibility.Visible : Visibility.Collapsed;
        }

        /// <summary>
        /// Use to convert on XAML & x:Bind from bool (or Boolean) to Visibility
        /// But instead of true=visible, it opposite
        /// </summary>
        /// <param name="input">Any boolean input</param>
        /// <returns></returns>
        public static Visibility BoolToVisibilityInvert(bool input)
        {
            return input ? Visibility.Collapsed : Visibility.Visible;
        }

        public static Visibility HideIfNoBulletOptionsShow(bool bullet, bool bold, bool strikethrough, bool underline, bool italic)
        {
            if (!bullet && !bold && !strikethrough && !underline && !italic)
            {
                return Visibility.Collapsed;
            }
            return Visibility.Visible;
        }

        public static Visibility HideIfNoAlignButtonShow(bool left, bool center, bool right, bool justify)
        {
            if (!left && !center && !right && !justify)
            {
                return Visibility.Collapsed;
            }
            return Visibility.Visible;
        }

        public static Visibility HideIfNoFormatsButtonShow(bool font, bool color, bool emoji)
        {
            if (!font && !color && !emoji)
            {
                return Visibility.Collapsed;
            }
            return Visibility.Visible;
        }

        public static Visibility HideIfNoSizeButtonShow(bool up, bool down)
        {
            if (!up && !down)
            {
                return Visibility.Collapsed;
            }
            return Visibility.Visible;
        }

        public static SolidColorBrush FromColorToBrush(Color input)
        {
            return new SolidColorBrush(input);
        }

        public static string SwitchBetweenOverlayIcon(bool input)
        {
            if (input)
            {
                return "\uEE47";
            }
            else
            {
                return "\uEE49";
            }
        }

        /// <summary>
        /// Use to check if input item is null or not
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public static bool IsItemNull(object item)
        {
            return item is null;
        }

        /// <summary>
        /// Use in XAML binding, if item is null hide that UI, if item is not null show the UI
        /// </summary>
        /// <param name="input">anything</param>
        /// <returns></returns>
        public static Visibility ShowIfItemIsNotNull(object input) => IsItemNull(input) ? Visibility.Collapsed : Visibility.Visible;

        public static Visibility CanIShowStatusBar(bool classicMode, bool focus, bool over, bool showStatusBar)
        {
            //Is it classic mode?
            if (classicMode)
            {
                //If it on either mode, is it allow to show status bar?
                if (showStatusBar)
                {
                    return Visibility.Visible;
                }
            }
            else if (!focus && !over && !classicMode)
            {
                //Is not in any mode (focus, overlay, classic)
                if (showStatusBar)
                {
                    return Visibility.Visible;
                }
            }
            return Visibility.Collapsed;
        }

        /// <summary>
        /// Use to compare number and return boolean
        /// </summary>
        /// <param name="input">Number input</param>
        /// <param name="compare">Compare type</param>
        /// <param name="target">Number to compare to</param>
        /// <returns></returns>
        public static bool CompareNumber(int input, string compare, int target)
        {
            switch (compare)
            {
                case IntCompare.NotEqual:
                    return input != target;
                case IntCompare.LessOrEqual:
                    return input <= target;
                case IntCompare.MoreOrEqual:
                    return input >= target;
                case IntCompare.Less:
                    return input < target;
                case IntCompare.More:
                    return input > target;
                case IntCompare.Equal:
                default:
                    return input == target;
            }
        }

        public static Visibility ShowAfterCompareNumber(int number, string compareType, int target)
        {
            return CompareNumber(number, compareType, target) ? Visibility.Visible : Visibility.Collapsed;
        }

        public static Brush SelectionBetweenBrush(bool determiner, Brush a, Brush b) => determiner ? a : b;

        public static FontFamilyModel SelectionFromString(string name, IList<FontFamilyModel> fonts)
        {
            if (fonts is null)
                return new FontFamilyModel(App.Settings.DefaultFont);
            return fonts.FirstOrDefault(i => i.Name == name);
        }

        public static Color GetColorFromHex(string hex)
        {
            if (!hex.StartsWith("#"))
            {
                if (hex == "Default")
                    return new UISettings().GetColorValue(UIColorType.Background);
                else
                    return (Color)XamlBindingHelper.ConvertValue(typeof(Color), hex);
            }
            hex = hex.Replace("#", string.Empty);
            var a = (byte)(Convert.ToUInt32(hex.Substring(0, 2), 16));
            var r = (byte)(Convert.ToUInt32(hex.Substring(2, 2), 16));
            var g = (byte)(Convert.ToUInt32(hex.Substring(4, 2), 16));
            var b = (byte)(Convert.ToUInt32(hex.Substring(6, 2), 16));
            return Windows.UI.Color.FromArgb(a, r, g, b);
        }

        public static string GetHexFromColor(Color color)
        {
            return $"#{color.A:X2}{color.R:X2}{color.G:X2}{color.B:X2}";
        }
    }
}