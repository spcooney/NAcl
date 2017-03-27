using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
#if !SILVERLIGHT
using System.Configuration.Provider;
using NAcl.Configuration;
#endif
using System.Collections.Specialized;

namespace NAcl.Providers
{
    public class RouterProvider : IAclProvider
    {
        public RouterProvider()
        {
            Routes = new SortedDictionary<string, OrderedList<int, IAclProvider>>();
        }

#if !SILVERLIGHT
        public RouterProvider(NameValueCollection parameters)
            : this()
        {

        }


        public RouterProvider(Configuration.AclConfigurationSection configSection, NameValueCollection parameters)
            : this()
        {
            string providerNames = parameters["providers"];
            if (providerNames != null)
            {
                foreach (string providerName in providerNames.Split(','))
                {
                    string realProviderName = providerName;

                    ProviderElement provider = null;
                    while (provider == null)
                    {
                        provider = configSection.Providers[realProviderName];
                        if (provider == null)
                        {
                            realProviderName = providerName.Substring(0, providerName.LastIndexOf('/'));
                            if (realProviderName == AclManager.ROOT)
                                throw new NotSupportedException(string.Format("The provider with name '{0}' could not be found. Make sure it is registered in your configuration file", providerName));
                        }
                    }
                    if (realProviderName == providerName && !providerName.Contains("/"))
                        Register("/", provider.Provider);
                    else
                        Register(providerName.Substring(realProviderName.Length), provider.Provider);

                }
            }
        }

#endif


        protected IDictionary<string, OrderedList<int, IAclProvider>> Routes { get; set; }


        public RouterProvider Register(string resource, IAclProvider provider, int priority)
        {
            resource = resource.ToLower();
            if (!Routes.ContainsKey(resource))
                Routes[resource] = new OrderedList<int, IAclProvider>();
            Routes[resource].Add(priority, provider);
            provider.AclChanged += provider_AclChanged;
            return this;
        }

        void provider_AclChanged(IAclProvider sender, string obj)
        {
            if (AclChanged != null)
            {
                foreach (var providerResource in Routes)
                {
                    string resource = providerResource.Key;
                    foreach (IAclProvider provider in providerResource.Value)
                    {
                        if (provider == sender)
                        {
                            AclChanged(this, resource + obj);
                        }
                    }
                }
            }
        }

        public RouterProvider Register(string resource, IAclProvider provider)
        {
            return Register(resource, provider, 3);
        }

        private IEnumerable<KeyValuePair<string, IAclProvider>> GetConcernedProviders(string resource)
        {
            //Ordered list for priority. Values are the provider and the resource it was registered for
            OrderedList<int, KeyValuePair<string, IAclProvider>> concernedProviders = new OrderedList<int, KeyValuePair<string, IAclProvider>>();
            resource = resource.ToLower();
            while (resource != string.Empty)
            {
                if (Routes.ContainsKey(resource))
                {
                    foreach (KeyValuePair<int, ICollection<IAclProvider>> providers in ((IEnumerable<KeyValuePair<int, ICollection<IAclProvider>>>)Routes[resource]))
                    {
                        foreach (IAclProvider provider in providers.Value)
                            concernedProviders.Add(providers.Key, new KeyValuePair<string, IAclProvider>(resource, provider));
                    }
                }
                int lastIndexOf = resource.LastIndexOf('/');
                if (lastIndexOf > 0)
                    resource = resource.Substring(0, lastIndexOf);
                else if (lastIndexOf == 0 && resource.Length > 1)
                    resource = AclManager.ROOT;
                else
                    break;

            }
            return concernedProviders;
        }

        #region IAclProvider Members

        public IEnumerable<AccessRule> GetAcls(string resource, string verb)
        {
            OrderedList<string, AccessRule> acls = new OrderedList<string, AccessRule>();
            foreach (KeyValuePair<string, IAclProvider> provider in GetConcernedProviders(resource))
            {
                foreach (AccessRule acl in provider.Value.GetAcls(provider.Key == AclManager.ROOT ? resource : resource.Substring(provider.Key.Length), verb))
                {
                    AccessRule computedAcl = null;
                    switch (acl.Type)
                    {
                        case AccessRules.Allow:
                            computedAcl = new Allow(acl.Resource == AclManager.ROOT ? provider.Key : provider.Key + acl.Resource, acl.Verb, acl.Subject);
                            break;
                        case AccessRules.Deny:
                            computedAcl = new Deny(acl.Resource == AclManager.ROOT ? provider.Key : provider.Key + acl.Resource, acl.Verb, acl.Subject);
                            break;
                    }
                    if (computedAcl != null)
                        acls.Add(computedAcl.Resource, computedAcl);
                }
            }
            return acls;
        }

        public IEnumerable<AccessRule> GetAclsBySubject(params string[] subjects)
        {
            throw new NotImplementedException();
        }

        public IAclProvider SetAcls(params AccessRule[] acls)
        {
            if (acls == null)
                return this;

            foreach (AccessRule acl in acls)
            {
                foreach (var provider in GetConcernedProviders(acl.Resource))
                {
                    switch (acl.Type)
                    {
                        case AccessRules.Allow:
                            provider.Value.SetAcls(new Allow(provider.Key == AclManager.ROOT ? acl.Resource : acl.Resource.Substring(provider.Key.Length), acl.Verb, acl.Subject));
                            break;
                        case AccessRules.Deny:
                            provider.Value.SetAcls(new Deny(provider.Key == AclManager.ROOT ? acl.Resource : acl.Resource.Substring(provider.Key.Length), acl.Verb, acl.Subject));
                            break;
                    }
                }
            }

            return this;
        }

        public IAclProvider DeleteAcls(params AccessRule[] acls)
        {
            if (acls == null)
                return this;

            foreach (AccessRule acl in acls)
            {
                foreach (var provider in GetConcernedProviders(acl.Resource))
                {
                    switch (acl.Type)
                    {
                        case AccessRules.Allow:
                            provider.Value.DeleteAcls(new Allow(acl.Resource.Substring(provider.Key.Length), acl.Verb, acl.Subject));
                            break;
                        case AccessRules.Deny:
                            provider.Value.DeleteAcls(new Deny(acl.Resource.Substring(provider.Key.Length), acl.Verb, acl.Subject));
                            break;
                    }
                }
            }

            return this;
        }

        public IAclProvider DeleteAcls(string resource, params string[] subjects)
        {
            foreach (var provider in GetConcernedProviders(resource))
            {
                provider.Value.DeleteAcls(resource.Substring(provider.Key.Length), subjects);
            }

            return this;
        }

        #endregion

        #region IAclProvider Members


        public event AclChangedHandler AclChanged;

        #endregion
    }
}
