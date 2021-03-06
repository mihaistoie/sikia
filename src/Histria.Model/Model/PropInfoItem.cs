﻿using Histria.Sys;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Histria.Model
{

    ///<summary>
    /// Provides access to  property metadata
    ///</summary>   
    public class PropInfoItem
    {
        #region Type initialization

        private static void BigIntAction(PropInfoItem item)
        {
            item.DtType = DataTypes.BigInt;
        }

        private static void GuidAction(PropInfoItem item)
        {
            item.DtType = DataTypes.uuid;
        }


        private static void IntAction(PropInfoItem item)
        {
            item.DtType = DataTypes.Int;
        }
        private static void ShortAction(PropInfoItem item)
        {
            item.DtType = DataTypes.SmallInt;
        }
        private static void StringAction(PropInfoItem item)
        {
            item.DtType = DataTypes.String;
            item.TypeValidation = item.PropInfo.GetCustomAttributes(typeof(DtStringAttribute), false).FirstOrDefault() as DtStringAttribute;
            if (item.TypeValidation == null)
                item.TypeValidation = new DtStringAttribute() { Template = TemplateManager.DefaultString };
            DtStringAttribute sa = (DtStringAttribute)item.TypeValidation;
            item.DbSize = (sa.MaxLength <= 0) ? DataTypesConsts.MAX_STRING_SIZE : sa.MaxLength;
            //
        }
        private static void BoolAction(PropInfoItem item)
        {
            item.DtType = DataTypes.Bool;
        }

        private static void NumberAction(PropInfoItem item)
        {
            item.DtType = DataTypes.Number;
            item.TypeValidation = item.PropInfo.GetCustomAttributes(typeof(DtNumberAttribute), false).FirstOrDefault() as DtNumberAttribute;
        }
        private static void DateTimeAction(PropInfoItem item)
        {
            item.DtType = DataTypes.DateTime;
        }
        private static void EnumAction(PropInfoItem item)
        {
            //
            item.DtType = DataTypes.Enum;
            if (item.EnumInfo != null)
            {
                if (item.EnumInfo.StoredAsString)
                {
                    item.DtType = DataTypes.String;
                    item.DbSize = DataTypesConsts.MAX_CODE_SIZE;
                }
            }
        }
        private static void UnknownAction(PropInfoItem item)
        {
            item.DtType = DataTypes.Unknown;
        }

        private static Dictionary<Type, Action<PropInfoItem>> handleAction =
            new Dictionary<Type, Action<PropInfoItem>>() 
            { 
                { typeof(Guid), GuidAction },
                { typeof(long), BigIntAction },
                { typeof(long?), BigIntAction },
                { typeof(ulong), BigIntAction },
                { typeof(ulong?), BigIntAction },
                { typeof(int), IntAction },
                { typeof(int?), IntAction },
                { typeof(uint), IntAction },
                { typeof(uint?), IntAction },
                { typeof(short), ShortAction },
                { typeof(short?), ShortAction },
                { typeof(ushort), ShortAction },
                { typeof(ushort?), ShortAction },
                { typeof(sbyte), ShortAction },
                { typeof(sbyte?), ShortAction },
                { typeof(byte), ShortAction },
                { typeof(byte?), ShortAction },
                { typeof(char), ShortAction },
                { typeof(char?), ShortAction },
                { typeof(string), StringAction },
                { typeof(bool), BoolAction },
                { typeof(bool?), BoolAction },
                { typeof(decimal), NumberAction },
                { typeof(decimal?), NumberAction },
                { typeof(double), NumberAction },
                { typeof(float), NumberAction },
                { typeof(DateTime?), DateTimeAction },
                { typeof(DateTime), DateTimeAction }

            };

        private void InitializeType()
        {
            if (this.IsEnum)
            {
                EnumAction(this);
            }
            else
            {
                Action<PropInfoItem> action;
                if (handleAction.TryGetValue(this.PropInfo.PropertyType, out action))
                {
                    action(this);
                }
                else
                {
                    UnknownAction(this);
                }
            }
        }


        #endregion

        #region Title and description

        private void InitializeTitleAndDescription()
        {
            title = Name;
            DisplayAttribute da = PropInfo.GetCustomAttributes(typeof(DisplayAttribute), false).FirstOrDefault() as DisplayAttribute;
            if (da != null)
            {
                title = da.Title;
                description = da.Description;
            }
            if (string.IsNullOrEmpty(description))
                description = title;
        }

        #endregion

        #region Persistence initialization
        // This property has db attribute ?  
        private bool _dbAttribute = false;

        private void InitializePersistence()
        {
            this.DbName = this.PropInfo.Name;
            this.IsPersistent = this.ClassInfo.IsPersistent;
            DbAttribute pa = this.PropInfo.GetCustomAttributes(typeof(DbAttribute), false).FirstOrDefault() as DbAttribute;
            if (pa != null)
            {
                if (!string.IsNullOrEmpty(pa.Name))
                {
                    this.DbName = pa.Name;
                    this._dbAttribute = true;
                }
            }
            else
            {
                NoPersistentlAttribute np = this.PropInfo.GetCustomAttributes(typeof(NoPersistentlAttribute), false).FirstOrDefault() as NoPersistentlAttribute;
                if (np != null)
                    this.IsPersistent = false;
            }
        }

        #endregion

        #region Default state initialization

        private void InitializeDefaultState()
        {
            DefaultAttribute dfa = PropInfo.GetCustomAttributes(typeof(DefaultAttribute), false).FirstOrDefault() as DefaultAttribute;
            if (dfa != null)
            {
                IsDisabled = dfa.Disabled;
                IsHidden = dfa.Hidden;
                IsMandatory = dfa.Required;
                IsReadOnly = dfa.ReadOnly;
                DefaultValue = dfa.Value;
            }
        }

        #endregion

        #region Initialize Role info

        void InitializeRole()
        {
            Type associationType = typeof(IAssociation);
            Type roleListType = typeof(IRoleList);
            Type roleRefType = typeof(IRoleRef);
            Type roleChild = typeof(IRoleChild);

            //Association
            if (associationType.IsAssignableFrom(this.PropInfo.PropertyType))
            {
                if (!this._dbAttribute)
                {
                    this.DbName = null;
                } 
                AssociationAttribute ra = this.PropInfo.GetCustomAttributes(typeof(AssociationAttribute), false).FirstOrDefault() as AssociationAttribute;
                if (ra == null)
                {
                    throw new ModelException(String.Format(L.T("Association attribute is missing.({0}.{1})"), Name, this.ClassInfo.Name), this.ClassInfo.Name);
                }

                RoleInfoItem role = null;

                if (roleListType.IsAssignableFrom(this.PropInfo.PropertyType))
                {
                    role = new RoleInfoItem(this) { Min = ra.Min, Max = ra.Max, InvRoleName = ra.Inv, Type = ra.Type, IsList = true, IsChild = false, ClassType = this.ClassInfo.CurrentType };
                }
                else if (roleRefType.IsAssignableFrom(this.PropInfo.PropertyType))
                {
                    role = new RoleInfoItem(this) { Min = ra.Min, Max = ra.Max, InvRoleName = ra.Inv, Type = ra.Type, ForeignKey = ra.ForeignKey, IsList = false, IsChild = false, ClassType = this.ClassInfo.CurrentType };
                    if (roleChild.IsAssignableFrom(this.PropInfo.PropertyType))
                    {
                        if ((ra.Type != Relation.Embedded) || (ra.Type != Relation.Composition) || (ra.Type != Relation.Aggregation))
                        {
                            role.IsChild = true;
                            if (ra.Type != Relation.Aggregation)
                            {
                                if (ClassInfo.Parent != null)
                                    throw new ModelException(String.Format(L.T("Invalid model. Multiple parents : {0}.{1} - {2}.{1}.)"), Name, this.ClassInfo.Name, ClassInfo.Parent.Name), this.ClassInfo.Name);
                                ClassInfo.Parent = this;
                            }
                        }
                        else
                        {
                            throw new ModelException(String.Format(L.T("Invalid association type.({0}.{1}. Excepted composition or aggregation.)"), Name, this.ClassInfo.Name), this.ClassInfo.Name);
                        }
                    }
                }
            }
        }

        #endregion

        #region Internal fields
        // Static title and description
        private string title;
        private string description;
        // Handlers to get title and description
        private MethodInfo titleGet = null;
        private MethodInfo descriptionGet = null;
        // Rules by type
        private readonly Dictionary<Rule, RuleList> rules = new Dictionary<Rule, RuleList>();
        // State Rules by type
        private readonly Dictionary<Rule, RuleList> statesRules = new Dictionary<Rule, RuleList>();

        private PropInfoItem() { }

        #endregion

        #region Properties
        public PropertyInfo PropInfo { get; internal set; }

        ///<summary>
        /// Property info of model
        ///</summary>   
        public PropInfoItem ModelPropInfo { get; internal set; }

        ///<summary>
        /// Use reflection to get value ?
        ///</summary>   
        public bool CanGetValueByReflection { get { return !IsRole; } }


        ///<summary>
        /// Roles which depend on this property
        ///</summary>   
        internal List<RoleInfoItem> dependOnMe = null;

        ///<summary>
        /// Class info 
        ///</summary>   
        public ClassInfoItem ClassInfo { get; internal set; }

        ///<summary>
        /// Name of property
        ///</summary>   
        public string Name { get; internal set; }

        ///<summary>
        /// Column Name - Database Mapping
        ///</summary>   
        public string DbName { get; internal set; }

        ///<summary>
        /// Is stored in db ?
        ///</summary>   
        public bool IsPersistent { get; internal set; }

        ///<summary>
        /// Default is required ?
        ///</summary>   
        public bool IsMandatory { get; internal set; }

        ///<summary>
        /// Default is hidden ?
        ///</summary>   
        public bool IsHidden { get; internal set; }

        ///<summary>
        /// Default is disabled ?
        ///</summary>   
        public bool IsDisabled { get; internal set; }

        ///<summary>
        /// Read Only Property
        /// 
        ///</summary>   
        public bool IsReadOnly { get; internal set; }

        ///<summary>
        /// Default Value ?
        ///</summary>   
        public object DefaultValue { get; internal set; }

        /// <summary>
        /// Is enum ?
        /// </summary>
        internal bool IsEnum { get { return this.PropInfo.PropertyType.IsEnum; } }



        ///<summary>
        /// Title of property
        ///</summary>   
        public string Title
        {
            get
            {
                if (titleGet != null)
                    return (string)titleGet.Invoke(this, null);
                if ((title == Name) && ModelPropInfo != null)
                {
                    return ModelPropInfo.Title;
                }
                return title;
            }
        }

        ///<summary>
        /// A short description of property 
        ///</summary>   
        public string Description
        {
            get
            {
                if (descriptionGet != null)
                    return (string)descriptionGet.Invoke(this, null);
                if ((description == Name) && ModelPropInfo != null)
                {
                    return ModelPropInfo.Description;
                }
                return description;
            }
        }

        ///<summary>
        /// Role detail
        ///</summary>   
        public RoleInfoItem Role { get; internal set; }

        ///<summary>
        /// IsRole ?
        ///</summary>  
        public bool IsRole { get { return Role != null; } }

        ///<summary>
        /// Schema validation
        ///</summary>   
        internal TypeAttribute TypeValidation;

        ///<summary>
        /// Data type
        ///</summary> 
        internal DataTypes DtType = DataTypes.Unknown;



        ///<summary>
        /// DB Size
        ///</summary>   
        internal int DbSize = 0;

        //<summary>
        /// Data type
        ///</summary>   
        internal EnumInfoItem EnumInfo = null;



        #endregion

        #region Loading
        internal static bool IsModelPrperty(PropertyInfo pi)
        {
            if (pi.PropertyType.IsEnum || handleAction.ContainsKey(pi.PropertyType))
                return true;
            Type associationType = typeof(IAssociation);
            if (associationType.IsAssignableFrom(pi.PropertyType))
                return true;
            if (IsComplex(pi))
                return true;
            return false;
        }

        internal static bool IsComplex(PropertyInfo pi)
        {
            Type ct = typeof(IComplexData);
            if (ct.IsAssignableFrom(pi.PropertyType))
                return true;
            return false;
        }

        internal PropInfoItem(PropertyInfo cPi, ClassInfoItem ci)
        {
            PropInfo = cPi;
            ClassInfo = ci;
            Name = PropInfo.Name;
            InitializeTitleAndDescription();
            InitializeType();
            InitializePersistence();
            InitializeDefaultState();
            InitializeRole();
        }

        internal void AddRole(RoleInfoItem role)
        {
            if (this.dependOnMe == null)
            {
                this.dependOnMe = new List<RoleInfoItem>();
            }
            this.dependOnMe.Add(role);
        }

        public void InitializeView(ModelManager model, ClassInfoItem ci)
        {
            if (ci.IsView)
            {
                ViewInfoItem vi = (ViewInfoItem)ci;
                if (vi.ModelClass != null)
                    ModelPropInfo = vi.ModelClass.PropertyByName(Name);
            }
        }

        internal void AfterLoad(ModelManager model, ClassInfoItem ci)
        {
            this.InitializeView(model, ci);
            if (this.IsRole)
            {
                // Check role && Load role dependencies

                RoleInfoItem role = this.Role;
                if (role.Type != Relation.List && role.IsList && string.IsNullOrEmpty(role.InvRoleName))
                {
                    throw new ModelException(String.Format(L.T("Invalid role definition {0}.{1}. Missing 'Inv' attribute."), ci.Name, PropInfo.Name), ci.Name);
                }
                Type invClassType = PropInfo.PropertyType.GetGenericArguments()[0];

                ClassInfoItem remoteClass = model.ClassByType(invClassType);
                if (remoteClass == null)
                {
                    throw new ModelException(String.Format(L.T("Invalid role definition {0}.{1}. Remote class not found."), ci.Name, PropInfo.Name), ci.Name);
                }
                role.RemoteClass = remoteClass;
                if (!string.IsNullOrEmpty(role.InvRoleName))
                {
                    PropInfoItem pp = remoteClass.PropertyByName(role.InvRoleName);
                    if (pp != null)
                    {
                        role.InvRole = pp.Role;
                    }
                    if (role.InvRole == null)
                        throw new ModelException(String.Format(L.T("Invalid role definition {0}.{1}. Invalid inv role {2}.{3}."), ci.Name, PropInfo.Name, remoteClass.Name, role.InvRoleName), ci.Name);
                }
                if (role.InvRole != null)
                {
                    //One-to-one relationship 
                    if (role.IsRef && !role.IsChild && !role.InvRole.IsList && role.InvRole.IsChild)
                    {
                        role.IsList = true;
                    }
                }
                if (role.IsRef)
                {
                    role.UsePk = false;
                    if (!string.IsNullOrEmpty(role.ForeignKey))
                    {
                        if (this._dbAttribute)
                        {
                            throw new ModelException(String.Format(L.T("Invalid db attribute for {0}.{1}."), ci.Name, PropInfo.Name), ci.Name);
                        }
                        string[] fks = role.ForeignKey.Split(',');
                        role.PkFields = new List<PKeyInfo>(fks.Length);
                        role.FkFields = new List<ForeignKeyInfo>(fks.Length);
                        role.UsePk = role.ForeignKey.IndexOf("=") < 0;
                        if (role.UsePk && (fks.Length != remoteClass.Key.Items.Count))
                        {
                            throw new ModelException(String.Format(L.T("Invalid role definition {0}.{1}. Invalid inv role {2}.{3}."), ci.Name, PropInfo.Name, remoteClass.Name, role.InvRoleName), ci.Name);
                        }
                        int index = 0;
                        foreach (string fk in fks)
                        {
                            if (role.UsePk)
                            {
                                role.FkFields.Add(new ForeignKeyInfo() { Field = fk.Trim() });
                                role.PkFields.Add(new PKeyInfo() { Field = remoteClass.Key.Items[index].Key });

                            }
                            else
                            {
                                string ff = fk.Trim();
                                bool readOnly = false;
                                int pos = ff.IndexOf("==");
                                if (pos <= 0)
                                {
                                    pos = ff.IndexOf("=");
                                }
                                else
                                {
                                    readOnly = true;
                                }
                                if (pos <= 0)
                                {
                                    throw new ModelException(String.Format(L.T("Invalid role definition {0}.{1}. Invalid foreing key '{2}'."), ci.Name, PropInfo.Name, role.ForeignKey), ci.Name);
                                }
                                role.FkFields.Add(new ForeignKeyInfo() { Field = fk.Substring(0, pos).Trim(), ReadOnly = readOnly });
                                role.PkFields.Add(new PKeyInfo() { Field = fk.Substring(pos + (readOnly ? 2 : 1)).Trim() });
                            }

                            index++;
                        }

                    }
                    else
                    {
                        //Using Uuid
                        role.UsePk = remoteClass.UseUuidAsPk;
                        role.PkFields = new List<PKeyInfo>() { new PKeyInfo() { Field = ModelConst.UUID } };
                        ForeignKeyInfo fki = new ForeignKeyInfo() { Field = ModelConst.RefProperty(Name), Prop = this };
                        if (!fki.Prop._dbAttribute)
                            fki.Prop.DbName = fki.Field;

                        role.FkFields = new List<ForeignKeyInfo>() { fki };
                        role.FkFieldsExist = false;

                    }

                    if (!role.UsePk && role.PkFields.Count == remoteClass.Key.Items.Count)
                    {
                        role.UsePk = true;
                        for (int i = 0; i < role.PkFields.Count; i++)
                        {
                            if (string.Compare(role.PkFields[i].Field, remoteClass.Key.Items[i].Key, true) != 0)
                            {
                                role.UsePk = false;
                            }
                        }
                    }
                    //Check if fields exists  
                    for (int i = 0, len = role.PkFields.Count; i < len; i++)
                    {
                        ForeignKeyInfo fi = role.FkFields[i];
                        PropInfoItem pp = remoteClass.PropertyByName(role.PkFields[i].Field);
                        role.PkFields[i].Prop = pp;
                        #region Check Model
                        if (model.CheckModel &&pp == null)
                        {
                            throw new ModelException(String.Format(L.T("Invalid role definition {0}.{1}. Field not found '{2}.{3}'."), ci.Name, PropInfo.Name, remoteClass.Name, role.PkFields[i]), ci.Name);
                        }
                        #endregion

                        if (role.FkFieldsExist)
                        {
                            PropInfoItem fp = ci.PropertyByName(fi.Field);
                            fi.Prop = fp;
                            fp.AddRole(role);
                            #region Check Model
                            if (model.CheckModel)
                            {
                                if (fp == null)
                                {
                                    throw new ModelException(String.Format(L.T("Invalid role definition {0}.{1}. Field not found '{2}.{3}'."), ci.Name, PropInfo.Name, ci.Name, fi), ci.Name);
                                }

                                if (fp.PropInfo.PropertyType != pp.PropInfo.PropertyType)
                                {
                                    throw new ModelException(String.Format(L.T("Invalid role definition {0}.{1}. Type mismatch '{2}({3}.{4}) != {5}({6}.{7})'."),
                                        ci.Name, PropInfo.Name, fp.PropInfo.PropertyType.Name, ci.Name, fp.Name,
                                        pp.PropInfo.PropertyType.Name, remoteClass.Name, pp.Name), ci.Name);
                                }
                                if (this.IsPersistent && (this.IsPersistent != fi.Prop.IsPersistent))
                                {
                                    throw new ModelException(String.Format(L.T("Invalid role definition {0}.{1}. Persistence error."), ci.Name, PropInfo.Name));

                                }
                            }
                            #endregion
                        }

                    }
                }

            }
            else if (this.IsEnum)
            {
                model.Enums.TryGetEnumInfo(this.PropInfo.PropertyType, out this.EnumInfo);
                if (this.EnumInfo == null)
                {
                    throw new ModelException(String.Format(L.T("Invalid enum type for property '{0}.{1}'. (Enum not found in model)"), ci.Name, this.PropInfo.Name), ci.Name);
                }
                EnumAction(this);
            }

        }

        #endregion

        #region  Validation
        ///<summary>
        /// Check value (range, length ....) 
        ///</summary>   
        public void SchemaValidation(ref object value)
        {
            if (TypeValidation != null)
            {
                value = TypeValidation.SchemaValidation(value);
            }
        }

        #endregion

        #region Rules
        ///<summary>
        /// Associate a rule to this property
        ///</summary>   
        internal void AddRule(RuleItem ri)
        {
            RuleHelper.AddRule(rules, ri);
        }

        ///<summary>
        /// Associate a state rule to this property
        ///</summary>   
        internal void AddStateRule(RuleItem ri)
        {
            RuleHelper.AddRule(statesRules, ri);
        }
        ///<summary>
        /// Execute rules by type
        ///</summary>   
        public void ExecuteRules(Rule kind, object target, RoleOperation operation, string detail, object[] arguments = null)
        {
            RuleHelper.ExecuteRules(rules, kind, target, operation, detail, arguments);
        }

        ///<summary>
        /// Execute state rules by type
        ///</summary>   
        public void ExecuteStateRules(Rule kind, object target, RoleOperation operation, string detail, object[] arguments = null)
        {
            RuleHelper.ExecuteRules(statesRules, kind, target, operation, detail, arguments);
        }

        public void ExecuteCheckValueRules(object target, ref object value)
        {
            RuleList rl = null;
            if (rules.TryGetValue(Rule.Correction, out rl))
            {
                rl.ExecuteCheckValueRules(target, ref value);
            }
        }

        #endregion
    }
}

