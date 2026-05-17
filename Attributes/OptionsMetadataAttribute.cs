using System;
using System.Collections.Generic;
using System.Text;

namespace MongoOptions.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class OptionsMetadataAttribute(Type MetadataRecord) : Attribute
    {
        public Type? MetadataRecord { get; set; } = MetadataRecord;
        // public string[] MetadataNames { get; set; } = new string[] { };
    }
}
