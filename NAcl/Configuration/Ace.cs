using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration;

namespace NAcl.Configuration
{
    public class Ace : ConfigurationElement
    {
        public Ace()
        {

        }

        public Ace(AccessRules type)
        {
            Type = type;
        }

        public AccessRules Type { get; set; }

        [ConfigurationProperty("resource")]
        public string Resource
        {
            get { return (string)base["resource"]; }
            set { base["resource"] = value; }
        }

        [ConfigurationProperty("subject")]
        public string Subject
        {
            get { return (string)base["subject"]; }
            set { base["subject"] = value; }
        }

        [ConfigurationProperty("verb")]
        public string Verb
        {
            get { return (string)base["verb"]; }
            set { base["verb"] = value; }
        }

        [ConfigurationProperty("targetProvider")]
        public string TargetProvider
        {
            get { return (string)base["targetProvider"]; }
            set { base["targetProvider"] = value; }
        }
    }
}
