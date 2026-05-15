using System;
using System.Collections.Generic;
using System.Text;

namespace MongoOptions.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class OptionsMetadataAttribute() : Attribute
    {
        public Type? MetadataRecord { get; set; }
        public string[] MetadataNames { get; set; } = new string[] { };
    }
}
