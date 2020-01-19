using System;

namespace QuickPad.Mvvm.Models
{
    [Flags]
    public enum QuickPadTextSetOptions : uint
    {
        /// <summary>No text setting option is specified.</summary>
        None = 0U,
        /// <summary>Use the Unicode bidirectional algorithm.</summary>
        UnicodeBidi = 1U,
        /// <summary>Don't include text as part of a hyperlink.</summary>
        Unlink = 8U,
        /// <summary>Don't insert as hidden text.</summary>
        Unhide = 16U,
        /// <summary>Obey the current text limit instead of increasing the limit to fit.</summary>
        CheckTextLimit = 32U,
        /// <summary>Treat input text as Rich Text Format (RTF) (the Rich Text Format (RTF) text will be validated).</summary>
        FormatRtf = 8192U,
        /// <summary>Apply the Rich Text Format (RTF) default settings for the document. Rich Text Format (RTF) often contains document default properties. These properties are typically ignored when inserting Rich Text Format (RTF) (as distinguished from opening an Rich Text Format (RTF) file).</summary>
        ApplyRtfDocumentDefaults = 16384U,
    }
}