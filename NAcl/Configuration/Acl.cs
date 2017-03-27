using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration;

namespace NAcl.Configuration
{
    [ConfigurationCollection(typeof(Ace), AddItemName = "allow, deny", CollectionType = ConfigurationElementCollectionType.BasicMap)]
    public class Acl : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new Ace();
        }

        protected override bool IsElementName(string elementname)
        {
            return (elementname == "allow" || elementname == "deny");
        }

        protected override ConfigurationElement CreateNewElement(string elementName)
        {
            switch (elementName)
            {
                case "allow":
                    return new Ace(AccessRules.Allow);
                case "deny":
                    return new Ace(AccessRules.Deny);
            }
            return base.CreateNewElement(elementName);
        }


        public override ConfigurationElementCollectionType CollectionType
        {
            get { return ConfigurationElementCollectionType.BasicMap; }
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((Ace)element).Resource + ((Ace)element).Verb;
        }
    }
}
