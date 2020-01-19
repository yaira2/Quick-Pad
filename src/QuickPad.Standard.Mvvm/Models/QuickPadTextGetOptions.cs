using System;

namespace QuickPad.Mvvm.Models
{
    [Flags]
    public enum QuickPadTextGetOptions : uint
    {
        /// <summary>None of the following options is used.</summary>
        None = 0U,
        /// <summary>If the starting character position is in the middle of a construct such as a CRLF (U+000D U+000A), surrogate pair, variation-selector sequence, or table-row delimiter, move to the beginning of the construct.</summary>
        AdjustCrlf = 1U,
        /// <summary>Use carriage return/line feed (CR/LF) in place of a carriage return.</summary>
        UseCrlf = 2U,
        /// <summary>Retrieve text including the alternate text for the images in the range.</summary>
        UseObjectText = 4U,
        /// <summary>Allow retrieving the final end-of-paragraph (EOP) if it’s included in the range. This EOP exists in all rich-text controls and cannot be deleted. It does not exist in plain-text controls.</summary>
        AllowFinalEop = 8U,
        /// <summary>Don't include hidden text.</summary>
        NoHidden = 32U,
        /// <summary>Include list numbers.</summary>
        IncludeNumbering = 64U,
        /// <summary>Retrieve Rich Text Format (RTF) instead of plain text. Rich Text Format (RTF) is a BYTE (8-bit) format, but because ITextRange.GetText returns a string, the Rich Text Format (RTF) is returned as WCHARs (16-bit or UTF-16), not bytes, when you call ITextRange.GetText with the **FormatRtf** value. When you call ITextRange.SetText with **FormatRtf**, the method accepts an string containing either bytes or WCHARs, but other Rich Text Format (RTF) readers only understand bytes.</summary>
        FormatRtf = 8192U,
    }
}