﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Histria.Model;

namespace Histria.Model.Tests.ModelToTest
{

    public class MInvalidRule : BaseModel
    {
        public virtual string Name { get; set; }
        [RulePropagation("FirstName")]
        protected virtual void InvalidRuleDefinition()
        {
        }
    }
    public class MStateRuleOnValidation : BaseModel
    {
        public virtual string Name { get; set; }
        [State(Rule.Validation, Property = "Name")]
        protected virtual void InvalidStateRuleDefinition()
        {
        }
    }

    

    public class MR1 : BaseModel
    {
        public virtual string Name { get; set; }
        public virtual string RuleResult { get; set; }
        [RulePropagation("Name")]
        protected virtual void Test()
        {
            RuleResult = "MR1.Test";
        }
    }
    public class MR2 : MR1
    {

        [RulePropagation("Name")]
        protected override void Test()
        {
            RuleResult = "MR2.Test";
        }
    }

    public class MSR1 : BaseModel
    {
        public virtual string Name { get; set; }
        public virtual string RuleResult { get; set; }
        [State(Rule.Propagation, Property = "Name")]
        protected virtual void Test()
        {
            RuleResult = "MR1.Test";
        }
    }
    public class MSR2 : MSR1
    {

        [State(Rule.Propagation, Property = "Name")]
        protected override void Test()
        {
            RuleResult = "MR2.Test";
        }
    }

    
    [Display("M3-T", Description="M3-D")]
    public class MR3 : BaseModel
    {
    }

    [Display("@xxx", Description = "@yyy")]
    public class MR4 : BaseModel
    {
        public static string xxx()
        {
            return "MR4.xxx";
        }
        public static string yyy()
        {
            return "MR4.yyy";
        }
    }


    public class ComplexClass : BaseModel
    {
        public virtual String Name { get; set; }
        public virtual DateTime? DateTimeNullable { get; set; }
        public virtual int? IntNullable { get; set; }
        public virtual List<string> MyList { get; set; }
        public virtual Memo Log { get; set; }
  
  
    }

    
}
