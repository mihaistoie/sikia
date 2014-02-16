﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Collections.ObjectModel;


namespace Sikia.Model
{
    using Sikia.Sys;

    ///<summary>
    /// 
    ///</summary>   
    public class ClassInfoItem
    {
        #region private members
        private readonly Dictionary<string, PropertyInfo> propsMap = new Dictionary<string, PropertyInfo>();
        private readonly PropertiesCollection properties = new PropertiesCollection();
        private List<RuleItem> rulesList = new List<RuleItem>();
        private readonly List<MethodItem> methodsList = new List<MethodItem>();
        private readonly Dictionary<string, MethodItem> methods = new Dictionary<string, MethodItem>();
        private readonly KeysCollection key = new KeysCollection();
        private readonly List<IndexInfo> indexes = new List<IndexInfo>();
        // Rules by type
        private readonly Dictionary<Rule, RuleList> rules = new Dictionary<Rule, RuleList>();
        private bool inherianceResolved = false;
        private string title;
        private string description;
        private MethodItem gTitle = null;
        private MethodItem gDescription = null;
        #endregion
        public ClassType ClassType = ClassType.Unknown;

        public Type CurrentType { get; set; }
        public Type TargetType { get; set; }
        public bool UseUuid { get; set; }

        ///<summary>
        /// Is persistent ?
        ///</summary>   		
        public bool IsPersistent { get; set; }

        ///<summary>
        /// The name of the class
        ///</summary>   		
        public string Name { get; set; }
        ///<summary>
        /// The title of the class
        ///</summary>   		
        public string Title { get { return ModelHelper.GetStringValue(title, gTitle); } }
        ///<summary>
        /// The Description of the class
        ///</summary>   		
        public string Description { get { return ModelHelper.GetStringValue(description, gDescription); } }
        ///<summary>
        ///(Persistence) Name of table used to store this class
        ///</summary>   		
        public string DbName { get; set; }

        public bool Static { get; set; }
        ///<summary>
        /// ModelManager
        ///</summary>   		
        #region Properties/Primary Key/Indexes
        public PropertiesCollection Properties { get { return properties; } }
        public KeysCollection Key { get { return key; } }
        public List<IndexInfo> Indexes { get { return indexes; } }
        #endregion

        #region Rules
        public void ExecuteRules(Rule kind, Object target)
        {
        }

        private bool _ruleExists(RuleItem ri, List<RuleItem> rl)
        {
            Type dt = ri.Method.GetBaseDefinition().DeclaringType;
            if (CurrentType.IsSubclassOf(dt))
            {
                foreach (RuleItem rule in rl)
                {
                    if (ri.IsOveriddenOf(rule))
                    {
                        throw new ModelException(String.Format(StrUtils.TT("Rule \"{0}.{1}\": Duplicated rule ({2}.{3}). "), ri.DeclaringType.Name, ri.Name, ri.Kind, ri.Property), Name);
                    }
                }
            }
            return false;
        }

        ///<summary>
        /// Associate a rule to this class
        ///</summary>   
        private void AddRule(RuleItem ri)
        {
            RuleList rl = null;
            if (!rules.ContainsKey(ri.Kind))
            {
                rl = new RuleList();
                rules[ri.Kind] = rl;
            }
            else
            {
                rl = rules[ri.Kind];
            }
            rl.Add(ri);
        }
        #endregion

        #region Methods
        private void CheckMehodsAndSetRules()
        {

            if (Static) return;
            //Attach rules to properties/classes
            foreach (RuleItem ri in rulesList)
            {

                if (!String.IsNullOrEmpty(ri.Property))
                {
                    //checked if property exists  
                    PropinfoItem pi = PropertyByName(ri.Property);
                    if (pi == null)
                        throw new ModelException(String.Format(StrUtils.TT("Rule \"{0}.{1}\": The class \"{2}\" has not the property \"{3}\". "), ri.DeclaringType.Name, ri.Name, ri.TargetType.Name, ri.Property), Name);
                    pi.AddRule(ri);

                }
                else
                {
                    AddRule(ri);
                }
            }
            rulesList.Clear();
            foreach (MethodItem mi in methodsList)
            {
                methods.Add(mi.Method.Name, mi);
            }
            methodsList.Clear();

            gTitle = ExtractMethod(title);
            gDescription = ExtractMethod(description);
        }
        private MethodItem MethodByName(string methodName)
        {
            try
            {
                return methods[methodName];
            }
            catch
            {
                return null;
            }
        }
        #endregion

