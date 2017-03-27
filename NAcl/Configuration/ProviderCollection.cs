using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration;

namespace NAcl.Configuration
{
    public class ProviderCollection : ConfigurationElementCollection
    {
        public new ProviderElement this[string providerName]
        {
            get { return (ProviderElement)base.BaseGet(providerName); }
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new ProviderElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((ProviderElement)element).Name;
        }
    }
}
