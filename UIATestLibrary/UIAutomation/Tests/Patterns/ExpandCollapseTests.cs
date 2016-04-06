// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Permissive License.
// See http://www.microsoft.com/opensource/licenses.mspx#Ms-PL.
// All other rights reserved.


/******************************************************************* 
* Purpose: InternalHelper
*******************************************************************/
using System;
using System.CodeDom;
using System.Collections;
using Drawing = System.Drawing;
using System.Globalization;
using System.Threading;
using System.Windows.Automation;
using System.Windows;
using ATGTestInput;
using System.Windows.Input;

namespace InternalHelper.Tests.Patterns
{
	using InternalHelper;
	using InternalHelper.Tests;
	using InternalHelper.Enumerations;
	using Microsoft.Test.UIAutomation;
	using Microsoft.Test.UIAutomation.Core;
	using Microsoft.Test.UIAutomation.TestManager;
	using Microsoft.Test.UIAutomation.Interfaces;

    /// -----------------------------------------------------------------------
    /// <summary></summary>
    /// -----------------------------------------------------------------------
    public class ExpandCollapsePatternWrapper : PatternObject
    {
        #region Variables

        // Pattern variable
        ExpandCollapsePattern m_pattern;

        #endregion Variables

		/// -------------------------------------------------------------------
		/// <summary></summary>
		/// -------------------------------------------------------------------
		internal ExpandCollapsePatternWrapper(AutomationElement element, string testSuite, TestPriorities priority, TypeOfControl typeOfControl, TypeOfPattern typeOfPattern, string dirResults, bool testEvents, IApplicationCommands commands)
            :
            base(element, testSuite, priority, typeOfControl, typeOfPattern, dirResults, testEvents, commands)
        {
            m_pattern = (ExpandCollapsePattern)GetPattern(m_le, m_useCurrent, ExpandCollapsePattern.Pattern);
        }

		/// -------------------------------------------------------------------
		/// <summary></summary>
		/// -------------------------------------------------------------------
		internal ExpandCollapseState pattern_getExpandCollapseState
        {
            get
            {
                if (m_useCurrent)
                    return m_pattern.Current.ExpandCollapseState;
                else
                    return m_pattern.Cached.ExpandCollapseState;
            }
        }

        #region Methods

        internal void pattern_Collapse(Type expectedException, CheckType checkType)
        {
            string call = "Collapse()";
            try
            {
                m_pattern.Collapse();
                System.Threading.Thread.Sleep(1);
            }
            catch (Exception actualException)
            {
                TestException(expectedException, actualException, call, checkType);
                return;
            }
            TestNoException(expectedException, call, checkType);
        }

        internal void pattern_Expand(Type expectedException, CheckType checkType)
        {
            string call = "Expand()";
            try
            {
                m_pattern.Expand();
                System.Threading.Thread.Sleep(1);
            }
            catch (Exception actualException)
            {
                TestException(expectedException, actualException, call, checkType);
                return;
            }
            TestNoException(expectedException, call, checkType);
        }

        #endregion Methods
    }
}

namespace Microsoft.Test.UIAutomation.Tests.Patterns
{
    using InternalHelper;
    using InternalHelper.Tests;
    using InternalHelper.Tests.Patterns;
    using InternalHelper.Enumerations;
    using Microsoft.Test.UIAutomation;
    using Microsoft.Test.UIAutomation.Core;
    using Microsoft.Test.UIAutomation.TestManager;
    using Microsoft.Test.UIAutomation.Interfaces;

    /// -----------------------------------------------------------------------
    /// <summary></summary>
    /// -----------------------------------------------------------------------
	public sealed class ExpandCollapseTests : ExpandCollapsePatternWrapper
    {
        #region Member variables

        #endregion
        const string THIS = "ExpandCollapseTests";

		/// -------------------------------------------------------------------
		/// <summary></summary>
		/// -------------------------------------------------------------------
		public const string TestSuite = NAMESPACE + "." + THIS;

		/// -------------------------------------------------------------------
		/// <summary></summary>
		/// -------------------------------------------------------------------
		public static readonly string TestWhichPattern = Automation.PatternName(ExpandCollapsePattern.Pattern);

		/// -------------------------------------------------------------------
		/// <summary></summary>
		/// -------------------------------------------------------------------
		public ExpandCollapseTests(AutomationElement element, TestPriorities priority, string dirResults, bool testEvents, TypeOfControl typeOfControl, IApplicationCommands commands)
            :
            base(element, TestSuite, priority, typeOfControl, TypeOfPattern.ExpandCollapse, dirResults, testEvents, commands)
        {
        }

