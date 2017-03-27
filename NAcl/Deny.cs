using System;
using System.Collections.Generic;
using System.Text;

namespace NAcl
{
    public class Deny : AccessRule
    {
        public Deny(string resource, string verb, string subject)
            : base(resource, verb,subject)
        {

        }

        public Deny()
        {

        }

        public override AccessRules Type
        {
            get { return AccessRules.Deny; }
        }
    }
}