        #region Loading
        // validate class after load
        public void ValidateAndPrepare(ModelManager model)
        {
            foreach (PropinfoItem pi in properties)
            {
                pi.AfterLoad(model, this);
            }
            if (Static)
            {
                //Move rules to target class
                ClassInfoItem dst = model.ClassByType(TargetType);
                foreach (RuleItem ri in rulesList)
                {
                    ClassInfoItem cdst = (ri.TargetType == TargetType ? dst : model.ClassByType(ri.TargetType));
                    if (cdst != null)
                    {
                        cdst.rulesList.Add(ri);
                    }
                }
                rulesList.Clear();
                //Move methods to target class
                foreach (MethodItem mi in methodsList)
                {
                    ClassInfoItem cdst = (mi.TargetType == TargetType ? dst : model.ClassByType(mi.TargetType));
                    if (cdst != null)
                    {
                        cdst.methodsList.Add(mi);
                    }
                }
                methodsList.Clear();
            }


        }
        public MethodItem ExtractMethod(string value)
        {
            if (string.IsNullOrEmpty(value))
                return null;
            if (value[0] == '@')
            {
                string methodName = value.Substring(1);
                return MethodByName(methodName);
            }
            return null;
        }

        ///<summary>
        /// Prepare memory strutures for faster executing
        ///</summary>
        public void ResolveInheritance(ModelManager model)
        {
            if (Static) return;
            if (inherianceResolved) return;
            if (CurrentType.BaseType != null)
            {
                ClassInfoItem pci = model.ClassByType(CurrentType.BaseType);
                if (pci != null)
                {
                    pci.ResolveInheritance(model);
                    // Add parent rules
                    List<RuleItem> parentRules = new List<RuleItem>();
                    parentRules.AddRange(pci.rulesList);
                    // Add child Rules
                    foreach (RuleItem ri in rulesList)
                    {
                        if (!_ruleExists(ri, parentRules))
                        {
                            parentRules.Add(ri);
                        }
                    }
                    rulesList = parentRules;
                }

            }
            inherianceResolved = true;
        }

        ///<summary>
        /// Prepare memory structures for faster executing
        ///</summary>
        public void Loaded(ModelManager model)
        {
            CheckMehodsAndSetRules();
        }

        private void LoadTitle()
        {
            DisplayAttribute da = CurrentType.GetCustomAttributes(typeof(DisplayAttribute), false).FirstOrDefault() as DisplayAttribute;
            if (da != null)
            {
                title = da.Title;
                description = da.Description;
            }
            if (description == "") description = title;
            if (Static)
            {
                RulesForAttribute rf = CurrentType.GetCustomAttributes(typeof(RulesForAttribute), false).FirstOrDefault() as RulesForAttribute;
                if (rf != null)
                {
                    TargetType = rf.TargetType;
                }
            }

        }

        private void LoadProperties()
        {
            if (Static) return;
            Type associationType = typeof(IAssociation);
            Type roleListType = typeof(IRoleList);
            Type roleRefType = typeof(IRoleRef);
            Type roleChild = typeof(IRoleChild);

            PropertyInfo[] props = CurrentType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (PropertyInfo pi in props)
            {
                PropinfoItem item = new PropinfoItem(pi);
                DefaultAttribute da = pi.GetCustomAttributes(typeof(DefaultAttribute), false).FirstOrDefault() as DefaultAttribute;
                if (da != null)
                {
                    item.IsDisabled = da.Disabled;
                    item.IsHidden = da.Hidden;
                    item.IsMandatory = da.Required;
                    item.DefaultValue = da.Value;
                }
                item.IsPersistent = IsPersistent;

                PersistentAttribute pa = pi.GetCustomAttributes(typeof(PersistentAttribute), false).FirstOrDefault() as PersistentAttribute;
                if (pa != null)
                {
                    item.IsPersistent = pa.IsPersistent;

                    if (!string.IsNullOrEmpty(pa.PersistentName))
                        item.PersistentName = pa.PersistentName;
                }
                //Association
                if (associationType.IsAssignableFrom(pi.PropertyType))
                {
                    //is association
                    AssociationAttribute ra = pi.GetCustomAttributes(typeof(AssociationAttribute), false).FirstOrDefault() as AssociationAttribute;
                    if (ra == null)
                    {
                        throw new ModelException(String.Format(StrUtils.TT("Association attribute is missing.({0}.{1})"), item.Name, Name), Name);
                    }
                    RoleInfoItem role = null;
                    if (roleListType.IsAssignableFrom(pi.PropertyType))
                    {
                        role = new RoleInfoItem() { Min = ra.Min, Max = ra.Max, RoleInvName = ra.Inv, Type = ra.Type,  IsList = true, IsChild = false};
                    }
                    else if (roleRefType.IsAssignableFrom(pi.PropertyType))
                    {
                        role = new RoleInfoItem() { Min = ra.Min, Max = ra.Max, RoleInvName = ra.Inv, Type = ra.Type, Foreingkey = ra.ForeignKey, IsList = false, IsChild = false };
                        if (roleChild.IsAssignableFrom(pi.PropertyType))
                        {
                            if ((ra.Type != Relation.Embedded) || (ra.Type != Relation.Composition) || (ra.Type != Relation.Aggregation))
                            {
                                role.IsChild = true;
                            }
                            else
                            {
                                throw new ModelException(String.Format(StrUtils.TT("Invalid association type.({0}.{1}. Excepted composition or aggregation.)"), item.Name, Name), Name);
                            }

                        }
  
                    }
                    item.Role = role;

                }
                propsMap.Add(item.Name, item.PropInfo);
                properties.Add(item);

            }
        }