        #region Tests

        enum MethodToExecute
        {
            Expand,
            Collapse
        }
        #region Expand

        /// <summary></summary>
        [TestCaseAttribute("ExpandCollapsePattern.Expand.S.1.5",
            TestSummary = "Verify that calling Expand() on a collapsed element expands the element",
            Priority = TestPriorities.Pri0,
            Status = TestStatus.Works,
            Author = "Microsoft Corp.",
            TestCaseType = TestCaseType.Events, EventTested = "AutomationPropertyChangedEventHandler(ExpandCollapsePattern.ExpandCollapseStateProperty)",
            Description = new string[] {
				"Precondition: Verify that this is not a leaf node",
                "Step: Call Collapse()", 
                "Step: Call Collapse() twice in case of ExpandCollapseState = PartialExpanded", 
                "Step: Verify that the ExpandCollapseState = Collapsed", 
                "Step: Add StatePropertyChange event", 
                "Step: Call Expand()",
                "Step: Wait for events",
                "Step: Verify that the ExpandCollapseState != Collapsed", 
                "Step: Verify that the StatePropertyChange event was fired",
        })]
        public void TestExpandS15(TestCaseAttribute checkType)
        {
            HeaderComment(checkType);
            //"Precondition: Verify that this is not a leaf node",
            TSC_VerifyProperty(pattern_getExpandCollapseState, ExpandCollapseState.LeafNode, false, ExpandCollapsePattern.ExpandCollapseStateProperty.ToString(), CheckType.IncorrectElementConfiguration);

            //"Step: Call Expand()",
            TS_Collapse(null, CheckType.IncorrectElementConfiguration);

            //"Step: Call Expand() twice in case of ExpandCollapseState = PartialExpanded",
            TS_Collapse(null, CheckType.IncorrectElementConfiguration);

            // "Step: Verify that the ExpandCollapseState = Collapsed",
            TS_VerifyState(new object[] { ExpandCollapseState.LeafNode, ExpandCollapseState.Collapsed }, true, CheckType.Verification);

            //"Step: Add StatePropertyChange event",
            TSC_AddPropertyChangedListener(m_le, TreeScope.Element, new AutomationProperty[] { ExpandCollapsePattern.ExpandCollapseStateProperty }, CheckType.Verification);

            //"Step: Call Expand() and verify that Expand() returns true",
            TS_Expand(null, CheckType.Verification);

            // Wait for events
            TSC_WaitForEvents(1);

            //"Step: Verify that the ExpandCollapseState != Collapsed",
            TS_VerifyState(new object[] { ExpandCollapseState.Collapsed }, false, CheckType.Verification);

            //"Step: Verify that the StatePropertyChange event was fired"
            TSC_VerifyPropertyChangedListener(m_le, new EventFired[] { EventFired.True }, new AutomationProperty[] { ExpandCollapsePattern.ExpandCollapseStateProperty }, CheckType.Verification);

        }

        /// -------------------------------------------------------------------
        /// <summary>
        /// Determine what will expand the element, and then expand it using
        /// the keystroke
        /// </summary>
        /// -------------------------------------------------------------------
        private void TS_ExpandUsingMouseLeftClick(AutomationElement element, CheckType checkType)
        {
            AutomationElement dropDown;
            switch (m_le.Current.ControlType.ProgrammaticName)
            {
                case "ControlType.ComboBox":
                    {
                        // Assume combo boxes have one and only one drop down button
                        // at the child level of the combo box
                        AutomationElementCollection buttons = element.FindAll(TreeScope.Children, new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Button));
                        if (buttons.Count != 1)
                            ThrowMe(checkType, "There are two buttons, not sure which one will expand the combo box");

                        dropDown = buttons[0];

                        break;
                    }
                default:
                    {
                        dropDown = element;
                        break;
                    }
            }

            TS_SendLeftMouseClick(dropDown, CheckType.IncorrectElementConfiguration);

            // m_TestStep++ this is done in TS_SendLeftMouseClick above, so don't 
            // duplicate the addition here.
        }

 
        #region Test Lib

        #endregion Test Lib

        #endregion Expand

        #region Collapse

