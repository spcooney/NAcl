using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration;
using NAcl.Providers;

namespace NAcl.Configuration
{
    public class AclConfigurationSection : ConfigurationSection
    {
        [ConfigurationProperty("defaultProvider")]
        public string DefaultProviderName
        {
            get { return (string)base["defaultProvider"]; }
            set { base["defaultProvider"] = value; }
        }

        public IAclProvider DefaultProvider
        {
            get { return Providers[DefaultProviderName].Provider; }
        }

        [ConfigurationProperty("providers")]
        [ConfigurationCollection(typeof(ProviderCollection))]
        public ProviderCollection Providers
        {
            get { return (ProviderCollection)base["providers"]; }
            set { base["providers"] = value; }
        }

        [ConfigurationProperty("rights")]
        //[ConfigurationCollection(typeof(Ace), AddItemName = "allow, deny", CollectionType = ConfigurationElementCollectionType.BasicMapAlternate)]
        public Acl Rights
        {
            get { return (Acl)base["rights"]; }
            set { base["rights"] = value; }
        }



    }
}
