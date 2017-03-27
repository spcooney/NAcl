using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration;
using System.Collections.Specialized;
using System.Reflection;
using NAcl.Providers;

namespace NAcl.Configuration
{
    public class ProviderElement : ConfigurationElement
    {
        [ConfigurationProperty("name", IsKey = true)]
        public string Name
        {
            get { return (string)base["name"]; }
            set { base["name"] = value; }
        }

        [ConfigurationProperty("type")]
        public string ProviderType
        {
            get { return (string)base["type"]; }
            set { base["type"] = value; }
        }

        NameValueCollection parameters = new NameValueCollection();

        protected override bool OnDeserializeUnrecognizedAttribute(string name, string value)
        {
            parameters.Add(name, value);
            return true;
        }

        private IAclProvider provider;

        public IAclProvider Provider
        {
            get
            {
                if (provider == null)
                {
                    Type providerType = Type.GetType(ProviderType);
                    if (providerType == null)
                        throw new ConfigurationErrorsException("The provider of type '" + ProviderType + "' could not be found");
                    ConstructorInfo ctor = providerType.GetConstructor(new Type[] { typeof(AclConfigurationSection), typeof(NameValueCollection) });
                    if (ctor != null)
                        provider = (IAclProvider)ctor.Invoke(new object[] { EvaluationContext.GetSection("nacl"), parameters });
                    else
                    {
                        ctor = providerType.GetConstructor(new Type[] { typeof(NameValueCollection) });
                        if (ctor != null)
                            provider = (IAclProvider)ctor.Invoke(new object[] { parameters });
                        else
                            provider = (IAclProvider)Activator.CreateInstance(providerType);
                    }
                }
                return provider;
            }
            set { provider = value; }
        }
    }
}