        private void LoadMethodsAndRules()
        {
            BindingFlags bindingAttr = (Static ? (BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)
                : (BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static));

            MethodInfo[] methods = CurrentType.GetMethods(bindingAttr);
            foreach (MethodInfo mi in methods)
            {

                if (mi.Name.StartsWith("get_") || mi.Name.StartsWith("set_") || mi.Name.StartsWith("AOP")) continue;

                RuleAttribute[] ras = mi.GetCustomAttributes(typeof(RuleAttribute), false) as RuleAttribute[];
                if ((ras != null) && (ras.Length > 0))
                {
                    foreach (RuleAttribute ra in ras)
                    {
                        if (ra.Rule == Rule.Unknown)
                        {
                            throw new ModelException(String.Format(StrUtils.TT("Invalid rule type for \"{0}\" in the class \"{1}\"."), mi.Name, Name), Name);
                        }
                        if (!ra.CheckProperty())
                        {
                            throw new ModelException(String.Format(StrUtils.TT("Invalid rule property for \"{0}\" in the class \"{1}\"."), mi.Name, Name), Name);
                        }
                        RuleItem ri = new RuleItem(mi);
                        ri.Kind = ra.Rule;
                        ri.Property = ra.Property;
                        if (Static)
                        {
                            if (ri.TargetType == null)
                                ri.TargetType = TargetType;
                            if (ri == null)
                            {
                                throw new ModelException(String.Format(StrUtils.TT("Invalid rule target for \"{0}\" in the class \"{1}\"."), mi.Name, Name), Name);
                            }
                        }
                        else
                        {
                            ri.TargetType = CurrentType;
                        }
                        ri.DeclaringType = CurrentType;
                        rulesList.Add(ri);
                    }
                }
                //Add methods
                if (TargetType != null)
                {
                    MethodItem method = new MethodItem(mi);
                    method.TargetType = TargetType;
                    method.DeclaringType = CurrentType;
                    methodsList.Add(method);
                }
            }

        }
        private void LoadPersistence()
        {
            if (Static) return;
            
            if(typeof(IPersistentObj).IsAssignableFrom(CurrentType))
            {
                IsPersistent = true;
                ClassType = ClassType.Model;
                DbAttribute db = CurrentType.GetCustomAttributes(typeof(DbAttribute), false).FirstOrDefault() as DbAttribute;
                DbName = (String.IsNullOrEmpty(db.TableName) ? Name : db.TableName);
            }


        }
        private void LoadPrimarykey()
        {
            if (Static) return;
            PrimaryKeyAttribute pk = CurrentType.GetCustomAttributes(typeof(PrimaryKeyAttribute), false).FirstOrDefault() as PrimaryKeyAttribute;
            string[] akeys;
            if (pk != null)
            {
                akeys = pk.Keys.Split(',');
                if (akeys.Length == 0)
                {
                    throw new ModelException(String.Format(StrUtils.TT("Missing Primary key for {0}."), Name), Name);
                }
                UseUuid = ((akeys.Length == 0) && akeys[0] == "Uuid");
            }
            else
            {
                UseUuid = true;
                akeys = new string[] { "Uuid" };

            }
            foreach (string akey in akeys)
            {
                string skey = akey.Trim();
                PropinfoItem pi = PropertyByName(skey);
                if (pi == null)
                    throw new ModelException(String.Format(StrUtils.TT("Invalid field {0}  in Primary key for {1}."), skey, Name), Name);
                key.Add(new KeyItem(skey, pi.PropInfo));
            }
        }

        private void LoadIndexes()
        {
            //load indexes 
            object[] attributes = CurrentType.GetCustomAttributes(typeof(IndexAttribute), false);
            foreach (object arttr in attributes)
            {
                IndexAttribute ia = arttr as IndexAttribute;
                IndexInfo ii = new IndexInfo();
                ii.Load(ia.Columns, ia.Name, ia.Unique, this);
                indexes.Add(ii);
            }
        }


        #endregion

        public PropertyInfo PropertyInfoByName(string propName)
        {
            PropertyInfo pi = null;
            propsMap.TryGetValue(propName, out pi);
            return pi;

        }
        public PropinfoItem PropertyByName(string propName)
        {
            try
            {
                return properties[propsMap[propName]];
            }
            catch (Exception)
            {
                return null;
            }
        }

        public ClassInfoItem(Type cType, bool staticClass)
        {
            CurrentType = cType;
            TargetType = (staticClass ? null : CurrentType);

            Name = CurrentType.Name;
            title = CurrentType.Name;
            Static = staticClass;

            LoadTitle();
            LoadProperties();
            LoadMethodsAndRules();
            LoadPrimarykey();
            LoadIndexes();

        }

    }
    public class PropertiesCollection : KeyedCollection<PropertyInfo, PropinfoItem>
    {
        protected override PropertyInfo GetKeyForItem(PropinfoItem item)
        {
            return item.PropInfo;
        }
    }
    public class KeysCollection : KeyedCollection<PropertyInfo, KeyItem>
    {
        protected override PropertyInfo GetKeyForItem(KeyItem item)
        {
            return item.Property;
        }
    }




}

