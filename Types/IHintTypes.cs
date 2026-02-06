using System;
using System.Collections.Generic;
using System.Text;

namespace MongoOptions.Types
{
    public enum PropertyIntent
    {
        Text,
        Password,
        Numeric
    }

    public record PropertyHint(
        PropertyIntent Intent,
        bool IsRequired,
        string? DisplayName,
        object? Min = null,
        object? Max = null
    );
}
