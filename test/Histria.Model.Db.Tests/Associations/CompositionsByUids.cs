﻿using Histria.Core;
using Histria.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Histria.DbModel.Tests
{
    public class Car : InterceptedDbObject
    {
        public virtual string Name { get; set; }
        [Association(Relation.Composition, Inv = "Car", Min = 1)]
        public virtual HasOne<SteeringWheel> SteeringWheel { get; set; }
    }


    public class SteeringWheel : InterceptedDbObject
    {
        public virtual string SerialNumber { get; set; }
        [Association(Relation.Composition, Inv = "SteeringWheel", Min = 1)]
        public virtual BelongsTo<Car> Car { get; set; }
    }

}
