using System;
using System.Collections.Generic;
using System.Text;
using NAcl.Providers;

namespace NAcl.Providers
{
    public class HubSecurityProvider : IAclProvider
    {
        public HubSecurityProvider()
        {
            Providers = new List<IAclProvider>();
        }

        public void Add(IAclProvider provider)
        {
            ((List<IAclProvider>)Providers).Add(provider);
            provider.AclChanged += provider_AclChanged;
        }

        public void Remove(IAclProvider provider)
        {
            ((List<IAclProvider>)Providers).Remove(provider);
            provider.AclChanged -= provider_AclChanged;
        }

        void provider_AclChanged(IAclProvider sender, string resource)
        {
            if (AclChanged != null)
                AclChanged(this, resource);
        }

        public IEnumerable<IAclProvider> Providers { get; private set; }

        #region IAclProvider Members

        public IEnumerable<AccessRule> GetAcls(string resource, string verb)
        {
            List<AccessRule> acls = new List<AccessRule>();
            foreach (IAclProvider provider in Providers)
            {
                acls.AddRange(provider.GetAcls(resource, verb));
            }
            return acls;
        }

        public IEnumerable<AccessRule> GetAclsBySubject(params string[] subjects)
        {
            List<AccessRule> acls = new List<AccessRule>();
            foreach (IAclProvider provider in Providers)
            {
                acls.AddRange(provider.GetAclsBySubject(subjects));
            }
            return acls;
        }

        public IAclProvider SetAcls(params AccessRule[] acls)
        {
            foreach (IAclProvider provider in Providers)
            {
                provider.SetAcls(acls);
            }
            return this;
        }

        public IAclProvider DeleteAcls(params AccessRule[] acls)
        {
            foreach (IAclProvider provider in Providers)
            {
                provider.DeleteAcls(acls);
            }
            return this;
        }

        public IAclProvider DeleteAcls(string resource, params string[] subjects)
        {
            foreach (IAclProvider provider in Providers)
            {
                provider.DeleteAcls(resource, subjects);
            }
            return this;
        }

        #endregion

        #region IAclProvider Members


        public event AclChangedHandler AclChanged;

        #endregion
    }
}
