﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Histria.Model;

namespace Histria.Core.Tests.Rules.Customers
{

    [Display("Gender", Description = "Gender of Customer")]
    public enum CustomerGender
    {
        [Display("Masculin")]
        Male,
        [Display("Féminin")]
        Female
    };


    [Db("FirstName, LastName")]
    [Display("Customer", Description = "Class Customer")]
    public class Customer : InterceptedObject
    {
        public int RCount = 0;
        public int ARCount = 0;
        public int MethodTest = 0;
        [Display("First Name", Description = "First Name of Customer")]
        public virtual string FirstName { get; set; }

        [Display("Last Name", Description = "Last Name of Customer")]
        public virtual string LastName { get; set; }

        [Display("Full Name", Description = "Full Name of Customer")]
        public virtual string FullName { get; set; }


        [Display("Age", Description = "Age")]
        public virtual int Age { get; set; }

        [Display("Gender", Description = "Gender")]
        public virtual CustomerGender Gender { get; set; }
         
        public virtual string AfterFirstNameChanged { get; set; }



        [Rule(Rule.Propagation, Property = "LastName, FirstName")]
        [Display("Calculate Full Name", Description = "Calculate Full Name")]
        protected virtual void CalculatePersistentFullName()
        {
            RCount++;
            FullName = FirstName + " " + (String.IsNullOrEmpty(LastName) ? "" : LastName).ToUpper();
        }

        [RulePropagation("Age")]
        protected virtual void CountAgeChanges()
        {
            ARCount++;
        }

        [Method]
        [Display("Customer Method 1 ")]
        public void MethodInBaseClass1()
        {

        }
        [Method]
        [Display("Customer Method 2")]
        public virtual void MethodInBaseClass2()
        {
            this.MethodTest = 100;
        }
  
    }

    [RulesFor(typeof(Customer))]
    public class PlugInCustomer : IPluginModel
    {
        [RulePropagation("FirstName")]
        public static void Test(Customer target)
        {
            target.AfterFirstNameChanged = "AfterFirstNameChanged";
        }
    }

    [Display("Russian Customer", Description = "Russian Class Customer")]
    public class RussianCustomer : Customer
    {
        [Display("Middle Name", Description = "Middle Name of Customer")]
        public virtual string MiddleName { get; set; }
        [RulePropagation("MiddleName")]
        protected override void CalculatePersistentFullName()
        {
            FullName = (String.IsNullOrEmpty(LastName) ? "" : LastName).ToUpper() + " " + MiddleName + " " + FirstName;
        }
        [Method]
        [Display("Russian Method")]
        public void MethodInDerivedClass()
        {

        }
        public override void MethodInBaseClass2()
        {
            this.MethodTest = 200;
        }
    }



}
