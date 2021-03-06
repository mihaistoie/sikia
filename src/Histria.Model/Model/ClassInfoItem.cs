﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;


namespace Histria.Model
{
    using Histria.Sys;

    ///<summary>
    /// Class structure  
    ///</summary>   
    public class ClassInfoItem
    {
        public static readonly string ProperiesStatePropertyName = "Properties";

        #region Private members
        private readonly Dictionary<string, PropertyInfo> _propsMap = new Dictionary<string, PropertyInfo>();
        private readonly PropertiesCollection _properties = new PropertiesCollection();
        private readonly PropertiesCollection _roles = new PropertiesCollection();
        private readonly PropertiesCollection _complexes = new PropertiesCollection();
        private List<RuleItem> _rulesList = new List<RuleItem>();
        private List<RuleItem> _stateRulesList = new List<RuleItem>();
        private readonly List<MethodItem> _methodsList = new List<MethodItem>();
        private readonly Dictionary<string, MethodItem> _methods = new Dictionary<string, MethodItem>();
        private readonly PrimaryKeyItem _key = new PrimaryKeyItem();
        private readonly List<IndexInfo> _indexes = new List<IndexInfo>();
        // Rules by type
        private readonly Dictionary<Rule, RuleList> rules = new Dictionary<Rule, RuleList>();
        // State rules by type
        private readonly Dictionary<Rule, RuleList> stateRules = new Dictionary<Rule, RuleList>();
        private bool _inherianceResolved = false;
        private string _title;
        private string _description;
        private MethodItem _gTitle = null;
        private MethodItem _gDescription = null;
        private ClassInfoItem _super = null;
        private PropInfoItem _cidProp = null;
        private List<ClassInfoItem> _descendants = new List<ClassInfoItem>();

        #endregion

        #region Properties/Primary Key/Indexes
        public ClassType ClassType = ClassType.Unknown;
        public Type CurrentType { get; set; }
        public Type TargetType { get; set; }
        public Type StateClassType { get; private set; }

        public List<ClassInfoItem> Descendants { get { return _descendants; } }

        ///<summary>
        /// Property  "Parent"
        /// If this class is the child-side in an composition relationship, Parent allow to access access the parent-side of relation.
        ///</summary>   	
        public PropInfoItem Parent { get; internal set; }


        ///<summary>
        /// Use Uuid as primary key 
        ///</summary>   	
        public bool UseUuidAsPk { get; internal set; }

        ///<summary>
        /// Is persistent ?
        ///</summary>   		
        public bool IsPersistent { get; internal set; }

        ///<summary>
        /// The name of the class
        ///</summary>   		
        public string Name { get; internal set; }

        ///<summary>
        /// The title of the class
        ///</summary>   		
        public string Title { get { return ModelHelper.GetStringValue(_title, _gTitle); } }

        ///<summary>
        /// The Description of the class
        ///</summary>   		
        public string Description { get { return ModelHelper.GetStringValue(_description, _gDescription); } }

        ///<summary>
        ///(Persistence) Name of table used to store this class
        ///</summary>   		
        public string DbName { get; internal set; }

        ///<summary>
        /// Is a static class ?
        ///</summary>   		
        public bool Static { get; internal set; }

        ///<summary>
        /// List of properties
        ///</summary>   	
        ///
        public PropertiesCollection Properties { get { return _properties; } }

        ///<summary>
        /// List of roles (relationships)
        ///</summary>   	
        public PropertiesCollection Roles { get { return _roles; } }


        ///<summary>
        /// List of complex properties 
        ///</summary>   	
        public PropertiesCollection Complexes { get { return _complexes; } }

  
        ///<summary>
        /// Primary key
        ///</summary>   		
        public PrimaryKeyItem Key { get { return _key; } }

        ///<summary>
        ///List of indexes
        ///</summary>   		
        public List<IndexInfo> Indexes { get { return _indexes; } }

        ///<summary>
        /// Is View ?
        ///</summary>   	
        internal virtual bool IsView { get { return false; } }

        #endregion

        #region Rules

        ///<summary>
        /// Execute rules by type
        ///</summary>  
        public void ExecuteRules(Rule kind, Object target)
        {
            RuleHelper.ExecuteRules(rules, kind, target, RoleOperation.None, null, null);
        }

        ///<summary>
        /// Execute rules by type
        ///</summary>  
        public void ExecuteStateRules(Rule kind, Object target)
        {
            RuleHelper.ExecuteRules(stateRules, kind, target, RoleOperation.None, null, null);
        }


