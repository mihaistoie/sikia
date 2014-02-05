﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace Sikia.Db.Model
{
    public class DbTables : KeyedCollection<string, DbTable>
    {
        protected override string GetKeyForItem(DbTable item)
        {
            return item.TableName.ToLower();
        }
    }

}