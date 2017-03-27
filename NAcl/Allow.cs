using System;
using System.Collections.Generic;
using System.Text;

namespace NAcl
{
    public class Allow : AccessRule
    {
        public Allow(string resource, string verb, string subject)
            : base(resource, verb, subject)
        {

        }

        public Allow()
        {

        }

        public override AccessRules Type
        {
            get { return AccessRules.Allow; }
        }
    }
}