        private bool _ruleExists(RuleItem ri, List<RuleItem> rl)
        {
            Type dt = ri.Method.GetBaseDefinition().DeclaringType;
            if (this.CurrentType.IsSubclassOf(dt))
            {
                foreach (RuleItem rule in rl)
                {
                    if (ri.IsOveriddenOf(rule))
                    {
                        throw new ModelException(String.Format(L.T("Rule \"{0}.{1}\": Duplicated rule ({2}.{3}). "), ri.DeclaringType.Name, ri.Name, ri.Kind, ri.Property), this.Name);
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
            RuleHelper.AddRule(this.rules, ri);

        }

        ///<summary>
        /// Associate a state rule to this class
        ///</summary>   
        private void AddStateRule(RuleItem ri)
        {
            RuleHelper.AddRule(stateRules, ri);

        }
        #endregion

        #region Methods
        private void CheckMehodsAndSetRules()
        {

            if (this.Static) return;
            //Attach rules to properties/classes
            for (int index = 0, len = this._rulesList.Count; index < len; index++)
            {
                RuleItem ri = this._rulesList[index];
                if (!String.IsNullOrEmpty(ri.Property))
                {
                    //checked if property exists  
                    PropInfoItem pi = PropertyByName(ri.Property);
                    if (pi == null)
                        throw new ModelException(String.Format(L.T("Rule \"{0}.{1}\": The class \"{2}\" has not the property \"{3}\". "), ri.DeclaringType.Name, ri.Name, ri.TargetType.Name, ri.Property), Name);
                    pi.AddRule(ri);

                }
                else
                {
                    this.AddRule(ri);
                }
            }
            this._rulesList.Clear();
            //Attach state rules to properties/classes
            for (int index = 0, len = this._stateRulesList.Count; index < len; index++)
            {
                RuleItem ri = this._stateRulesList[index];
                if (!String.IsNullOrEmpty(ri.Property))
                {
                    //checked if property exists  
                    PropInfoItem pi = PropertyByName(ri.Property);
                    if (pi == null)
                        throw new ModelException(String.Format(L.T("Rule \"{0}.{1}\": The class \"{2}\" has not the property \"{3}\". "), ri.DeclaringType.Name, ri.Name, ri.TargetType.Name, ri.Property), this.Name);
                    pi.AddStateRule(ri);

                }
                else
                {
                    this.AddStateRule(ri);
                }
            }
            this._stateRulesList.Clear();

            foreach (MethodItem mi in this._methodsList)
            {
                this._methods.Add(mi.Method.Name, mi);
            }
            this._methodsList.Clear();

            this._gTitle = this.ExtractMethod(_title);
            this._gDescription = this.ExtractMethod(_description);
        }
        public MethodItem MethodByName(string methodName)
        {
            MethodItem mi = null;
            this._methods.TryGetValue(methodName, out mi);
            return mi;
        }
        #endregion

        #region DB helpers

        public ClassInfoItem GetTopPersistentAscendent()
        {
            if (!this.IsPersistent) return null;
            ClassInfoItem ci = this;
            while (ci._super != null && ci._super.IsPersistent)
                ci = ci._super;
            return ci;
        }

        #endregion

        #region Loading
        protected virtual void InitializeView(ModelManager model)
        {
        }
        // validate class after load
        internal void ValidateAndPrepare(ModelManager model)
        {
            this.InitializeView(model);
            foreach (PropInfoItem pi in this._properties)
            {
                pi.AfterLoad(model, this);
            }
            if (this.Static)
            {
                //Move rules to target class
                ClassInfoItem dst = model.ClassByType(this.TargetType);
                for (int index = 0, len = this._rulesList.Count; index < len; index++)
                {
                    RuleItem ri = this._rulesList[index];
                    ClassInfoItem cdst = (ri.TargetType == this.TargetType ? dst : model.ClassByType(ri.TargetType));
                    if (cdst != null)
                    {
                        cdst._rulesList.Add(ri);
                    }
                }
                this._rulesList.Clear();
                for (int index = 0, len = this._stateRulesList.Count; index < len; index++)
                {
                    RuleItem ri = this._stateRulesList[index];
                    ClassInfoItem cdst = (ri.TargetType == this.TargetType ? dst : model.ClassByType(ri.TargetType));
                    if (cdst != null)
                    {
                        cdst._rulesList.Add(ri);
                    }
                }
                this._stateRulesList.Clear();


                //Move methods to target class
                foreach (MethodItem mi in this._methodsList)
                {
                    ClassInfoItem cdst = (mi.TargetType == this.TargetType ? dst : model.ClassByType(mi.TargetType));
                    if (cdst != null)
                    {
                        cdst._methodsList.Add(mi);
                    }
                }
                this._methodsList.Clear();
            }


        }
        internal MethodItem ExtractMethod(string value)
        {
            if (string.IsNullOrEmpty(value))
                return null;
            if (value[0] == '@')
            {
                string methodName = value.Substring(1);
                return this.MethodByName(methodName);
            }
            return null;
        }

        /// <summary>
        /// Inheritance: copy parent rules in childs
        /// </summary>
        /// <param name="pci"></param>
        private void AddRulesFromParent(ClassInfoItem pci)
        {
            // Add parent rules
            List<RuleItem> parentRules = new List<RuleItem>();
            parentRules.AddRange(pci._rulesList);
            // Add child Rules
            foreach (RuleItem ri in _rulesList)
            {
                if (!this._ruleExists(ri, parentRules))
                {
                    parentRules.Add(ri);
                }
            }
            this._rulesList = parentRules;


            // Add parent state rules
            List<RuleItem> parentStateRules = new List<RuleItem>();
            parentStateRules.AddRange(pci._stateRulesList);
            // Add child State Rules
            foreach (RuleItem ri in this._stateRulesList)
            {
                if (!this._ruleExists(ri, parentStateRules))
                {
                    parentStateRules.Add(ri);
                }
            }
            this._stateRulesList = parentStateRules;
        }

        /// <summary>
        /// Inheritance: copy table name in childs
        /// </summary>
        /// <param name="pci"></param>
        private void ResolveInheritancePersistence(ClassInfoItem pci)
        {
            InheritanceAttribute ia = this.CurrentType.GetCustomAttributes(typeof(InheritanceAttribute), false).FirstOrDefault() as InheritanceAttribute;
            // Add parent rules
            if (pci.IsPersistent)
            {
                if (this.DbName != pci.DbName)
                {
                    if (this.DbName != Name)
                    {
                        throw new ModelException(String.Format(L.T("Invalid DB attribute for class \"{0}\"."), this.Name), this.Name);
                    }
                    this.DbName = pci.DbName;
                }
                if (ia == null)
                {
                    throw new ModelException(String.Format(L.T("Missing inheritance attribute for  the class \"{0}\"."), this.Name), this.Name);
                }

            }
            if (ia != null)
            {
                this.InheritanceProperty(ia.EnumProperty, ia.Value);
            }
        }

        private void InheritanceProperty(string propName, object value)
        {
            PropInfoItem pi = this.PropertyByName(propName);
            if (pi == null || !pi.IsEnum)
                throw new ModelException(String.Format(L.T("Invalid inheritance attribute for the class \"{0}\" Property not found or not an enum \"{1}\"."), this.Name, propName), this.Name);
            pi.DefaultValue = value;
            pi.IsReadOnly = true;
            ClassInfoItem ci = this;
            ci._cidProp = pi;
            while (ci._super != null)
            {
                ci = ci._super;
                pi = ci.PropertyByName(propName);
                if (pi == null || pi.IsReadOnly)
                    break;
                pi.IsReadOnly = true;
            }

        }

        private void ResolveInheritancePk(ClassInfoItem pci)
        {
            if (!this.UseUuidAsPk)
            {
                throw new ModelException(L.T("Invalid PK attribute (inheritance)."), this.Name);
            }
            else if (!pci.UseUuidAsPk)
            {
                // 
                string[] akeys = new string[pci._key.Items.Count];
                for (int i = 0; i < pci._key.Items.Count; i++)
                {
                    akeys[i] = pci._key.Items[i].Key;
                }

                this._key.Items.Clear();
                CreatePkItems(akeys);
            }
        }

        private void ResolveInheritanceIndexes(ClassInfoItem pci)
        {
            if (pci.Indexes.Count > 0)
            {
                List<IndexInfo> list = new List<IndexInfo>();
                foreach (IndexInfo ii in pci.Indexes)
                {
                    IndexInfo nii = new IndexInfo();
                    nii.Load(ii.ItemsAsString(), ii.IndexName, ii.Unique, this);
                    list.Add(nii);
                }
                this._indexes.InsertRange(0, list);
            }

        }

        ///<summary>
        /// Prepare memory strutures for faster executing
        ///</summary>
        internal void ResolveInheritance(ModelManager model)
        {
            if (this.Static) return;
            if (this._inherianceResolved) return;
            if (this.CurrentType.BaseType != null)
            {
                this._super = model.ClassByType(this.CurrentType.BaseType);
                if (this._super != null)
                {
                    this._super.AddDescendant(this);
                    this._super.ResolveInheritance(model);
                    this.ResolveInheritancePersistence(this._super);
                    this.ResolveInheritancePk(this._super);
                    this.ResolveInheritanceIndexes(this._super);
                    this.AddRulesFromParent(this._super);

                }

            }
            this._inherianceResolved = true;
        }

        private void AddDescendant(ClassInfoItem ci)
        {
            this._descendants.Add(ci);
            if (this._super != null)
                this._super.AddDescendant(ci);
        }

        ///<summary>
        /// Prepare memory structures for faster executing
        ///</summary>
        internal void Loaded(ModelManager model)
        {
            this.CheckMehodsAndSetRules();
        }

        private void LoadTitle()
        {
            DisplayAttribute da = CurrentType.GetCustomAttributes(typeof(DisplayAttribute), false).FirstOrDefault() as DisplayAttribute;
            if (da != null)
            {
                this._title = da.Title;
                this._description = da.Description;
            }
            if (string.IsNullOrEmpty(_description))
                this._description = this._title;
            if (this.Static)
            {
                RulesForAttribute rf = CurrentType.GetCustomAttributes(typeof(RulesForAttribute), false).FirstOrDefault() as RulesForAttribute;
                if (rf != null)
                {
                    this.TargetType = rf.TargetType;
                }
            }

        }

        private void LoadProperties()
        {
            if (this.Static) return;
            PropertyInfo[] props = CurrentType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (PropertyInfo pi in props)
            {
                MethodInfo setMethod = pi.GetSetMethod();
                if (setMethod == null || !setMethod.IsVirtual || setMethod.IsFinal)
                {
                    continue;
                }

                if (!PropInfoItem.IsModelPrperty(pi))
                    continue;
                PropInfoItem item = new PropInfoItem(pi, this);
                _propsMap.Add(item.Name, item.PropInfo);
                _properties.Add(item);
                if (item.IsRole)
                {
                    _roles.Add(item);
                }
                else if (PropInfoItem.IsComplex(pi))
                {
                    _complexes.Add(item);
                }
            }
        }
        private void AddRuleItem(MethodInfo mi, RuleAttribute ra, string prop)
        {
            RuleItem ri = new RuleItem(mi, prop);
            ri.Kind = ra.Rule;
            ri.Operation = ra.Operation;
            if (Static)
            {
                if (ri.TargetType == null)
                    ri.TargetType = TargetType;
                if (ri.TargetType == null)
                {
                    throw new ModelException(String.Format(L.T("Invalid rule target for \"{0}\" in the class \"{1}\"."), mi.Name, Name), Name);
                }
            }
            else
            {
                ri.TargetType = CurrentType;
            }
            ri.DeclaringType = CurrentType;

            if (ra is StateAttribute)
                _stateRulesList.Add(ri);
            else
                _rulesList.Add(ri);
        }

        private void LoadMethodsAndRules()
        {
            BindingFlags bindingAttr = (Static ? (BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)
                : (BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static));
            //Load all methods
            MethodInfo[] methods = this.CurrentType.GetMethods(bindingAttr);
            foreach (MethodInfo mi in methods)
            {

                if (mi.Name.StartsWith("get_") || mi.Name.StartsWith("set_") || mi.Name.StartsWith("AOP")) continue;

                RuleAttribute[] ras = mi.GetCustomAttributes(typeof(RuleAttribute), false) as RuleAttribute[];
                if (ras != null)
                {
                    // mi is a Rule !!!
                    for (int index = 0, len = ras.Length; index < len; index++)
                    {
                        RuleAttribute ra = ras[index];
                        if (ra.Rule == Rule.Unknown)
                        {
                            throw new ModelException(String.Format(L.T("Invalid rule type for \"{0}\" in the class \"{1}\"."), mi.Name, Name), Name);
                        }
                        if (!ra.CheckProperty())
                        {
                            throw new ModelException(String.Format(L.T("Invalid rule property for \"{0}\" in the class \"{1}\"."), mi.Name, Name), Name);
                        }
                        string[] props = ra.Properties;
                        //TODO Code review is requred
                        if (props != null)
                        {
                            foreach (string prop in props)
                            {
                                AddRuleItem(mi, ra, prop);
                            }
                        }
                        else
                        {
                            AddRuleItem(mi, ra, ra.Property);
                        }
                    }
                }
                //Add methods
                if (TargetType != null)
                {
                    MethodItem method = new MethodItem(mi);
                    method.TargetType = TargetType;
                    method.DeclaringType = CurrentType;
                    _methodsList.Add(method);
                }
            }

        }

        private void LoadPersistence()
        {
            if (Static) return;

            if (typeof(IPersistentObj).IsAssignableFrom(CurrentType))
            {
                IsPersistent = true;
                ClassType = ClassType.Model;
                DbAttribute db = CurrentType.GetCustomAttributes(typeof(DbAttribute), false).FirstOrDefault() as DbAttribute;
                DbName = ((db == null) || string.IsNullOrEmpty(db.Name) ? Name : db.Name);
            }

        }
        /// <summary>
        /// Load primary key from attributes
        /// </summary>
        private void LoadPrimarykey()
        {
            if (Static) return;
            PrimaryKeyAttribute pk = this.CurrentType.GetCustomAttributes(typeof(PrimaryKeyAttribute), false).FirstOrDefault() as PrimaryKeyAttribute;
            string[] akeys;
            if (pk != null)
            {
                akeys = pk.Keys.Split(',');
                if (akeys.Length == 0)
                {
                    throw new ModelException(String.Format(L.T("Missing Primary key for {0}."), Name), Name);
                }
                this.UseUuidAsPk = ((akeys.Length == 1) && akeys[0] == ModelConst.UUID);
            }
            else
            {
                this.UseUuidAsPk = true;
                akeys = new string[] { ModelConst.UUID };

            }
            this.CreatePkItems(akeys);
        }

        private void CreatePkItems(string[] akeys)
        {
            foreach (string akey in akeys)
            {
                string skey = akey.Trim();
                PropInfoItem pi = PropertyByName(skey);
                if (pi == null)
                    throw new ModelException(String.Format(L.T("Invalid primary key declaration. Field not found '{0}.{1}'."), Name, skey), Name);
                this._key.Items.Add(new KeyItem(skey, pi.PropInfo));
            }
        }

        /// <summary>
        /// Load indexes from attributes
        /// </summary>
        private void LoadIndexes()
        {
            //load indexes 
            object[] attributes = this.CurrentType.GetCustomAttributes(typeof(IndexAttribute), false);
            foreach (object arttr in attributes)
            {
                IndexAttribute ia = arttr as IndexAttribute;
                IndexInfo ii = new IndexInfo();
                ii.Load(ia.Columns, ia.Name, ia.Unique, this);
                this._indexes.Add(ii);
            }
        }

        private static Type GetPropertiesStateType(Type type)
        {
            BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Public;
            var propertiesState = (from prop in type.GetProperties(bindingFlags)
                                   where prop.Name == ProperiesStatePropertyName && typeof(IDictionary<string, PropertyState>).IsAssignableFrom(prop.PropertyType) && prop.PropertyType.IsClass
                                   select prop).ToList();
            switch (propertiesState.Count)
            {
                case 0:
                    return typeof(DefaultPropertiesState);
                case 1:
                    break;
                default:
                    propertiesState.Sort((p1, p2) =>
                    {
                        return p1.DeclaringType.IsAssignableFrom(p2.DeclaringType) ? 1 : -1;
                    });
                    break;
            }
            return propertiesState[0].PropertyType;
        }


        #endregion

        #region Properties Access by name/type
        public PropertyInfo PropertyInfoByName(string propName)
        {
            PropertyInfo pi = null;
            this._propsMap.TryGetValue(propName, out pi);
            return pi;

        }
        public PropInfoItem PropertyByName(string propName)
        {
            PropertyInfo pi;
            PropInfoItem result;
            if (!this._propsMap.TryGetValue(propName, out pi) || !this._properties.TryGetValue(pi, out result))
            {
                return null;
            }
            return result;
        }

        #endregion

        #region Constructor
        public ClassInfoItem(Type cType, bool staticClass)
        {
            this.CurrentType = cType;
            this.TargetType = (staticClass ? null : CurrentType);
            this.StateClassType = GetPropertiesStateType(CurrentType);

            this.Name = CurrentType.Name;
            this._title = CurrentType.Name;
            this.Static = staticClass;
            this.LoadTitle();
            this.LoadPersistence();
            this.LoadProperties();
            this.LoadMethodsAndRules();
            this.LoadPrimarykey();
            this.LoadIndexes();
        }
        #endregion
    }
}

