using System;
using System.Collections.Generic;
using System.Text;

namespace NAcl.Providers
{
    public class MemoryProvider : IAclProvider
    {
        public OrderedList<string, AccessRule> Allowed { get; set; }
        public OrderedList<string, AccessRule> Denied { get; set; }

        public MemoryProvider()
        {
            Allowed = new OrderedList<string, AccessRule>();
            Denied = new OrderedList<string, AccessRule>();
        }

        #region IAclProvider Members

        public IEnumerable<AccessRule> GetAcls(string resource, string verb)
        {
            resource = GetAbsoluteResourcePath(verb, resource);

            OrderedList<string, AccessRule> acls = new OrderedList<string, AccessRule>();

            while (resource != string.Empty)
            {
                if (Denied.ContainsKey(resource))
                {
                    foreach (AccessRule acl in Denied[resource])
                    {
                        acls.Add(acl.Resource, acl);
                    }
                }
                if (Allowed.ContainsKey(resource))
                {
                    foreach (AccessRule acl in Allowed[resource])
                    {
                        acls.Add(acl.Resource, acl);
                    }
                }
                if (resource == AclManager.ROOT)
                    break;

                resource = resource.LastIndexOf(AclManager.ROOT) <= 0 ? AclManager.ROOT : resource.Substring(0, resource.LastIndexOf(AclManager.ROOT));

            }
            return acls;
        }

        public IEnumerable<AccessRule> GetAclsBySubject(params string[] subjects)
        {
            throw new NotImplementedException();
        }

        public IAclProvider SetAcls(params AccessRule[] acls)
        {
            foreach (AccessRule acl in acls)
            {
                switch (acl.Type)
                {
                    case AccessRules.Allow:
                        Allowed.Add(GetAbsoluteResourcePath(acl), acl);
                        break;
                    case AccessRules.Deny:
                        Denied.Add(GetAbsoluteResourcePath(acl), acl);
                        break;
                }
                if (AclChanged != null)
                    AclChanged(this, acl.Resource);
            }
            return this;
        }

        public IAclProvider DeleteAcls(params AccessRule[] acls)
        {
            foreach (AccessRule acl in acls)
            {
                switch (acl.Type)
                {
                    case AccessRules.Allow:
                        if (Allowed.ContainsKey(GetAbsoluteResourcePath(acl)))
                        {
                            ICollection<AccessRule> allowed = Allowed[GetAbsoluteResourcePath(acl)];
                            allowed.Remove(acl);
                        }
                        break;
                    case AccessRules.Deny:
                        if (Denied.ContainsKey(GetAbsoluteResourcePath(acl)))
                        {
                            ICollection<AccessRule> denied = Denied[GetAbsoluteResourcePath(acl)];
                            denied.Remove(acl);
                        }
                        break;
                }

                if (AclChanged != null)
                    AclChanged(this, acl.Resource);
            }
            return this;
        }

        private string GetAbsoluteResourcePath(AccessRule acl)
        {
            return GetAbsoluteResourcePath(acl.Verb, acl.Resource);
        }

        private string GetAbsoluteResourcePath(string verb, string resource)
        {
            if (verb == "*")
                return string.IsNullOrEmpty(resource) ? AclManager.ROOT : resource;

            if (resource == "/")
                return verb;

            return verb + resource;
        }

        public IAclProvider DeleteAcls(string resource, params string[] subjects)
        {
            return this;
        }

        #endregion

        #region IAclProvider Members

        public event AclChangedHandler AclChanged;

        #endregion
    }

}
