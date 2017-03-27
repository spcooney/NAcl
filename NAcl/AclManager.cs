using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration;
#if !SILVERLIGHT
using NAcl.Configuration;
#endif
using NAcl.Providers;

namespace NAcl
{
    public delegate void AclChangedHandler(IAclProvider sender, string resource);

    public static class AclManager
    {
        static AclManager()
        {
            Providers = new Dictionary<string, IAclProvider>();
#if !SILVERLIGHT
            AclConfigurationSection configSection = (AclConfigurationSection)ConfigurationManager.GetSection("nacl");
            if (configSection != null)
            {
                foreach (ProviderElement provider in configSection.Providers)
                {
                    IAclProvider securityProvider = provider.Provider;
                    Providers.Add(provider.Name, securityProvider);
                    if (provider.Name == configSection.DefaultProviderName)
                        DefaultProvider = securityProvider;
                }
                foreach (Ace ace in configSection.Rights)
                {
                    AccessRule privilege = null;
                    switch (ace.Type)
                    {
                        case AccessRules.Allow:
                            privilege = new Allow(ace.Resource, ace.Verb, ace.Subject);
                            break;
                        case AccessRules.Deny:
                            privilege = new Deny(ace.Resource, ace.Verb, ace.Subject);
                            break;
                    }

                    if (!string.IsNullOrEmpty(ace.TargetProvider))
                        Providers[ace.TargetProvider].SetAcls(privilege);
                    else
                        DefaultProvider.SetAcls(privilege);
                }
            }
            else
#endif
            DefaultProvider = new MemoryProvider();
        }

        public static IAclProvider DefaultProvider { get; set; }

        public static IDictionary<string, IAclProvider> Providers { get; set; }

        public static IAclProvider Allow(string resource, string verb, params string[] subjects)
        {
            List<AccessRule> acls = new List<AccessRule>();
            resource = resource.ToLower();
            verb = verb.ToLower();
            foreach (string subject in subjects)
            {
                acls.Add(new Allow(resource, verb, subject.ToLower()));
            }
            return DefaultProvider.SetAcls(acls.ToArray());
        }

        public static IAclProvider Deny(string resource, string verb, params string[] subjects)
        {
            List<AccessRule> acls = new List<AccessRule>();
            resource = resource.ToLower();
            verb = verb.ToLower();
            foreach (string subject in subjects)
            {
                acls.Add(new Deny(resource, verb, subject.ToLower()));
            }
            return DefaultProvider.SetAcls(acls.ToArray());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="resource"></param>
        /// <param name="subjects"></param>
        /// <returns><see cref="true"/> if the <paramref name="subjects"/> specified have at least one rule that allow them to do something within the resource hierarchy</returns>
        public static bool CanBrowse(string resource, params string[] subjects)
        {
            resource = resource.ToLower();
            foreach (AccessRule acl in DefaultProvider.GetAcls(resource, "*"))
                if (acl.Type == AccessRules.Allow)
                    return true;

            return false;
        }

        public static bool IsAllowed(string resource, string verb, params string[] subjects)
        {
            OrderedList<string, AccessRule> acls = new OrderedList<string, AccessRule>(new ReverseComparer<string>());
            //OrderedList<string, Acl> denied = new OrderedList<string, Acl>(new ReverseComparer<string>());
            List<string> subjectList = new List<string>();
            foreach (string subject in subjects)
                subjectList.Add(subject.ToLower());
            resource = resource.ToLower();
            verb = verb.ToLower();
            foreach (AccessRule acl in DefaultProvider.GetAcls(resource, verb))
                acls.Add(acl.Resource, acl);

            bool isExplicit = false;
            AccessRules aclType = AccessRules.Deny;
            bool set = false;
            string mostAppropriateResourcePath = resource;

            bool isFirst = true;

            foreach (AccessRule acl in acls)
            {
                if (isFirst)
                {
                    mostAppropriateResourcePath = acl.Resource;
                    isFirst = false;
                }

                if (set && mostAppropriateResourcePath != acl.Resource)
                    return aclType == AccessRules.Allow;

                if (acl.Subject == "*")
                {
                    set = true;
                    aclType = acl.Type;
                }
                if (subjectList.Contains(acl.Subject))
                {
                    set = true;
                    isExplicit = true;
                    aclType = acl.Type;
                }

                if (isExplicit)
                    return aclType == AccessRules.Allow;
            }

            return aclType == AccessRules.Allow;



            // Search for explicit rule or inherit for parent at each level
            // If two explicit rules are found, Deny has the priority
            //bool isDenied = false;
            //while (resource != string.Empty)
            //{
            //    //foreach (string verb in verbs)
            //    //{
            //    if (denied.ContainsKey(resource))
            //    {
            //        foreach (Acl acl in denied[resource])
            //        {
            //            if (verbList.Contains(acl.verb))
            //                return false;
            //        }

            //        // if global rule, allow only if there is a specific user's rule for the current path
            //        if (denied[resource].Contains(new Deny(resource, "*")))
            //            isDenied = allowed.ContainsKey(resource) && allowed[resource].Contains(verb);
            //    }


            //    if (allowed.ContainsKey(resource) && (allowed[resource].Contains(verb) || (!isDenied && allowed[resource].Contains("*"))))
            //        return true;
            //}

            //if (isDenied)
            //    return false;

            //if (resource == ROOT)
            //    return false;

            //resource = resource.LastIndexOf(ROOT) <= 0 ? ROOT : resource.Substring(0, resource.LastIndexOf(ROOT));


            //return false;
        }

        internal const string ROOT = "/";

        static OrderedList<string, WeakReference> aclChangedHandlers = new OrderedList<string, WeakReference>();

        public static event Action<string> AclChanged
        {
            add { RegisterForRuleChange("*", value); }
            remove { UnregisterForRuleChange("*", value); }
        }

        private static void UnregisterForRuleChange(string resource, Action<string> handler)
        {
            aclChangedHandlers.Remove(resource, new WeakReference(handler));
            if (aclChangedHandlers.Count == 0)
                DefaultProvider.AclChanged -= DefaultProvider_AclChanged;
        }

        public static void RegisterForRuleChange(string resource, Action<string> handler)
        {
            if (aclChangedHandlers.Count == 0)
                DefaultProvider.AclChanged += DefaultProvider_AclChanged;
            aclChangedHandlers.Add(resource, new WeakReference(handler));
        }

        static void DefaultProvider_AclChanged(IAclProvider sender, string resource)
        {
            string currentResource = resource;
            int lastIndexOfSlash = resource.Length;
            ICollection<WeakReference> handlers;
            do
            {
                currentResource = currentResource.Substring(0, lastIndexOfSlash);
                if (aclChangedHandlers.TryGetValue(currentResource, out handlers))
                {
                    foreach (WeakReference weakHandler in handlers)
                    {
                        if (weakHandler.IsAlive)
                            ((Action<string>)weakHandler.Target)(resource);
                    }
                }
                if (lastIndexOfSlash == 0)
                    break;
                lastIndexOfSlash = currentResource.LastIndexOf('/');
            }
            while (true);

            if (aclChangedHandlers.TryGetValue("*", out handlers))
            {
                foreach (WeakReference weakHandler in handlers)
                {
                    if (weakHandler.IsAlive)
                        ((Action<string>)weakHandler.Target)(resource);
                }
            }
        }


    }
}
