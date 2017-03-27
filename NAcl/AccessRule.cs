using System;
using System.Collections.Generic;
using System.Text;

namespace NAcl
{
    public enum AccessRules
    {
        Allow,
        Deny
    }

    public abstract class AccessRule : IEquatable<AccessRule>
    {
        public AccessRule(string resource, string verb, string subject)
        {
            Resource = resource;
            Verb = verb;
            Subject = subject;
        }

        public AccessRule()
        {

        }

        public string Resource { get; set; }

        public string Subject { get; set; }

        public string Verb { get; set; }

        public abstract AccessRules Type { get; }

        #region IEquatable<Acl> Members

        public bool Equals(AccessRule other)
        {
            return other.Type == Type && other.Resource == Resource && other.Subject == Subject && other.Verb == Verb;
        }

        #endregion
    }
}
