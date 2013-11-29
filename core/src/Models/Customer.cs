﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using Sikia.Aop;
using Sikia.Framework;
using Sikia.Framework.Attributes;


namespace Sikia.Models
{

    [Display("Gender", Description = "Gender of Customer")]
    public enum Gender 
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
        [Display("First Name", Description = "First Name of Customer")]
        public  string FirstName { get; set; }
        
        [Display("Last Name", Description = "Last Name of Customer")]
        public  string LastName { get; set; }
        
        [Display("Full Name", Description = "Full Name of Customer")]
        public string FullName { get; set; }
        
        [Rule("Propagation", Property = "FirstName")]
        [Rule("Propagation", Property = "LastName")]
        [Display("Calculate Full Name", Description = "Calculate Full Name")]
        public void CalculatePersistentFullName() { FullName = FirstName + " " + LastName.ToUpper(); }
    }
    [Display("Address", Description = "Class Address")]
    [Index("Street desc, Complement", Unique = true)]
    public class Address : InterceptedObject
    {
        [Display("Street", Description = "Street Address")]
        public virtual string Street { get; set; }
        [Display("Complement", Description = "Complement Address")]
        public virtual string Complement { get; set; }
    }

}
