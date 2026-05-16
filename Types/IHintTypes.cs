using System;
using System.Collections.Generic;
using System.Text;

namespace MongoOptions.Types
{
    /// <exclude />
    public enum PropertyIntent
    {
        Text,
        Password,
        Numeric
    }

    /// <exclude />
    public record PropertyHint(
        PropertyIntent Intent,
        bool IsRequired,
        string? DisplayName,
        object? Min = null,
        object? Max = null
    );
}