        ///<summary>TestCase: ExpandCollapsePattern.Collapse.S.2.4</summary>
        [TestCaseAttribute("ExpandCollapsePattern.Collapse.S.2.4",
            TestSummary = "Verify that after calling Collapse() on an expanded element, then element is not Expanded",
            Priority = TestPriorities.Pri0,
            Author = "Microsoft Corp.",
            Status = TestStatus.Works,
            TestCaseType = TestCaseType.Events, 
            EventTested = "AutomationPropertyChangedEventHandler(ExpandCollapsePattern.ExpandCollapseStateProperty)",
            Description = new string[] {
                "Precondition: ExpandCollapseState != LeafNode",
                "Step: Expand all elements",
                "Step: Add PropertyChange event for ExpandCollapseStateProperty for element",
                "Step: Collapse all elements",
                "Step: Wait for events to fire",
                "Step: Verify that the ExpandCollapseStateProperty != Expanded", 
                "Step: Verify that ExpandCollapseStateProperty PropertyChange events was fired for the element"
		}
        )]
        public void TestCollapseS24(TestCaseAttribute checkType)
        {
            HeaderComment(checkType);

            TS_VerifyState(new object[] { ExpandCollapseState.LeafNode }, false, CheckType.IncorrectElementConfiguration);

            //"Step: Expand all elements",
            TS_ExpandIfCollapsed(m_le, CheckType.Verification);

            //"Step: Add PropertyChange event for StateProperty for all elements include itself and those below the AutomationElement",
            TSC_AddPropertyChangedListener(m_le, TreeScope.Element, new AutomationProperty[] { ExpandCollapsePattern.ExpandCollapseStateProperty }, CheckType.Verification);

            //"Step: Collapse all elements",
            TS_Collapse(null, CheckType.Verification);

            // 11 Wait for events
            TSC_WaitForEvents(1);

            //"Step: Verify that the ExpandCollapseState = Collapsed",
            TS_VerifyState(new object[] { ExpandCollapseState.Expanded }, false, CheckType.Verification);

            //"Step: Verify that only one PropertyChange event for StateProperty is fired for the element",
            TSC_VerifyPropertyChangedListener(m_le, new EventFired[] { EventFired.True }, new AutomationProperty[] { ExpandCollapsePattern.ExpandCollapseStateProperty }, CheckType.Verification);
        }

        ///<summary>Expand if it is collapsed</summary>
        private void TS_ExpandIfCollapsed(AutomationElement m_le, CheckType checkType)
        {
            if (((ExpandCollapsePattern)m_le.GetCurrentPattern(ExpandCollapsePattern.Pattern)).Current.ExpandCollapseState == ExpandCollapseState.Collapsed)
                TS_Expand(null, checkType);
            else
                m_TestStep++;
        }

        private void TS_CollapseIfExpanded(AutomationElement m_le, CheckType checkType)
        {
            if (((ExpandCollapsePattern)m_le.GetCurrentPattern(ExpandCollapsePattern.Pattern)).Current.ExpandCollapseState == ExpandCollapseState.Expanded)
                TS_Collapse(null, checkType);
            else
                m_TestStep++;
        }

        ///<summary>TestCase: ExpandCollapsePattern.Collapse.S.2.5</summary>
        [TestCaseAttribute("ExpandCollapsePattern.Collapse.S.2.5",
            TestSummary = "Verify that after calling Collapse() on a PartialExpanded element, the element is collapsed",
            Priority = TestPriorities.Pri0,
            Status = TestStatus.Works,
            Author = "Microsoft Corp.",
            TestCaseType = TestCaseType.Events, 
            EventTested = "AutomationPropertyChangedEventHandler(ExpandCollapsePattern.ExpandCollapseStateProperty)",
            Description = new string[] {
                "Precondition: ExpandCollapseState != LeafNode",
                "Step: Call Collapse()",
                "Step: Call Collapse() again in the case ExpandCollapseState = PartialExpanded",
                "Step: Verify that the ExpandCollapseState = Collapse",
                "Step: Call Expand()",
                "Precondition: ExpandCollapseState = PartialExpanded",
                "Step: Add PropertyChange event for StateProperty",
                "Step: Call Collapse() and verify that Collapse() returns true",
                "Step: Wait for events to fire",
                "Step: Verify that ExpandCollapseState = Collapse",
                "Step: Verify that PropertyChange event for StateProperty is fired", 
		})]
        public void TestCollapseS25(TestCaseAttribute checkType)
        {
            HeaderComment(checkType);

            TS_VerifyState(new object[] { ExpandCollapseState.LeafNode }, false, CheckType.IncorrectElementConfiguration);

            // "Step: Call Collapse()",
            TS_CollapseIfExpanded(m_le, CheckType.Verification);

            // "Step: Call Collapse() again in the case ExpandCollapseState == PartialExpanded",
            TS_Collapse(null, CheckType.Verification);

            // "Step: Verify that the ExpandCollapseState == Collapse",
            TS_VerifyState(new object[] { ExpandCollapseState.Collapsed }, true, CheckType.Verification);

            // "Step: Call Expand()",
            TS_Expand(null, CheckType.Verification);

            // "Precondition: ExpandCollapseState == PartialExpanded",
            TS_VerifyState(new object[] { ExpandCollapseState.PartiallyExpanded }, true, CheckType.IncorrectElementConfiguration);

            // "Step: Add PropertyChange event for StateProperty",
            TSC_AddPropertyChangedListener(m_le, TreeScope.Element, new AutomationProperty[] { ExpandCollapsePattern.ExpandCollapseStateProperty }, CheckType.Verification);

            // "Step: Call Collapse() and verify that Collapse() returns true",
            TS_Collapse(null, CheckType.Verification);

            // Wait for events
            TSC_WaitForEvents(1);

            // "Step: Verify that ExpandCollapseState == Collapse",
            TS_VerifyState(new object[] { ExpandCollapseState.Collapsed }, true, CheckType.IncorrectElementConfiguration);

            // "Step: Verify that PropertyChange event for StateProperty is fired",
            TSC_VerifyPropertyChangedListener(m_le, new EventFired[] { EventFired.True }, new AutomationProperty[] { ExpandCollapsePattern.ExpandCollapseStateProperty }, CheckType.Verification);
        }


