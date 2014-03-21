﻿using Histria.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Histria.Model
{
    ///<summary>
    /// Used to register an attribute 
    ///</summary>  
    public sealed class TemplateManager
    {
        #region Private Members
        private readonly Dictionary<string, TypeAttribute> templates;
        #endregion

        #region Singleton thread-safe pattern
        private static volatile TemplateManager instance = null;
        private static object syncRoot = new Object();
        private TemplateManager()
        {
            templates = new Dictionary<string, TypeAttribute>();
        }

        public static TemplateManager Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                        {
                            TemplateManager tm = new TemplateManager();
                            instance = tm;
                        }
                    }
                }
                return instance;
            }
        }
        #endregion

        public static void Register(string templateName, TypeAttribute template)  
        {
        }
    }
}