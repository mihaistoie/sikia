﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sikia
{
    public interface IRoleChild
    {
        void SetParent(IInterceptedObject value);
    }
}