        ///<summary>TestCase: ExpandCollapsePattern.Collapse.S.2.5.MouseClick</summary>
        [TestCaseAttribute("ExpandCollapsePattern.Collapse.S.2.5.MouseClick",
            TestSummary = "Verify that after mouse clicking on a non collapsed element, the element is collapsed",
            Priority = TestPriorities.Pri0,
            Status = TestStatus.Problem,
            ProblemDescription="Cannot tell reliably where user can click to collapse",
            Author = "Microsoft Corp.",
            Client = Client.ATG,
            TestCaseType = TestCaseType.Events | TestCaseType.Scenario, /* Not everything uses mouse click to expand, so needs to be called from scenario */
            EventTested = "AutomationPropertyChangedEventHandler(ExpandCollapsePattern.ExpandCollapseStateProperty)",
            Description = new string[] {
                "Precondition: ExpandCollapseState != LeafNode",
                "Step: Call Collapse()",
                "Step: Call Collapse() again in the case ExpandCollapseState = PartialExpanded",
                "Step: Verify that the ExpandCollapseState = Collapse",
                "Step: Call Expand()",
                "Precondition: ExpandCollapseState != Collapsed",
                "Step: Add PropertyChange event for StateProperty",
                "Step: Mouse click on center of element",
                "Step: Wait for events to fire",
                "Step: Verify that ExpandCollapseState = Collapse",
                "Step: Verify that PropertyChange event for StateProperty is fired", 
		})]
        public void ExpandCollapsePatternCollapseS25MouseClick(TestCaseAttribute checkType)
        {
            HeaderComment(checkType);

            if (m_le.Current.ControlType == ControlType.TreeItem)
                ThrowMe(CheckType.IncorrectElementConfiguration, "TreeItems no not report a correct location to click on to expand, but click only exposes the edit control of the tree item");

            TS_VerifyState(new object[] { ExpandCollapseState.LeafNode }, false, CheckType.IncorrectElementConfiguration);

            // "Step: Call Collapse()",
            TS_Collapse(null, CheckType.Verification);

            // "Step: Call Collapse() again in the case ExpandCollapseState == PartialExpanded",
            TS_Collapse(null, CheckType.Verification);

            // "Step: Verify that the ExpandCollapseState == Collapse",
            TS_VerifyState(new object[] { ExpandCollapseState.Collapsed }, true, CheckType.Verification);

            // "Step: Call Expand()",
            TS_Expand(null, CheckType.Verification);

            // "Precondition: ExpandCollapseState != Collapsed",
            TS_VerifyState(new object[] { ExpandCollapseState.Collapsed }, false, CheckType.IncorrectElementConfiguration);

            // "Step: Add PropertyChange event for StateProperty",
            TSC_AddPropertyChangedListener(m_le, TreeScope.Element, new AutomationProperty[] { ExpandCollapsePattern.ExpandCollapseStateProperty }, CheckType.Verification);

            //"Step: Mouse click on center of element",
            TS_SendLeftMouseClick(m_le, CheckType.IncorrectElementConfiguration); 

            // Wait for events
            TSC_WaitForEvents(2);

            // "Step: Verify that ExpandCollapseState == Collapse",
            TS_VerifyState(new object[] { ExpandCollapseState.Collapsed }, true, CheckType.IncorrectElementConfiguration);

            // "Step: Verify that PropertyChange event for StateProperty is fired",
            TSC_VerifyPropertyChangedListener(m_le, new EventFired[] { EventFired.True }, new AutomationProperty[] { ExpandCollapsePattern.ExpandCollapseStateProperty }, CheckType.Verification);

        }
        #endregion Collapse

