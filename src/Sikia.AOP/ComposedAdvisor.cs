﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Sikia.AOP
{
    internal class ComposedAdvisor : AdvisorBase
    {
        public AdvisorBase InnerAdvisor { get; set; }

        protected override void Proceed(IAspectInvocationContext context, IList<Exception> errors)
        {
            if (InnerAdvisor == null)
            {
                base.Proceed(context, errors);
            }
            else
            {
                InnerAdvisor.Execute(context, errors);
            }
        }
    }
}
