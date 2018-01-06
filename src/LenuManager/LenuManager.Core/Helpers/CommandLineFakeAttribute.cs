using System;
using System.Collections.Generic;
using System.Text;

namespace CommandLine
{
    [AttributeUsage(AttributeTargets.Property)]
    class OptionAttribute : Attribute
    {
        public OptionAttribute()
        {
        }
        public OptionAttribute(string longName)
        {
        }
        public OptionAttribute(char shortName, string longName)
        {
        }
        public OptionAttribute(char shortName)
        {
        }
        public string HelpText { get; set; }
        public bool Required { get; set; }
        public object DefaultValue { get; set; }
    }
}
