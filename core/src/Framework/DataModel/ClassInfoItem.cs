﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Sikia.Framework.Attributes;
using Sikia.Framework.Utils;
using System.Collections.ObjectModel;

namespace Sikia.Framework.DataModel
{
    class PropCollection : KeyedCollection<PropertyInfo, PropinfoItem>
    {
        protected override PropertyInfo GetKeyForItem(PropinfoItem item)
        {
            return item.PropInfo;
        }
    }
    class KeysCollection : KeyedCollection<PropertyInfo, KeyItem>
    {
        protected override PropertyInfo GetKeyForItem(KeyItem item)
        {
            return item.Property;
        }
    }


    public enum ClassType { Unknown, Model, ViewModel, Process };

    public class ClassInfoItem
    {
        #region private members
        private readonly Dictionary<string, PropertyInfo> propsMap = new Dictionary<string, PropertyInfo>();
        private readonly KeyedCollection<PropertyInfo, PropinfoItem> properties = new PropCollection();
        private readonly KeyedCollection<PropertyInfo, KeyItem> keys;
        private readonly List<IndexInfo> indexes = new List<IndexInfo>();
        #endregion
        public Type ClassTypeInfo { get; set; }
        public ClassType ClassType = ClassType.Unknown;
        public string Name { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string DbName { get; set; }

        #region Properties/Keys/Indexes
        public KeyedCollection<PropertyInfo, PropinfoItem> Properties { get { return properties; } }
        public KeyedCollection<PropertyInfo, KeyItem> Keys { get { return keys; } }
        public List<IndexInfo> Indexes { get { return indexes; } }
        #endregion
        public PropertyInfo Name2Propertyinfo(string propName)
        {
            try
            {
                return propsMap[propName];
            }
            catch (Exception)
            {
                return null;
            }
        }

        public ClassInfoItem(Type cType)
        {
            ClassTypeInfo = cType;
            Name = StrUtils.Namespace2Name(ClassTypeInfo.ToString());
            Title = Name;

            DisplayAttribute da = ClassTypeInfo.GetCustomAttributes(typeof(DisplayAttribute), false).FirstOrDefault() as DisplayAttribute;
            Title = Name;
            if (da != null)
            {
                Title = da.Title;
                Description = da.Description;
            }
            if (Description == "") Description = Title;

            PropertyInfo[] props = ClassTypeInfo.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (PropertyInfo pi in props)
            {
                PropinfoItem item = new PropinfoItem(pi);
                propsMap.Add(item.Name, item.PropInfo);
                properties.Add(item);

            }
            DbAttribute db = ClassTypeInfo.GetCustomAttributes(typeof(DbAttribute), false).FirstOrDefault() as DbAttribute;
            if (db != null)
            {
                ClassType = ClassType.Model;
                DbName = (String.IsNullOrEmpty(db.TableName) ? Name : db.TableName);
                // Load primary key
                keys = new KeysCollection();
                string[] akeys = db.Keys.Split(',');
                if (akeys.Length == 0)
                {
                    throw new ModelException(String.Format(StrUtils.TT("Missing Primary key for {0}."), Name), Name);
                }
                foreach (string akey in akeys)
                {
                    string key = akey.Trim();
                    PropertyInfo pi = Name2Propertyinfo(key);
                    if (pi == null)
                        throw new ModelException(String.Format(StrUtils.TT("Invalid field {0}  in Primary key for {1}."), key, Name), Name);
                    keys.Add(new KeyItem(key, pi));
                }

            }
            //load indexes 
            object[] attributes = ClassTypeInfo.GetCustomAttributes(typeof(IndexAttribute), false);
            foreach (object arttr in attributes)
            {
                IndexAttribute ia = arttr as IndexAttribute;
                IndexInfo ii = new IndexInfo();
                ii.Load(ia.Columns, ia.Name, ia.Unique, this);
                indexes.Add(ii);
            };



        }

    }
}

