﻿using Sikia.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sikia.DataTypes
{
    public class BelongsTo<T> : Association<T> where T: InterceptedObject
    {
    }
}