        void TS_VerifyState(object[] validStates, bool ShouldBe, CheckType ct)
        {
            string comment = "State for AutomationElement \"" + Library.GetUISpyLook(m_le) + "\" = \"" + pattern_getExpandCollapseState + "\"";

            switch (ShouldBe)
            {
                case true:
                    {
                        int i = Array.IndexOf(validStates, pattern_getExpandCollapseState);
                        if (Array.IndexOf(validStates, pattern_getExpandCollapseState) == -1)
                            ThrowMe(ct, comment);
                        Comment(comment);
                        break;
                    }
                case false:
                    {
                        if (Array.IndexOf(validStates, pattern_getExpandCollapseState) != -1)
                            ThrowMe(ct, comment);
                        Comment(comment);
                        break;
                    }
            }
            m_TestStep++;
        }

        void TS_Expand(Type expectedException, CheckType checkType)
        {
            try
            {
                pattern_Expand(expectedException, checkType);
            }
            catch (Exception error)
            {
                // Something is goofy with menus and they are not ready yet somethings due to timing
                if (error is InvalidOperationException && m_le.Current.ControlType == ControlType.MenuItem)
                    pattern_Expand(expectedException, checkType);
                else
                    throw error;
            }

            m_TestStep++;
        }

        void TS_Collapse(Type expectedException, CheckType ct)
        {
            pattern_Collapse(expectedException, ct);

            if (pattern_getExpandCollapseState == ExpandCollapseState.Expanded)
                ThrowMe(ct, "m_pattern.ExpandCollapseState == ExpandCollapseState.Expanded");

            Comment("After calling Collapse(), m_pattern.ExpandCollapseState = " + pattern_getExpandCollapseState);
            m_TestStep++;
        }

        void TS_ExpandAll(CheckType ct)
        {
            Comment("Expanding all elements below \"" + Library.GetUISpyLook(m_le) + "\"...");
            ExpandAll(m_le, ct);
            Comment("Expanded all elements below \"" + Library.GetUISpyLook(m_le) + "\"");
            m_TestStep++;
        }

        void ExpandAll(AutomationElement le, CheckType checkType)
        {
            if (le == null)
                return;

            string element = "\"" + Library.GetUISpyLook(le) + "\"";
            ExpandCollapsePattern ec = (ExpandCollapsePattern)le.GetCurrentPattern(ExpandCollapsePattern.Pattern);

            if (ec != null)
            {
                ec.Expand();
                if (ec.Current.ExpandCollapseState.Equals(ExpandCollapseState.PartiallyExpanded))
                {
                    ec.Expand();
                }

                if (ec.Current.ExpandCollapseState.Equals(ExpandCollapseState.Collapsed))
                    ThrowMe(checkType, "Current ExpandCollapseState for (" + element + ") = " + ec.Current.ExpandCollapseState + " after call to Expand()");

                Comment("ExpandCollapseState = Expanded for " + element + " after call to Expand()");

            }
        }

        #endregion Tests

        #region Step/Verification

        /// -------------------------------------------------------------------
        /// <summary>
        /// Check the IsOffScreen property, and that FromPoint(pt) == element where point 
        /// was obtained from TryClickablePoint
        /// </summary>
        /// -------------------------------------------------------------------
        private void TS_VerifyElementIsOnScreenAndNotOverlapped(AutomationElement element, CheckType checkType)
        {
            if (true == element.Current.IsOffscreen)
                ThrowMe(checkType, "IsOffScreen == true");

            Point pt = new Point();
            if (false == element.TryGetClickablePoint(out pt))
                ThrowMe(checkType, "TryGetClickablePoint() returned false");

            if (false == Automation.Compare(element, AutomationElement.FromPoint(pt)))
                ThrowMe(checkType, "Could not get element from pt{0}, could the element be covered by another window?", pt);

            m_TestStep++;
        }

        #endregion Step/Verification

    }
}