﻿// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Permissive License.
// See http://www.microsoft.com/opensource/licenses.mspx#Ms-PL.
// All other rights reserved.

using System;
using System.Collections;
using System.Diagnostics;
using System.Runtime.InteropServices;
using UIAComWrapperInternal;

namespace System.Windows.Automation
{
    public class TransformPattern : BasePattern
    {
        
        private UIAutomationClient.IUIAutomationTransformPattern _pattern;
        public static readonly AutomationPattern Pattern = TransformPatternIdentifiers.Pattern;
        public static readonly AutomationProperty CanMoveProperty = TransformPatternIdentifiers.CanMoveProperty;
        public static readonly AutomationProperty CanResizeProperty = TransformPatternIdentifiers.CanResizeProperty;
        public static readonly AutomationProperty CanRotateProperty = TransformPatternIdentifiers.CanRotateProperty;

        
        private TransformPattern(AutomationElement el, UIAutomationClient.IUIAutomationTransformPattern pattern, bool cached)
            : base(el, cached)
        {
            Debug.Assert(pattern != null);
            this._pattern = pattern;
        }

        internal static object Wrap(AutomationElement el, object pattern, bool cached)
        {
            return (pattern == null) ? null : new TransformPattern(el, (UIAutomationClient.IUIAutomationTransformPattern)pattern, cached);
        }

        public void Move(double x, double y)
        {
            try
            {
                this._pattern.Move(x, y);
            }
            catch (System.Runtime.InteropServices.COMException e)
            {
                Exception newEx; if (Utility.ConvertException(e, out newEx)) { throw newEx; } else { throw; }
            }
        }

        public void Resize(double width, double height)
        {
            try
            {
                this._pattern.Resize(width, height);
            }
            catch (System.Runtime.InteropServices.COMException e)
            {
                Exception newEx; if (Utility.ConvertException(e, out newEx)) { throw newEx; } else { throw; }
            }
        }

        public void Rotate(double degrees)
        {
            try
            {
                this._pattern.Rotate(degrees);
            }
            catch (System.Runtime.InteropServices.COMException e)
            {
                Exception newEx; if (Utility.ConvertException(e, out newEx)) { throw newEx; } else { throw; }
            }
        }

        
        public TransformPatternInformation Cached
        {
            get
            {
                Utility.ValidateCached(this._cached);
                return new TransformPatternInformation(this._el, true);
            }
        }

        public TransformPatternInformation Current
        {
            get
            {
                return new TransformPatternInformation(this._el, false);
            }
        }


        
        [StructLayout(LayoutKind.Sequential)]
        public struct TransformPatternInformation
        {
            private AutomationElement _el;
            private bool _isCached;
            internal TransformPatternInformation(AutomationElement element, bool isCached)
            {
                this._el = element;
                this._isCached = isCached;
            }

            public bool CanMove
            {
                get
                {
                    return (bool)this._el.GetPropertyValue(TransformPattern.CanMoveProperty, _isCached);
                }
            }
            public bool CanResize
            {
                get
                {
                    return (bool)this._el.GetPropertyValue(TransformPattern.CanResizeProperty, _isCached);
                }
            }
            public bool CanRotate
            {
                get
                {
                    return (bool)this._el.GetPropertyValue(TransformPattern.CanRotateProperty, _isCached);
                }
            }
        }
    }
}