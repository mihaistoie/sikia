﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sikia.Framework;
using Sikia.Framework.Model;
using Sikia.Settings;
using System;
using UnitTestModel.Models;


namespace UnitTestModel
{
    [TestClass]
    public class ModelTests
    {
        [TestMethod]
        [ExpectedException(typeof(ModelException))]
        public void InvalidRuleDefinition()
        {
            ApplicationConfig config = new ApplicationConfig();
            Type[] Types = { typeof(MInvalidRule) };
            config.Types = Types;
            ModelManager m = ModelManager.LoadModel(config);

        }

        [TestMethod]
        [ExpectedException(typeof(ModelException))]
        public void DuplicatedRule()
        {
            ApplicationConfig config = new ApplicationConfig();
            Type[] Types = { typeof(MR1), typeof(MR2) };
            config.Types = Types;
            ModelManager m = ModelManager.LoadModel(config);

        }

        [TestMethod]
        public void StaticClassTitle()
        {
            ApplicationConfig config = new ApplicationConfig();
            Type[] Types = { typeof(MR3) };
            config.Types = Types;
            ModelManager m = ModelManager.LoadModel(config);
            ClassInfoItem ci = m.Class<MR3>();
            Assert.AreEqual(ci.Title, "M3-T", "Class static title");
            Assert.AreEqual(ci.Description, "M3-D", "Class static description");
        }

        [TestMethod]
        public void DynamicClassTitle()
        {
            ApplicationConfig config = new ApplicationConfig();
            Type[] Types = { typeof(MR4) };
            config.Types = Types;
            ModelManager m = ModelManager.LoadModel(config);
            ClassInfoItem ci = m.Class<MR4>();
            Assert.AreEqual(ci.Title, "MR4.xxx", "Class dynamic title");
            Assert.AreEqual(ci.Description, "MR4.yyy", "Class dynamic title");
        }


    }
}