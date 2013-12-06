﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sikia.Framework.Types
{


    public enum RuleType
    {
        Unknown = 0, Validation = 2, Propagation = 4, AfterCreate = 8,
        AfterLoaded = 16, BeforeSave = 32, Correction = 64
    };
    public static class AttributeParser
    {
        static public RuleType ParseRuleType(string value)
        {
            foreach (RuleType enumValue in Enum.GetValues(typeof(RuleType)))
            {
                string svalue = Enum.GetName(typeof(RuleType), enumValue);
                if (String.Compare(svalue, value, true) == 0) return enumValue;
            }
            if (String.Compare("create", value, true) == 0) return RuleType.AfterCreate;
            if (String.Compare("loaded", value, true) == 0) return RuleType.AfterLoaded;
            if (String.Compare("save", value, true) == 0) return RuleType.BeforeSave;
            if (String.Compare("validate", value, true) == 0) return RuleType.Validation;
            if (String.Compare("corection", value, true) == 0) return RuleType.Correction;
            return RuleType.Unknown;
        }
    }

}