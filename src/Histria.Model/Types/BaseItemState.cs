﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Histria.Model
{
    public class BaseItemState
    {
        #region Private Members
        private List<string> errors;
        private List<string> warnings;
        private void CheckErrors()
        {
            if (errors == null)
                errors = new List<string>();
        }
        private void CheckWarnings()
        {
            if (warnings == null)
                warnings = new List<string>();
        }

        #endregion

        #region State Properties
        public virtual bool IsHidden { get; set; }
        public virtual bool IsDisabled { get; set; }
        public virtual bool IsMandatory { get; set; }
        #endregion

        #region Errors and Warnings
        public void AddError(string error)
        {
            CheckErrors();
            errors.Add(error);
        }
        public void AddWarning(string warning)
        {
            CheckWarnings();
            warnings.Add(warning);
        }
        public void Clear()
        {
            if (errors != null)
            {
                errors.Clear();
            }
            if (warnings != null)
            {
                warnings.Clear();
            }
        }

        #endregion
    }
}
