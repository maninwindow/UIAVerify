// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Permissive License.
// See http://www.microsoft.com/opensource/licenses.mspx#Ms-PL.
// All other rights reserved.

using System;
using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using Drawing = System.Drawing;
using System.Globalization;
using System.Threading;
using System.Windows.Automation;
using System.Windows;
using System.Runtime.InteropServices;
using System.Xml;
using System.Reflection;
using System.Diagnostics;
using System.Windows.Input;
using System.Text;
using Accessibility;
using MS.Win32;
using WUIATestLibrary.InternalTestHelper;
#if NATIVE_UIA
using QueryStringWrapper;
#endif
using System.IO;

// This suppresses warnings #'s not recognized by the compiler.
#pragma warning disable 1634, 1691

namespace InternalHelper.Tests
{
    using InternalHelper.Enumerations;
    using Microsoft.Test.UIAutomation.TestManager;
    using Microsoft.Test.UIAutomation.Interfaces;
    using Microsoft.Test.UIAutomation;
    using Microsoft.Test.UIAutomation.Core;
    using Microsoft.Test.UIAutomation.Tests.Controls;
    using Microsoft.Test.UIAutomation.Tests.Scenarios;
    using Microsoft.Test.UIAutomation.Tests.Patterns;
    using UIVerifyLogger = Microsoft.Test.UIAutomation.Logging.UIVerifyLogger;
    using InternalHelper.Tests;

    /// -----------------------------------------------------------------------
    /// <summary></summary>
    /// -----------------------------------------------------------------------
    public class TestObject
    {
        /// -------------------------------------------------------------------
        /// <summary>
        /// Sets the error flag to zero
        /// </summary>
        /// -------------------------------------------------------------------
        [DllImport("Kernel32.dll", CharSet = System.Runtime.InteropServices.CharSet.Auto, SetLastError = true)]
        internal static extern void SetLastError(int errorCode);

        const string THIS = "TestObject";
        const string FRAMEWORK_WINFORMS = "winform";
        const string FRAMEWORK_WIN32 = "win32";

        public const string tagBugs = "Bugs";
        public const string tagVersion = "Version";
        public const string tagTestMethod = "TestMethod";
        public const string tagTestAttribute = "TestAttributes";
        public const string tagMethod = "Method";
        public const string tagPriority = "Priority";
        public const string tagTestName = "TestName";
        public const string tagFailedStep = "FailedStep";
        public const string tagStepDescription = "Dscr";
        public const string tagStepIndex = "Step";
        public const string tagSteps = "Steps";
        public const string tagStep = "Step";
        public const string tagStepError = "StepError";
        public const string tagErrorDescription = "Error";
        public const string tagIssue = "Issue";
        public const string tagCommandLine = "ReproCommandLine";
        public const string tagPath = "ControlPath";
        public const string tagVerificationMethod = "VerificationMethod";
        public const string tagBugId = "BugNumber";
        public const string tagDate = "DateRan";
        public const string tagDataBase = "Database.Windows7";
        public const string tagClientSideProvider = "ClientSideProvider";
        public const string tagClientSideProviderInstalled = "Installed";

        #region Intialized in constructor

        /// -------------------------------------------------------------------
        /// <summary></summary>
        /// -------------------------------------------------------------------
        private string IDS_NO_EVENT_TESTING_ON_CONTROLVIEW = "Event testing turned off since event firing to non-ContentView elements is optional";

        /// -------------------------------------------------------------------
        /// <summary></summary>
        /// -------------------------------------------------------------------
        internal AutomationElement m_le;

        /// -------------------------------------------------------------------
        /// <summary>Loaded modules for the application/AutomationElement</summary>
        /// -------------------------------------------------------------------
        internal List<string> _loadedModules = null;

        /// -------------------------------------------------------------------
        /// <summary>Query string for every Automation Element</summary>
        /// -------------------------------------------------------------------
        string _queryString = string.Empty;

        /// -------------------------------------------------------------------
        /// <summary>Query string for every Automation Element</summary>
        /// -------------------------------------------------------------------
        string _globalizedQueryString = string.Empty;

        /// ------------------------------------------------------------------------
        /// <summary>
        /// This is passed by a test that knows how to drive the applications UI to 
        /// add/delete elements of the control.  ie. Adding an list element to a 
        /// listbox control.  The tests here have no idea on how to add an item 
        /// to the control, so it calls the test application at the appropriate
        /// time to add/delete the element.
        /// </summary>
        /// ------------------------------------------------------------------------
        internal IApplicationCommands _appCommands;

        /// -------------------------------------------------------------------
        /// <summary></summary>
        /// -------------------------------------------------------------------
        internal TestPriorities m_TestPriority;

        /// -------------------------------------------------------------------
        /// <summary></summary>
        /// -------------------------------------------------------------------
        internal string m_CachedControlType = String.Empty;

        /// -------------------------------------------------------------------
        /// <summary>Determines why type of test this is so that we can
        /// dump out the appropriate TestRuns() call for the logging </summary>
        /// -------------------------------------------------------------------
        internal enum TestCaseSampleType
        {
            /// ---------------------------------------------------------------
            /// <summary></summary>
            /// ---------------------------------------------------------------
            Scenario,

            /// ---------------------------------------------------------------
            /// <summary></summary>
            /// ---------------------------------------------------------------
            Pattern,

            /// ---------------------------------------------------------------
            /// <summary></summary>
            /// ---------------------------------------------------------------
            Control,

            /// ---------------------------------------------------------------
            /// <summary></summary>
            /// ---------------------------------------------------------------
            AutomationElement
        }

        /// -------------------------------------------------------------------
        /// <summary>Used to generate a sample call for the test</summary>
        /// -------------------------------------------------------------------
        internal TestCaseSampleType _testCaseSampleType;

        /// -------------------------------------------------------------------
        /// <summary>Used to generate a sample call for the test</summary>
        /// -------------------------------------------------------------------
        protected string _testSuite = string.Empty;

        #endregion Intialized in constructor

        #region member variables

        /// -------------------------------------------------------------------
        /// <summary>
        /// Current test object
        /// </summary>
        /// -------------------------------------------------------------------
        static TestObject m_testObject = null;

        /// -------------------------------------------------------------------
        /// <summary>
        /// XML code tag
        /// </summary>
        /// -------------------------------------------------------------------
        const string TAG_TESTS = @"<Tests/>";

        /// -------------------------------------------------------------------
        /// <summary>
        /// Reference to the TestCaseAttribute for the specific test case running
        /// </summary>
        /// -------------------------------------------------------------------
        internal static TestCaseAttribute _testCaseAttribute;

        /// -------------------------------------------------------------------
        /// <summary>
        /// Flag to determine if we are calling cached or current properties/patterns
        /// </summary>
        /// -------------------------------------------------------------------
        internal bool m_useCurrent = true;

        /// -------------------------------------------------------------------
        /// <summary>
        /// Very important variable that you need to increment for each step in the logging
        /// </summary>
        /// -------------------------------------------------------------------
        internal static int m_TestStep = 0;

        /// -------------------------------------------------------------------
        /// <summary>
        /// List of implemented patterns
        /// </summary>
        /// -------------------------------------------------------------------
        static internal Hashtable m_ImplementedPatterns = new Hashtable();

        /// -------------------------------------------------------------------
        /// <summary>
        /// List of available properties
        /// </summary>
        /// -------------------------------------------------------------------
        internal System.Guid[] m_properties = null;

        /// -------------------------------------------------------------------
        /// <summary>
        /// Current patterns
        /// </summary>
        /// -------------------------------------------------------------------
        internal AutomationPattern[] m_patterns = null;

        /// -------------------------------------------------------------------
        /// <summary>
        /// Flag to test of events
        /// </summary>
        /// -------------------------------------------------------------------
        static internal bool _testEvents = true;

        /// -------------------------------------------------------------------
        /// <summary>
        /// Flag to preserve existing content for controls that support
        /// TextPattern and ValuePattern. (True = preserve existing content)
        /// </summary>
        /// -------------------------------------------------------------------
        static internal bool _noClobber = false;

        /// -------------------------------------------------------------------
        /// <summary>
        /// The type of control
        /// </summary>
        /// -------------------------------------------------------------------
        internal TypeOfControl m_TypeOfControl;

        /// -------------------------------------------------------------------
        /// <summary>
        /// The type of pattern
        /// </summary>
        /// -------------------------------------------------------------------
        internal TypeOfPattern m_TypeOfPattern;

        /// -------------------------------------------------------------------
        /// <summary>
        /// Error string for the tests constructors
        /// </summary>
        /// -------------------------------------------------------------------
        internal const string ERROR_COULD_NOT_GET_PATTERN = "Pattern not supported";

        ///// -------------------------------------------------------------------
        ///// <summary>
        ///// Legacy string for documentation
        ///// </summary>
        ///// -------------------------------------------------------------------
        //internal string m_persistentId;

        /// -------------------------------------------------------------------
        /// <summary>
        /// Events that were fired
        /// </summary>
        /// -------------------------------------------------------------------
        ArrayList m_FocusEvents;

        /// -------------------------------------------------------------------
        /// <summary>
        /// This is the list of files that will be needed for submitting bugs to 
        /// Product Studio
        /// </summary>
        /// -------------------------------------------------------------------
        internal static ArrayList PSFileList = new ArrayList();

        /// ------------------------------------------------------------------------
        /// <summary>
        /// Cached interface to applications structure change
        /// </summary>
        /// ------------------------------------------------------------------------
        internal IWUIStructureChange _structChange = null;

        /// -------------------------------------------------------------------
        /// <summary>
        /// Flag set by the caller to cancel after the current test it ran
        /// </summary>
        /// -------------------------------------------------------------------
        static bool _cancelRun = false;

        #endregion member variables

        /// -------------------------------------------------------------------
        /// <summary>
        /// Run a specific tests
        /// </summary>
        /// -------------------------------------------------------------------
        internal bool InvokeTest(string testSuite, string testName, object arguments)
        {

            Type type = Type.GetType(testSuite);

            if (type == null)
                throw new Exception("Type.GetType(" + testSuite + ") failed.");

            foreach (MethodInfo method in type.GetMethods())
            {
                foreach (Attribute attr in method.GetCustomAttributes(true))
                {
                    if (attr is TestCaseAttribute)
                    {
                        TestCaseAttribute testAttribute = (TestCaseAttribute)attr;

                        if (testName.ToLower(CultureInfo.CurrentCulture).Equals(((TestCaseAttribute)(attr)).TestName.ToLower(CultureInfo.CurrentCulture)))
                        {
                            return invokeTest(m_le, method, testAttribute, arguments);
                        }
                    }
                }
            }

            throw new Exception("Could not find test: " + testSuite.ToString() + "." + testName);
        }

        /// -------------------------------------------------------------------
        /// <summary>
        /// Run a specific suite of tests
        /// </summary>
        /// -------------------------------------------------------------------
        internal bool InvokeTests(string testSuite, TestCaseType testCaseType)
        {

            // Returned results from the tests
            bool passed = true;

            Type type = Type.GetType(testSuite);

            if (type == null)
                throw new Exception("Type.GetType(" + testSuite + ") failed.");

            foreach (MethodInfo method in type.GetMethods())
            {
                foreach (Attribute attr in method.GetCustomAttributes(true))
                {
                    if (attr is TestCaseAttribute)
                    {
                        TestCaseAttribute testAttribute = (TestCaseAttribute)attr;

                        // Run this if the priority and status are correct.
                        if (testAttribute.Status.Equals(TestStatus.Works) && ((testAttribute.TestCaseType & TestCaseType.Arguments) != TestCaseType.Arguments))
                        {
                            if (m_TestPriority.Equals(TestPriorities.PriAll) || ((int)testAttribute.Priority & (int)m_TestPriority) != 0)
                            {
                                if ((testCaseType & testAttribute.TestCaseType) == testCaseType)
                                {
                                    if (false == invokeTest(m_le, method, testAttribute, null))
                                        passed = false;

                                    // If caller wants to cancel running tests...
                                    if (TestObject._cancelRun == true)
                                    {
                                        TestObject._cancelRun = false;
                                        return passed;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return passed;
        }

        /// -------------------------------------------------------------------
        /// <summary>
        /// Run a specific suite of tests
        /// </summary>
        /// -------------------------------------------------------------------
        internal bool InvokeSceanario(string testSuite, string testName, object arguments)
        {
            Type type = Type.GetType(testSuite);

            if (type == null)
                throw new Exception("Type.GetType(" + testSuite + ") failed.");

            foreach (MethodInfo method in type.GetMethods())
            {
                foreach (Attribute attr in method.GetCustomAttributes(true))
                {
                    if (attr is TestCaseAttribute)
                    {
                        TestCaseAttribute testAttribute = (TestCaseAttribute)attr;

                        if (testName.ToLower(CultureInfo.CurrentCulture).Equals(((TestCaseAttribute)(attr)).TestName.ToLower(CultureInfo.CurrentCulture)))
                        {
                            return invokeScenario(m_le, method, testAttribute, arguments);
                        }
                    }
                }
            }
            throw new Exception("Could not find test: " + testSuite.ToString() + "." + testName);
        }

        string CreateExampleCall(TestCaseAttribute testCaseAttribute)
        {
            string exampleCode = string.Empty;

            switch (_testCaseSampleType)
            {
                case TestCaseSampleType.Scenario:
                    Comment("Sample Call:");
                    break;
                case TestCaseSampleType.Pattern:
                    exampleCode = "TestRuns.RunPatternTest(element, " + _testEvents.ToString().ToLower() + ", false, " + _testSuite + ".TestSuite, \"" + testCaseAttribute.TestName.Substring(testCaseAttribute.TestName.IndexOf(":") + 1) + "\", null, null);";
                    break;
                case TestCaseSampleType.Control:
                    exampleCode = "TestRuns.RunControlTest(element, " + _testEvents.ToString().ToLower() + ", " + _testSuite + ".TestSuite, \"" + testCaseAttribute.TestName.Substring(testCaseAttribute.TestName.IndexOf(":") + 1) + "\", null, null);";
                    break;
                case TestCaseSampleType.AutomationElement:
                    exampleCode = "TestRuns.RunAutomationElementTest(element, " + _testEvents.ToString().ToLower() + ", \"" + testCaseAttribute.TestName.Substring(testCaseAttribute.TestName.IndexOf(":") + 1) + "\", null, null);";
                    break;
            }
            return exampleCode;
        }
        /// -------------------------------------------------------------------
        /// <summary></summary>
        /// -------------------------------------------------------------------
        bool invokeScenario(AutomationElement m_le, MethodInfo method, TestCaseAttribute testCaseAttribute, object arguments)
        {

            bool passed = true;

            try
            {

                StartTestWrapper(testCaseAttribute, method);

                object[] test;

                // Pack the arguments if we have any
                if (arguments == null)
                    test = new object[] { testCaseAttribute };
                else
                    test = new object[] { testCaseAttribute, arguments };

                // Assume passing
                passed = true;

                method.Invoke(this, test);

                // Use this for development checks to see if the descriptions add up to the amount 
                // of steps we have taken.
#if CHECK_DESCRIPTION
                if (testCaseAttribute.Description.Length != m_TestStep)
                    throw new Exception("Steps count(" + m_TestStep + ") executed do not match of with the TestCaseAttribute.Description(" + testCaseAttribute.Description.Length + ")");
#endif
            }
            catch (Exception exception)
            {
                invokeExceptionHandler(m_le, ref passed, exception);
            }
            try
            {
                invokeTryHandler1();
            }
            catch (Exception exception)
            {
                invokeExceptionHandler2(ref passed, exception);
            }
            finally
            {
                invokeExceptionFinallyHandler(passed);
            }
            return passed;
        }

        /// -------------------------------------------------------------------
        /// <summary>Common exception handler used between the different methods 
        /// that invoke a test</summary>
        /// -------------------------------------------------------------------
        private static void invokeExceptionFinallyHandler(bool passed)
        {
            // Cleanup
            if (passed)
            {
                UIVerifyLogger.LogPass();
            }

            UIVerifyLogger.EndTest();
        }

        /// -------------------------------------------------------------------
        /// <summary>Common exception handler used between the different methods 
        /// that invoke a test</summary>
        /// -------------------------------------------------------------------
        private static void invokeExceptionHandler2(ref bool passed, Exception exception)
        {
            // General exception so fail
            passed = false;

            UIVerifyLogger.LogError(exception);
        }

        /// -------------------------------------------------------------------
        /// <summary>Common exception handler used between the different methods 
        /// that invoke a test</summary>
        /// -------------------------------------------------------------------
        private static void invokeTryHandler1()
        {
            EventObject.RemoveAllEventHandlers();
            Thread.Sleep(1);
        }

        /// -------------------------------------------------------------------
        /// <summary>Common exception handler used between the different methods 
        /// that invoke a test</summary>
        /// -------------------------------------------------------------------
        private static void invokeExceptionHandler(AutomationElement element, ref bool passed, Exception exception)
        {
            // Only fail on test and general exceptions
            if (exception.InnerException is TestErrorException)
            {
                passed = false;
            }
            else if (exception.InnerException is TestWarningException)
            {
                passed = true;
            }
            else if (exception.InnerException is IncorrectElementConfigurationForTestException)
            {
                passed = true;
            }
            else
            {
                // only fail on test errors or general excpetions
                passed = false;
            }

            if (passed == false)
            {
                string knownIssue;

                Exception realException = exception.InnerException == null ? exception : exception.InnerException;

                passed = BugLibrary.IsIssueKnown(realException, element, m_TestStep, out knownIssue);

                /// Add to the filter file in case so we have all repros(command lines) of this if it is not there
                BugLibrary.AddIssue(realException, element, m_TestStep);

                if (passed == true)
                {
                    string ni = "#### KNOWN ISSUE ###";
                    Comment(ni);
                    Comment(string.Format("{0}:  \"{1}\"", ni, knownIssue));
                    Comment(ni);
                }
                else
                {
                    UIVerifyLogger.LogError(exception);
                }
            }
        }

        /// -------------------------------------------------------------------
        /// <summary></summary>
        /// -------------------------------------------------------------------
        bool invokeTest(AutomationElement element, MethodInfo method, TestCaseAttribute testCaseAttribute, object arguments)
        {

            bool passed = true;

            object controlType = String.Empty;

            string name = String.Empty;

            try
            {

                // Wrap call since in the ExceptionNotFound tests, calling the properties throws an exception
                try
                {
                    controlType = m_le.Current.ControlType.ProgrammaticName;
                    if (controlType == null)
                        controlType = "* " + m_le.GetCurrentPropertyValue(AutomationElement.LocalizedControlTypeProperty);
                }
                catch (Exception exception)
                {
                    if (Library.IsCriticalException(exception))
                        throw;
                }

                testCaseAttribute.TestName = controlType + ":" + testCaseAttribute.TestName;

                StartTestWrapper(testCaseAttribute, method);

                object[] test;

                // Pack the arguments if we have any
                if (arguments == null)
                    test = new object[] { testCaseAttribute };
                else
                    test = new object[] { testCaseAttribute, arguments };

                // Assume passing
                passed = true;

                if (_appCommands != null)
                    _appCommands.TraceMethod("Run : " + testCaseAttribute.TestName);

                method.Invoke(this, test);

                // Use this for development checks to see if the descriptions add up to the amount 
                // of steps we have taken.
#if CHECK_DESCRIPTION
//                if (testCaseAttribute.Description.Length != m_TestStep)
//                    throw new Exception("Steps count(" + m_TestStep + ") executed do not match of with the TestCaseAttribute.Description(" + testCaseAttribute.Description.Length + ")");
#endif

            }
            catch (Exception exception)
            {
                invokeExceptionHandler(m_le, ref passed, exception);
            }
            try
            {
                invokeTryHandler1();
            }
            catch (Exception exception)
            {
                invokeExceptionHandler2(ref passed, exception);
            }
            finally
            {
                invokeExceptionFinallyHandler(passed);
            }

            return passed;
        }

        /// -------------------------------------------------------------------
        /// <summary>
        /// Flag set by the caller to cancel after the current test it ran
        /// </summary>
        /// -------------------------------------------------------------------
        internal static bool CancelRun
        {
            get { return TestObject._cancelRun; }
            set { TestObject._cancelRun = value; }
        }

        /// -------------------------------------------------------------------
        /// <summary>Wrapper the does UIVerifyLogger.StartTest and various up
        /// front maintenance work</summary>
        /// -------------------------------------------------------------------
        private void StartTestWrapper(TestCaseAttribute testCaseAttribute, MethodInfo methodInfo)
        {
            if (m_le != null)
            {
                testCaseAttribute.UISpyLookName = Library.GetUISpyLook(m_le);
                AutomationElementPathFromRoot apfr = new AutomationElementPathFromRoot(m_le, _queryString, _globalizedQueryString);
                if (apfr != null)
                {
                    UIVerifyLogger.StartTest(apfr.XmlNode, testCaseAttribute, methodInfo);
                }
                else
                {
                    UIVerifyLogger.StartTest(null, testCaseAttribute, methodInfo);
                }
            }
            else
            {
                UIVerifyLogger.StartTest(null, testCaseAttribute, methodInfo);
            }
        }

        /// -------------------------------------------------------------------
        /// <summary></summary>
        /// -------------------------------------------------------------------
        internal object GetPattern(AutomationElement element, bool m_useCurrent, AutomationPattern pattern)
        {

            object patternObj = null;

            if (m_useCurrent)
                patternObj = element.GetCurrentPattern(pattern);
            else
                patternObj = element.GetCachedPattern(pattern);

            if (patternObj == null)
                throw new Exception(Helpers.PatternNotSupported);

            return patternObj;
        }

        #region Navigation

        /// -------------------------------------------------------------------
        /// <summary></summary>
        /// -------------------------------------------------------------------
        internal AutomationElement ControlFirstChild(AutomationElement el)
        {
            return TreeWalker.ControlViewWalker.GetFirstChild(el);
        }

        /// -------------------------------------------------------------------
        /// <summary></summary>
        /// -------------------------------------------------------------------
        internal AutomationElement ControlLastChild(AutomationElement el)
        {
            return TreeWalker.ControlViewWalker.GetLastChild(el);
        }

        /// -------------------------------------------------------------------
        /// <summary></summary>
        /// -------------------------------------------------------------------
        internal AutomationElement ControlNextSibling(AutomationElement el)
        {
            return TreeWalker.ControlViewWalker.GetNextSibling(el);
        }

        /// -------------------------------------------------------------------
        /// <summary></summary>
        /// -------------------------------------------------------------------
        internal AutomationElement ControlParent(AutomationElement el)
        {
            return TreeWalker.ControlViewWalker.GetParent(el);
        }

        /// -------------------------------------------------------------------
        /// <summary></summary>
        /// -------------------------------------------------------------------
        internal AutomationElement ControlPreviousSibling(AutomationElement el)
        {
            return TreeWalker.ControlViewWalker.GetPreviousSibling(el);
        }

        #endregion Navigation

        /// -------------------------------------------------------------------
        /// <summary></summary>
        /// -------------------------------------------------------------------
        internal static AutomationElement FindAutomationElement(IntPtr hWnd, Condition condition)
        {
            //get the root logical element for the Avalon window
            AutomationElement el = AutomationElement.FromHandle(hWnd);

            if (el == null)
                throw new ArgumentException("FromHandle() returned null with supplied 'hWnd'");

            el = el.FindFirst(TreeScope.Element | TreeScope.Descendants, condition);
            if (el == null)
                throw new Exception("Could not find element based on the supplied handle and seach condition from the Element and it's Descendants");

            return el;
        }

        /// -------------------------------------------------------------------
        /// <summary></summary>
        /// -------------------------------------------------------------------
        internal static TestType GetTestType(string testSuite)
        {
            string pattern = testSuite.Substring(testSuite.LastIndexOf('.') + 1);

            return (TestType)Enum.Parse(typeof(TestType), pattern);
        }

        /// -------------------------------------------------------------------
        /// <summary></summary>
        /// -------------------------------------------------------------------
        internal static string GetTestType(AutomationElement element)
        {
            if (element.Current.ControlType == null)
                ThrowMe(CheckType.Verification, "Cannot determine which tests to run since the ControlType is not set");

            string cts = Helpers.GetProgrammaticName(element.Current.ControlType);

            return IDSStrings.IDS_NAMESPACE_UIVERIFY + "." + IDSStrings.IDS_NAMESPACE_CONTROL + "." + cts + "ControlTests";

        }

        #region DLLs
        [StructLayout(LayoutKind.Sequential)]
        internal struct MSG
        {
            /// ---------------------------------------------------------------
            /// <summary></summary>
            /// ---------------------------------------------------------------
            public IntPtr hwnd;

            /// ---------------------------------------------------------------
            /// <summary></summary>
            /// ---------------------------------------------------------------
            public int message;

            /// ---------------------------------------------------------------
            /// <summary></summary>
            /// ---------------------------------------------------------------
            public int wParam;

            /// ---------------------------------------------------------------
            /// <summary></summary>
            /// ---------------------------------------------------------------
            public int lParam;

            /// ---------------------------------------------------------------
            /// <summary></summary>
            /// ---------------------------------------------------------------
            public int time;

            /// ---------------------------------------------------------------
            /// <summary></summary>
            /// ---------------------------------------------------------------
            public int pt_x;

            /// ---------------------------------------------------------------
            /// <summary></summary>
            /// ---------------------------------------------------------------
            public int pt_y;
        }
        #endregion DLLs

        #region Logging

        /// -------------------------------------------------------------------
        /// <summary>
        /// Comment to write to the log file
        /// </summary>
        /// <param name="format">The format string</param>
        /// <param name="args">An array of objects to write using format.</param>
        /// -------------------------------------------------------------------
        static internal void Comment(string format, params object[] args)
        {
            // Remove formating chars if there is not formating args
            if (args.Length == 0)
                format = new StringBuilder(format).Replace('{', '[').Replace('}', ']').ToString();

            UIVerifyLogger.LogComment("{0}: {1}", m_TestStep, String.Format(format, args));
        }

        /// ---------------------------------------------------------------
        /// <summary></summary>
        /// ---------------------------------------------------------------
        internal static string TestCaseCurrentStep
        {
            get
            {
                if (_testCaseAttribute != null && m_TestStep < _testCaseAttribute.Description.GetLength(0))
                    return m_TestStep + "). " + _testCaseAttribute.Description[m_TestStep];
                else
                {
                    return string.Empty;
                }
            }
        }

        /// ---------------------------------------------------------------
        /// <summary></summary>
        /// ---------------------------------------------------------------
        static void LoadProxy(string proxySource, string proxyClass)
        {
            Assembly proxyAssembly = Assembly.LoadFile(proxySource);


            UIVerifyLogger.LogComment("Loading proxy " + proxyClass + " from " + proxySource);
            if (proxyAssembly == null)
            {
                throw new Exception("Assembly.LoadFile(" + proxySource + ") returned null");
            }

            Type proxyType = proxyAssembly.GetType(proxyClass);

            if (proxyType == null)
            {
                throw new Exception("Type.GetType(" + proxyClass + ") returned null");
            }

            MethodInfo registerMethodInfo = proxyType.GetMethod("Register");

            if (registerMethodInfo == null)
            {
                throw new Exception("MethodInfo.GetMethod('Register') returned null");
            }

            registerMethodInfo.Invoke(null, null);
        }

        /// ---------------------------------------------------------------
        /// <summary></summary>
        /// ---------------------------------------------------------------
        internal void InitTest(TestCaseAttribute attribute)
        {
            attribute.TestName = m_CachedControlType + ":" + attribute.TestName;
            m_TestStep = 0;
            _testCaseAttribute = attribute;
        }

        /// -------------------------------------------------------------------
        /// <summary>
        /// Used from within the test cases to document any issues
        /// </summary>
        /// -------------------------------------------------------------------
        internal void HeaderComment(TestCaseAttribute testCaseAttribute)
        {
            m_TestStep = 0;
            _testCaseAttribute = testCaseAttribute;

            // Clear the events from previous runs
            EventObject.EventList = new ArrayList();

            if ((testCaseAttribute.TestCaseType & TestCaseType.Scenario) == TestCaseType.Scenario)
            {
                if (testCaseAttribute.Client == Client.NoneSet)
                    ThrowMe(CheckType.Verification, "TestCaseAttribute.Client needs to be set in the tests TestCaseAttribute custom attribute heading before the test function");
                return;
            }
            else
            {
            }

            // Test to see if the element exists
            try
            {
                // Hack to see if it's here
                Rect ct = m_le.Current.BoundingRectangle;
            }
            catch (ElementNotAvailableException)
            {
                ThrowMe(CheckType.IncorrectElementConfiguration, "Element does not exists");
            }

            if (_testSuite == string.Empty)
                throw new ArgumentException("Need to set the _testSuite variable!!!!");

        }

        static internal void ThrowMe(CheckType checkType, string format, params object[] args)
        {
            ThrowMe(checkType, null, format, args);
        }

        /// ------------------------------------------------------------------------
        /// <summary>
        /// Method that is used to throw from the inherited test methods.
        /// </summary>
        /// ------------------------------------------------------------------------
        static internal void ThrowMe(CheckType checkType, Exception exceptionThrown, string format, params object[] args)
        {
            string er = "Step " + m_TestStep + " : " + String.Format(format, args);

            // Short curcuit documented bugs.
            // If the description starts with "BUG, then we know about this and throw a warning
            // instead for this bug and continue on.  This will need to be set in 
            // the TestCaseAttributes such as:
            //
            // Description = new string[] {
            //    "Step: Start Narrator",
            //    //------------------------------------------------------------
            //    "Step: Open the \"Background Message\" dialog",
            //    "BUG(32456): Verify no duplicate AccessKeys in any of the \"Background Message\" sub menu items",
            //    "Step: Close the \"Background Message\" dialog",
            //    "Step: Close Narrator",

            if (
                _testCaseAttribute != null &&               /* may notbe set if this is a pre test setup */
                _testCaseAttribute.Description != null &&
                m_TestStep < _testCaseAttribute.Description.Length &&
                _testCaseAttribute.Description[m_TestStep].StartsWith("BUG")
            )
            {
                checkType = CheckType.Warning;
                er = "\n\n$$$$$$$:" + _testCaseAttribute.Description[m_TestStep] + ":" + String.Format(format, args) + "\n\n";
            }
            else
            {
                er = "Step " + m_TestStep + " : " + String.Format(format, args);
            }

            Exception exception = null;

            switch (checkType)
            {
                case CheckType.IncorrectElementConfiguration:
                    exception = new IncorrectElementConfigurationForTestException(er, exceptionThrown);
                    break;

                case CheckType.Verification:
                    exception = new TestErrorException(er, exceptionThrown);
                    break;

                case CheckType.Warning:
                    Comment(er);
                    break;

                //case CheckType.KnownProductIssue:
                //    exception = new KnownProductIssueException(er, exceptionThrown);
                //    break;

            }

            if (exception != null)
            {
                throw exception;
            }
        }

        /// ------------------------------------------------------------------------
        /// <summary>
        /// Method that is used to throw from the inherited test methods.
        /// </summary>
        /// ------------------------------------------------------------------------
        static internal void ThrowMe(CheckType checkType)
        {
            switch (checkType)
            {
                case CheckType.IncorrectElementConfiguration:
                    throw new IncorrectElementConfigurationForTestException(TestCaseCurrentStep);

                case CheckType.Verification:
                    throw new TestErrorException(TestCaseCurrentStep);

                //case CheckType.KnownProductIssue:
                //    throw new KnownProductIssueException();
            }
        }

        /// ----------------------------------------------------------------------------
        /// <summary>
        /// Checks for a condition and logs an error if the condition is false
        /// </summary>
        /// <param name="checkType"></param>
        /// <param name="condition">True to prevent error message from being logged</param>
        /// <param name="testComment">Comment that will be logged before the test is performed.  Can be null.</param>
        /// <param name="errorComment">Error message to be displayed</param>
        /// <param name="args"></param>
        /// ----------------------------------------------------------------------------
        static internal void ThrowMeAssert(CheckType checkType, bool condition, string testComment, string errorComment)
        {
            if (!String.IsNullOrEmpty(testComment))
                Comment(testComment);

            if (condition == false)
            {
                ThrowMe(checkType, "###### Error" + errorComment);
            }
        }

        #endregion Logging

        /// -------------------------------------------------------------------
        /// <summary>
        /// Constructor
        /// </summary>
        /// -------------------------------------------------------------------
        public TestObject(AutomationElement element, string testSuite, TestPriorities priority, TypeOfControl typeOfControl, TypeOfPattern typeOfPattern, string dirResults, bool testEvents, IApplicationCommands commands)
        {
            m_le = element;
            _appCommands = commands;
            m_TestPriority = priority;
            m_TypeOfControl = typeOfControl;
            m_TypeOfPattern = typeOfPattern;
            _testEvents = testEvents;
            _testSuite = testSuite;
            m_TestStep = 0;
            _testCaseAttribute = null;
            TestObject.m_testObject = this;
            #if NATIVE_UIA
            GetGlobalizedQueryString(m_le);
            #endif
        }

        /// -------------------------------------------------------------------------
        /// <summary>
        /// Get the loaded modules for the process to which the Autmation Element
        /// belongs
        /// </summary>
        /// -------------------------------------------------------------------------
        internal void GetGlobalizedQueryString(AutomationElement element)
        {
        #if NATIVE_UIA
            string processName = string.Empty;
            if (element != null)
            {
                _loadedModules = new List<string>();
                int processId = element.Current.ProcessId;
                if (processId != 0)
                {
                    try
                    {
                        Process process = Process.GetProcessById(processId);
                        processName = process.ProcessName;

                        foreach (ProcessModule pm in process.Modules)
                        {
                            _loadedModules.Add(pm.FileName);
                        }
                    }
                    catch (System.ArgumentException)
                    {
                        // If the process does not exist there is not much to do.                       
                    }
                }
            }
            try
            {
                _queryString = QueryString.Generate(element.NativeElement, null, processName);
            }
            catch (Exception error)
            {
                // Don't return failure if we cannot get the query string
            }
            // Currently, globalize query string is not required
            //List<string> queryStrings = new List<string>();
            //List<string> ambiguousResources;

            //queryStrings.Add(_queryString);
            //try
            //{
                //if (_loadedModules.Count > 0)
                //{
                    //string[] globalizedQueryStrings = QueryString.Globalize(queryStrings, _loadedModules, out ambiguousResources);
                    //_globalizedQueryString = globalizedQueryStrings.GetLength(0) > 0 ? globalizedQueryStrings[0] : string.Empty;
                //}
            //}
            //catch (Exception error)
            //{
                // Don't return failure if we cannot globalize the query string
            //}
        #endif
        }

        /// ---------------------------------------------------------------
        /// <summary></summary>
        /// ---------------------------------------------------------------
        internal static object GetScenarioTestObject(string testSuite, AutomationElement element, bool testEvents, TestPriorities priority, IApplicationCommands commands)
        {

            TestType testType = GetTestType(testSuite);

            switch (testType)
            {

                case TestType.MenuScenarioTests:
                    return new MenuScenarioTests(element, testSuite, priority, TypeOfControl.UnknownControl, TypeOfPattern.Unknown, null, testEvents, commands);

                case TestType.NarratorScenarioTests:
                    return new NarratorScenarioTests(element, testSuite, priority, TypeOfControl.UnknownControl, TypeOfPattern.Unknown, null, testEvents, commands);

                case TestType.MsaaScenarioTests:
                    return new MsaaScenarioTests(element, testSuite, priority, TypeOfControl.UnknownControl, TypeOfPattern.Unknown, null, testEvents, commands);

                case TestType.TextScenarioTests:
                    return new TextScenarioTests(element, testSuite, priority, TypeOfControl.UnknownControl, TypeOfPattern.Unknown, null, testEvents, commands);

                case TestType.ScreenReaderScenarioTests:
                    return new ScreenReaderScenarioTests(element, testSuite, priority, TypeOfControl.UnknownControl, TypeOfPattern.Unknown, null, testEvents, commands);

                case TestType.TopLevelEventsScenarioTests:
                    return new TopLevelEventsScenarioTests(element, testSuite, priority, TypeOfControl.UnknownControl, TypeOfPattern.Unknown, null, testEvents, commands);

                case TestType.AvalonTextScenarioTests:
                    return new AvalonTextScenarioTests(element, testSuite, priority, TypeOfControl.UnknownControl, TypeOfPattern.Unknown, null, testEvents, commands);

                default:
                    throw new Exception("Unhandled TestType(" + testType + ") in GetScenarioTestObject()");
            }
        }

        /// ---------------------------------------------------------------
        /// <summary>
        /// Get test object associated with the testSuite namespace
        /// </summary>
        /// ---------------------------------------------------------------
        internal static object GetPatternTestObject(string testSuite, AutomationElement element, bool testEvents, TestPriorities priority, IApplicationCommands commands)
        {
            Type assemblyType = null;
            try
            {
                Assembly assembly = Assembly.GetExecutingAssembly();

                assemblyType = assembly.GetType(testSuite);
                if (assemblyType == null)
                    throw new Exception("Could not load type \"" + testSuite + "\"");

                return Activator.CreateInstance(assemblyType,
                    new object[] { element, priority, null, testEvents, TypeOfControl.UnknownControl, commands });
            }
            catch (Exception error)
            {
                string s = string.Format("Failed to Activator.CreateInstance({0}) because {1}", assemblyType.Name, error.InnerException.Message);
                throw new Exception(s, error.InnerException);
            }
        }

        /// ---------------------------------------------------------------
        /// <summary>Get the control test associated with the element's control type</summary>
        /// ---------------------------------------------------------------
        internal static object GetControlTestObject(AutomationElement element, bool testEvents, TestPriorities priority, IApplicationCommands commands)
        {
            ControlType ct = element.Current.ControlType;

            if (ct == null)
                throw new Exception("Could not get a ControlType for the element(" + Library.GetUISpyLook(element) + ") so could not select the correct test");

            Assembly assembly = Assembly.GetExecutingAssembly();

            string testSuite = "Microsoft.Test.UIAutomation.Tests.Controls." + Helpers.GetProgrammaticName(ct) + "ControlTests";
            Type assemblyType = assembly.GetType(testSuite);
            if (assemblyType == null)
                throw new Exception("Could not load type \"" + testSuite + "\"");

            return Activator.CreateInstance(assemblyType,
                new object[] { element, priority, null, testEvents, commands });

        }

        #region TestCode

        #region Event Testing

        /// ---------------------------------------------------------------
        /// <summary></summary>
        /// ---------------------------------------------------------------
        AutomationEvents m_AutomationEvents = null;

        /// ---------------------------------------------------------------
        /// <summary></summary>
        /// ---------------------------------------------------------------
        StructureChangedEvents m_StructureChangedEvents = null;

        /// ---------------------------------------------------------------
        /// <summary></summary>
        /// ---------------------------------------------------------------
        AutomationFocusChangedEvents m_AutomationFocusChangedEvents = null;

        /// ---------------------------------------------------------------
        /// <summary></summary>
        /// ---------------------------------------------------------------
        PropertyChangeEvents m_PropertyChangeEvents = null;

        /// -------------------------------------------------------------------
        /// <summary></summary>
        /// -------------------------------------------------------------------
        ManualResetEvent _gotNotifiedEvent = new ManualResetEvent(false);

        /// -------------------------------------------------------------------
        /// <summary>
        /// Method registered by AddEventHandler() as an event handler
        /// </summary>
        /// -------------------------------------------------------------------
        public virtual void OnEvent(object element, AutomationPropertyChangedEventArgs argument)
        {
            _gotNotifiedEvent.Set();
        }

        #region Misc


        /// -------------------------------------------------------------------
        /// <summary></summary>
        /// -------------------------------------------------------------------
        internal void TSC_WaitForEvents(int numberOfEvents)
        {
            TSC_WaitForEvents(numberOfEvents, 1000);
        }

        /// -------------------------------------------------------------------
        /// <summary>Purge the test event queue</summary>
        /// -------------------------------------------------------------------
        internal void RemoveAllEventsFired()
        {
            Comment("Purging stored test event queue");
            EventObject.RemoveAllEventsFired();
            Comment("Done purging stored test event queue");
        }

        /// -------------------------------------------------------------------
        /// <summary></summary>
        /// -------------------------------------------------------------------
        internal void TSC_WaitForEvents(int numberOfEvents, int milliSeconds)
        {
            if (_testEvents)
            {
                // TODO : should be a global parameter for wait time
                Comment("Waiting for " + numberOfEvents + " event(s)");
                EventObject.WaitForEvents(numberOfEvents, milliSeconds);
                Comment("End waiting for events");
            }
            else
            {
                Comment("Not testing for events");
            }

            m_TestStep++;
        }


        #endregion Misc

        #region AddEvents

        /// -------------------------------------------------------------------
        /// <summary></summary>
        /// -------------------------------------------------------------------
        internal ArrayList HelperGetContainersSelectableItems(AutomationElement selectionContainer)
        {
            ArrayList list = new ArrayList();

            // Win32 radio buttons do not have containers, so bail.
            if (selectionContainer != null)
            {

                PropertyCondition pc = new PropertyCondition(AutomationElement.IsSelectionItemPatternAvailableProperty, true);

                foreach (AutomationElement element in selectionContainer.FindAll(TreeScope.Subtree, pc))
                {
                    // Don't want to include any "Previous" in calendar controls
                    if (element.Current.AutomationId.IndexOf("Previous") == -1 && element.Current.AutomationId.IndexOf("Next") == -1)
                    {
                        list.Add(element);
                    }
                }
            }
            return list;
        }


        /// -------------------------------------------------------------------
        /// <summary>
        /// Obtain an IAccessible from an AutomationElement
        /// </summary>
        /// -------------------------------------------------------------------
        internal void TS_GetIAccessibleFromAutomationElement(AutomationElement element, out IAccessible accObject, CheckType checkType)
        {
            accObject = null;
            IntPtr ptr = Helpers.CastNativeWindowHandleToIntPtr(element);

            // Verify that this element has a hwnd associated with it
            if (ptr == IntPtr.Zero)
                ThrowMe(checkType, "Could not get an handle to the AutomationElement");

            int hr = Helpers.GetIAccessibleFromWindow(ptr, 0, ref accObject);
            if (NativeMethods.S_OK != hr)
                ThrowMe(checkType, "AccessibleObjectFromWindow failed(hr={0})", hr);

            m_TestStep++;
        }

        /// -------------------------------------------------------------------
        /// <summary></summary>
        /// -------------------------------------------------------------------
        internal void TSC_AddEventListener(AutomationElement element, AutomationEvent eventID, TreeScope treeScope, CheckType checkType)
        {
            if ((_testCaseAttribute.TestCaseType & TestCaseType.Events) != TestCaseType.Events || _testCaseAttribute.EventTested == null)
                ThrowMe(CheckType.Verification, "Need to flag this test case's TestCaseType to be composed of TestCaseType.Events");

            if (_testEvents)
            {
                if (m_AutomationEvents == null)
                    m_AutomationEvents = new AutomationEvents();

                m_AutomationEvents.AddEventHandler(eventID, Helpers.GetProgrammaticName(eventID), element, treeScope);
            }
            else
            {
                Comment("Event testing is not turned on");
            }

            Thread.Sleep(1);
            m_TestStep++;
        }

        /// <summary>
        /// Remove any existing events that have fired
        /// </summary>
        internal void TS_RemoveAllEventsFired()
        {
            EventObject.RemoveAllEventsFired();
            m_TestStep++;
        }

        /// -------------------------------------------------------------------
        /// <summary></summary>
        /// -------------------------------------------------------------------
        internal void TSC_AddPropertyChangedListener(AutomationElement element, TreeScope treeScope, AutomationProperty[] properties, CheckType checkType)
        {
            if ((_testCaseAttribute.TestCaseType & TestCaseType.Events) != TestCaseType.Events || _testCaseAttribute.EventTested == null)
                ThrowMe(CheckType.Verification, "Need to flag this test case's TestCaseType to be composed of TestCaseType.Events");

            if (_testEvents && element.Current.IsContentElement)
            {
                if (m_PropertyChangeEvents == null)
                    m_PropertyChangeEvents = new PropertyChangeEvents();

                Comment("Adding PropertyChangedListener (" + Library.GetUISpyLook(element));
                m_PropertyChangeEvents.AddEventHandler(element, treeScope, properties);
            }
            else
            {
                /* If the element's (ControlView == true && ContentView == false), events
                // are not required to be fired since the element has some other mechanism
                // through patterns on the elements containing element that does the same
                // characteristic.  Example, Lists ScrollBar does not have to fire a 
                // RangeValue change event since the ListView supports ScrollPattern and the 
                // user would set events on the ScrollPattern. */
                if (!element.Current.IsContentElement)
                {
                    Comment(IDS_NO_EVENT_TESTING_ON_CONTROLVIEW);
                }
                else
                {
                    Comment("Event testing is turned off");
                }
            }
            m_TestStep++;
        }

        /// -------------------------------------------------------------------
        /// <summary></summary>
        /// -------------------------------------------------------------------
        internal void TSC_AddFocusChangedListener(string eventParamName, CheckType checkType)
        {
            if ((_testCaseAttribute.TestCaseType & TestCaseType.Events) != TestCaseType.Events || _testCaseAttribute.EventTested == null)
                ThrowMe(CheckType.Verification, "Need to flag this test case's TestCaseType to be composed of TestCaseType.Events");

            if (_testEvents)
            {
                if (m_AutomationFocusChangedEvents == null)
                    m_AutomationFocusChangedEvents = new AutomationFocusChangedEvents();

                Comment("Adding FocusChangedListener");
                m_AutomationFocusChangedEvents.AddEventHandler();
            }
            else
            {
                Comment("Event testing is turned off");
            }
            // Allow the threading of UIA to do the focus change events 
            Thread.Sleep(100);
            m_TestStep++;
        }

        /// -------------------------------------------------------------------
        /// <summary></summary>
        /// -------------------------------------------------------------------
        internal void TS_VerifyBoundingRect(Rect oldRect, Rect currentRect, CheckType checkType)
        {
            if (oldRect != currentRect)
                ThrowMe(checkType, "Rectangles do not equal: oldRect(" + oldRect + "); currentRect(" + currentRect + ")");

            m_TestStep++;
        }

        /// -------------------------------------------------------------------
        /// <summary>
        /// Applictions need to support the callback to get specific information, 
        /// fire events, etc.
        /// </summary>
        /// -------------------------------------------------------------------
        internal void TS_SupportsApplicationCallback(CheckType checkType)
        {
            if (_appCommands == null)
                ThrowMe(checkType, "Does not support application callbacks");

            Comment("Appliction supports application callback");

            m_TestStep++;
        }

        /// -------------------------------------------------------------------
        /// <summary>
        /// Determine if the application supports causing structure events to
        /// happen
        /// </summary>
        /// -------------------------------------------------------------------
        internal void TS_SupportsStructureChangeEvents(CheckType checkType)
        {
            if (_appCommands == null)
                ThrowMe(checkType, "Does not support application callbacks");

            if (!_appCommands.SupportsIWUIStructureChange(m_le))
                ThrowMe(checkType, "Does not support testing StructureChangeEvents");

            Comment("Control supports testing StructureChangeEvents");

            m_TestStep++;
        }

        /// -------------------------------------------------------------------
        /// <summary>
        /// Determine if the application supports causing structure events to
        /// happen
        /// </summary>
        /// -------------------------------------------------------------------
        internal void TS_SupportsPropertyChangeEvents(CheckType checkType)
        {
            if (_appCommands == null)
                ThrowMe(checkType, "Does not support application callbacks");

            if (!_appCommands.SupportsIWUIPropertyChange(m_le))
                ThrowMe(checkType, "Does not support testing PropertyChange");

            Comment("Control supports testing PropertyChange");

            m_TestStep++;
        }

        /// -------------------------------------------------------------------
        /// <summary>
        /// Use the application's callback to reset the control to it's 
        /// original state.
        /// </summary>
        /// -------------------------------------------------------------------
        internal void TS_ResetControlToInitialState(AutomationElement element, CheckType checkType)
        {
            Comment("Reseting control to original state");

            if (_appCommands == null)
                ThrowMe(checkType, "Does not support application callbacks");

            if (_structChange == null)
                _structChange = _appCommands.GetIWUIStructureChange();

            _structChange.ResetControl(element);
            Comment("Control has been reset to original state");

            m_TestStep++;
        }

        /// -------------------------------------------------------------------
        /// <summary>
        /// 
        /// </summary>
        /// -------------------------------------------------------------------
        internal void TS_CausePropertyChange(AutomationElement element, AutomationProperty automationProperty, CheckType checkType)
        {
            if (_appCommands == null)
                ThrowMe(checkType, "Does not support application callbacks");

            IWUIPropertyChange pc = _appCommands.GetIWUIPropertyChange();

            pc.ChangeProperty(automationProperty, element.Current.AutomationId);

            Comment("Successfully called application to cause " + automationProperty.ProgrammaticName + " to occur");
            m_TestStep++;
        }

        /// -------------------------------------------------------------------
        /// <summary>Return whether this is a right to left layout</summary>
        /// -------------------------------------------------------------------
        internal bool IsRightToLeft(AutomationElement element)
        {
            // This may be a non hwnd'd automation element, find the parenting hwnd
            while ((element != null) && (element.Current.NativeWindowHandle == 0))
                element = TreeWalker.ControlViewWalker.GetParent(element);

            // This should never happen
            if (element == null)
                ThrowMe(CheckType.Verification, "Element should not be null");

            // This should never heppen either, but check it
            if (element == AutomationElement.RootElement)
                ThrowMe(CheckType.Verification, "Could not find a control with an hwnd");

            // returns whether it is right to left
            return (NativeMethods.GetWindowLong(
                new IntPtr(element.Current.NativeWindowHandle),
                NativeMethods.GWL_EXSTYLE) & NativeMethods.WS_EX_LAYOUTRTL)
                    == NativeMethods.WS_EX_LAYOUTRTL;
        }

        /// -------------------------------------------------------------------
        /// <summary></summary>
        /// -------------------------------------------------------------------
        internal void TS_AddLogicalTreeStructureListener(bool newInstance, AutomationElement element, TreeScope treeScope, CheckType checkType)
        {
            if ((_testCaseAttribute.TestCaseType & TestCaseType.Events) != TestCaseType.Events || _testCaseAttribute.EventTested == null)
                ThrowMe(CheckType.Verification, "Need to flag this test case's TestCaseType to be composed of TestCaseType.Events");

            if (_testEvents)
            {
                if (newInstance)
                    //if (m_StructureChangedEvents == null)
                    m_StructureChangedEvents = new StructureChangedEvents();

                Comment("Adding LogicalStructureListener");
                m_StructureChangedEvents.AddEventHandler(element, treeScope);
            }
            else
            {
                Comment("Event testing is turned off");
            }

            m_TestStep++;
        }

        /// ---------------------------------------------------------------
        /// <summary>
        /// Logs the current property value for diagnostics
        /// </summary>
        /// ---------------------------------------------------------------
        internal void TS_LogProperty(AutomationElement element, AutomationProperty automationProperty, CheckType checkType)
        {
            Comment(automationProperty.ProgrammaticName + " = " + element.GetCurrentPropertyValue(automationProperty));
            m_TestStep++;
        }

        /// ---------------------------------------------------------------
        /// <summary>
        /// Logs the number of descendants under the AutomationElement 
        /// for diagnostics
        /// </summary>
        /// ---------------------------------------------------------------
        internal void TS_LogDescendantsCount(AutomationElement element, CheckType checkType)
        {
            Trace.WriteLine("Element: " + element.Current.Name);
            Comment("AutomationElement has " + DescendantsCount(TreeWalker.RawViewWalker.GetFirstChild(element)) + " children");
            m_TestStep++;
        }

        /// ---------------------------------------------------------------
        /// <summary>
        /// HELPER: Recursive method used by TS_LogDescendantsCount
        /// </summary>
        /// ---------------------------------------------------------------
        int DescendantsCount(AutomationElement element)
        {

            int iCount = 0;
            if (element != null)
            {
                AutomationElement nextElement = element;
                iCount++;
                Trace.WriteLine("Element/ControlType: " + nextElement.Current.Name + ":" + nextElement.Current.ControlType.ProgrammaticName);
                iCount += DescendantsCount(TreeWalker.RawViewWalker.GetFirstChild(nextElement));

                while (null != (nextElement = TreeWalker.RawViewWalker.GetNextSibling(nextElement)))
                {
                    iCount++;
                    iCount += DescendantsCount(TreeWalker.RawViewWalker.GetFirstChild(nextElement));
                    Trace.WriteLine("Element/ControlType: " + nextElement.Current.Name + ":" + nextElement.Current.ControlType.ProgrammaticName);

                }

            }
            return iCount;
        }


        #endregion AddEvents

        #region Verify Events

        /// -------------------------------------------------------------------
        /// <summary>
        /// Verify that the event was fired
        /// </summary>        
        /// -------------------------------------------------------------------
        internal void TSC_VerifyEventListener(AutomationElement element, AutomationEvent eventId, EventFired shouldFire, CheckType checkType)
        {
            // Element == null means we are looking for WindowCloseEvent
            if (_testEvents && (element == null || element.Current.IsContentElement))
            {
                Comment("Start: Looking for event that might have been fired");

                EventFired ActualFired = m_AutomationEvents.WasEventFired(element, eventId);

                Comment("End : Looking for event that might have been fired");
                TestIfEventShouldFire(shouldFire, ActualFired, eventId, checkType);
            }
            else
            {
                Comment("Not testing for events");
            }

            m_TestStep++;
        }

        /// -------------------------------------------------------------------
        /// <summary>
        /// Verify that the amount of events fired is correct
        /// </summary>
        /// -------------------------------------------------------------------
        internal void TS_VerifyEventCount(AutomationElement element, int count, CheckType checkType)
        {

            Comment(string.Format("Expected \"{0}\" event to be fired", count));
            if (_testEvents && element.Current.IsContentElement)
            {
                int c = m_AutomationEvents.EventCount;

                if (c != count)
                    ThrowMe(checkType, "There were " + c + " events fired and expected " + count + " to be fired");
            }
            else
            {
                Comment("Not testing for events");
            }

            m_TestStep++;
        }

        /// -------------------------------------------------------------------
        /// <summary>
        /// Verify that the property changed
        /// </summary>
        /// -------------------------------------------------------------------
        internal void TS_VerifyLogicalStructureChangedEventArgs(AutomationElement element, EventFired shouldFire, StructureChangeType properties, CheckType checkType)
        {
            if (_testEvents)
            {
                Comment("Start: Looking for event that might have been fired");

                EventFired ActualFired = m_StructureChangedEvents.WasEventFired(element, properties);

                Comment("End : Looking for event that might have been fired");
                TestIfEventShouldFire(shouldFire, ActualFired, properties, checkType);
            }
            else
            {
                Comment("Not testing for events");
            }

            m_TestStep++;
        }

        /// -------------------------------------------------------------------
        /// <summary>
        /// Verify that the property changed
        /// </summary>
        /// -------------------------------------------------------------------
        internal void TSC_VerifyPropertyChangedListener(AutomationElement element, EventFired[] eventFired, AutomationProperty[] properties, CheckType checkType)
        {
            int counter = 0;

            for (int i = 0; i < properties.Length; i++)
            {
                Comment("Verifying property change \"{0}\" event firing was {1})", properties[i].ProgrammaticName, eventFired[i]);
            }

            foreach (AutomationProperty prop in properties)
                if (_testEvents && element.Current.IsContentElement)
                {
                    foreach (EventFired ShouldFire in eventFired)
                    {
                        if (!ShouldFire.Equals(EventFired.Undetermined))
                        {
                            Comment("Start: Looking for event that might have been fired");

                            EventFired ActualFired = m_PropertyChangeEvents.WasEventFired(element, properties[counter]);

                            Comment("End : Looking for event that might have been fired");
                            TestIfEventShouldFire(ShouldFire, ActualFired, properties[counter], checkType);
                        }
                        else
                        {
                            Comment("Test spec stated that firing of the event is " + ShouldFire.ToString());
                        }
                    }
                }
                else
                {
                    /* If the element's (ControlView == true && ContentView == false), events
                    // are not required to be fired since the element has some other mechanism
                    // through patterns on the elements containing element that does the same
                    // characteristic.  Example, Lists ScrollBar does not have to fire a 
                    // RangeValue change event since the ListView supports ScrollPattern and the 
                    // user would set events on the ScrollPattern. */
                    if (!element.Current.IsContentElement)
                    {
                        Comment(IDS_NO_EVENT_TESTING_ON_CONTROLVIEW);
                    }
                    else
                    {
                        Comment("Not testing for events");
                    }
                }
            m_TestStep++;
        }

        /// -------------------------------------------------------------------
        /// <summary>
        /// Verify focus changed
        /// </summary>
        /// -------------------------------------------------------------------
        internal void TSC_VerifyFocusedChangeEvent(AutomationElement element, EventFired shouldFire, string eventHandlerVar, CheckType checkType)
        {
            if (_testEvents)
            {
                Comment("Start: Looking for event that might have been fired");

                EventFired ActualFired = m_AutomationFocusChangedEvents.WasEventFired(element);

                Comment("End : Looking for event that might have been fired");
                TestIfEventShouldFire(shouldFire, ActualFired, null, checkType);
            }
            else
            {
                Comment("Not testing for events");
            }

            m_TestStep++;
        }

        /// -------------------------------------------------------------------
        /// <summary>
        /// Tests and displays the correct error/comment
        /// </summary>
        /// -------------------------------------------------------------------
        void TestIfEventShouldFire(EventFired shouldFire, EventFired actualFired, object eventId, CheckType checkType)
        {
            if (!shouldFire.Equals(EventFired.Undetermined))
            {
                string eventName;
                if (eventId == null)
                {
                    eventName = "Focus";
                }
                else
                {
                    // Property change event
                    eventName = ((AutomationIdentifier)(eventId)).ProgrammaticName;
                }

                if (!actualFired.Equals(shouldFire))
                {
                    if (actualFired.Equals(EventFired.False))
                    {
                        ThrowMe(checkType, eventName + " event was not fired and was expected to be fired");
                    }
                    else
                    {
                        // Was fired and should not have been
                        ThrowMe(checkType, eventName + " event did get fired and was not expected to be fired");
                    }
                }
                else
                {
                    if (actualFired.Equals(EventFired.False))
                    {
                        Comment(eventName + " event was not fired and was not expected to be fired");
                    }
                    else
                    {
                        Comment(eventName + " change event did get fired and was expected to be fired");
                    }
                }
            }
            else
            {
                if (actualFired.Equals(EventFired.False))
                    Comment("The test spec'd firing of " + eventId + " event is Undetermined. The event was not fired");
                else
                    Comment("The test spec'd firing of " + eventId + " event is Undetermined. The event was fired");
            }
        }


        #endregion Verify Events

        #endregion Event Testing

        #region TransformPattern

        /// -------------------------------------------------------------------
        /// <summary></summary>
        /// -------------------------------------------------------------------
        internal void TS_ResizeWindow(AutomationElement element, double width, double height, CheckType checkType)
        {
            TransformPattern transformPattern = (TransformPattern)m_le.GetCurrentPattern(TransformPattern.Pattern);
            if (transformPattern == null)
                ThrowMe(checkType, "transformPattern is not supported");

            transformPattern.Resize(width, height);
            m_TestStep++;

        }

        #endregion TransformPattern

        #region WindowPattern

        WindowPattern ObtainWindowPattern(AutomationElement element, CheckType checkType)
        {
            WindowPattern wp = element.GetCurrentPattern(WindowPattern.Pattern) as WindowPattern;

            if (wp == null)
                ThrowMe(checkType, "Element does not support WindowPattern");

            return wp;
        }

        /// -------------------------------------------------------------------
        /// <summary>
        /// Test that one can set the VisualState
        /// </summary>
        /// -------------------------------------------------------------------
        internal void TS_SetWindowVisualState(AutomationElement element, WindowVisualState wvs, CheckType checkType)
        {
            AutomationPropertyChangedEventHandler handler;

            WindowPattern wp = ObtainWindowPattern(m_le, checkType);

            if (wp.Current.WindowVisualState != wvs)
            {
                // Want to wait till the BoundingRectangle changes to determine if the window changed Visual State
                handler = new AutomationPropertyChangedEventHandler(OnEvent);
                Automation.AddAutomationPropertyChangedEventHandler(element, TreeScope.Element, handler, new AutomationProperty[] { AutomationElement.BoundingRectangleProperty });
                _gotNotifiedEvent.Reset();

                // Set the new visual state
                wp.SetWindowVisualState(wvs);

                // Wait up to 5 seconds or until we get the event.  If WaitOne returns false, the event was not fired.
                bool eventHappened = _gotNotifiedEvent.WaitOne(5000, true);

                // We don't need the event handler anymore, remove it
                Automation.RemoveAutomationPropertyChangedEventHandler(element, handler);

                // If the event did not happen, then log error
                if (!eventHappened)
                {
                    ThrowMe(checkType, "BoundingRectangle property change event was not fired and should have. WindowState is set to {0}", wp.Current.WindowVisualState);
                }
            }
            m_TestStep++;
        }

        // ----------------------------------------------------------------------------
        /// <summary>
        /// 
        /// </summary>
        /// <param name="element"></param>
        /// <param name="wvs"></param>
        /// <param name="checkType"></param>
        // ----------------------------------------------------------------------------
        internal void TS_VerifyWindowVisualState(AutomationElement element, WindowVisualState wvs, CheckType checkType)
        {
            WindowPattern wp = ObtainWindowPattern(m_le, checkType);

            if (wp.Current.WindowVisualState != wvs)
                ThrowMe(checkType, "Element's WindowVisualState(" + wp.Current.WindowVisualState + ") != " + wvs);

            m_TestStep++;
        }

        #endregion

        #region Invoke

        /// ---------------------------------------------------------------
        /// <summary>
        /// Some control types get dismissed when you perform SetFocus, so 
        /// don't run them on these control types.
        /// </summary>
        /// ---------------------------------------------------------------
        internal void TS_VerifySetFocusIsOK(AutomationElement element, CheckType checkType)
        {
            if (element.Current.ControlType == ControlType.MenuItem)
                ThrowMe(checkType, "This test does not support testing ControlType.MenuItem");
            m_TestStep++;
        }

        #endregion

        #region AutomationElement

        /// ---------------------------------------------------------------
        /// <summary>
        /// Call AutomationElement.FocusedElement and return the object as a ref
        /// </summary>
        /// ---------------------------------------------------------------
        internal void TS_FocusedElement(ref AutomationElement element, CheckType checkType)
        {
            element = AutomationElement.FocusedElement;
            m_TestStep++;
        }

        #endregion AutomationElement

        #region KeyboardInput

        /// ---------------------------------------------------------------
        /// <summary>
        /// Programmatically input sending keyboard strokes into the system.
        /// If "isSequential" = true, then keys are send character by character.
        /// Otherwise, keys are pressed together as a combination.
        /// </summary>
        /// -------------------------------------------------------------------
        internal void TS_PressKeys(bool isSequential, params Key[] keys)
        {
            if (isSequential)
            {
                foreach (Key key in keys)
                {
                    Comment("Pressing : " + key);
                    ATGTestInput.Input.SendKeyboardInput(key, true);
                    Comment("Releasing : " + key);
                    ATGTestInput.Input.SendKeyboardInput(key, false);
                    //200ms second sleep between each key
                    Thread.Sleep(200);
                }
            }
            //the keys are pressed together
            else
            {

                foreach (Key key in keys)
                {
                    Comment("Pressing : " + key);
                    ATGTestInput.Input.SendKeyboardInput(key, true);
                    Thread.Sleep(200);
                }
                foreach (Key key in keys)
                {
                    Comment("Releasing : " + key);
                    ATGTestInput.Input.SendKeyboardInput(key, false);
                    Thread.Sleep(200);
                }
            }
            m_TestStep++;
        }

        #endregion KeyboardInput

        #region MouseInput

        /// -------------------------------------------------------------------
        /// <summary>
        /// Send a left mouse click on the center of the AutomationElement
        /// </summary>
        /// -------------------------------------------------------------------
        internal void TS_SendLeftMouseClick(AutomationElement element, CheckType checkType)
        {
            Rect rc = element.Current.BoundingRectangle;

            if (rc == Rect.Empty)
                ThrowMe(checkType, "BoundingRectangle = Rect.Empty");

            Point pt = new Point(rc.Left + (rc.Right - rc.Left) / 2, rc.Top + (rc.Bottom - rc.Top) / 2);
            Comment("Sending mouse click to ({0}, {1})", pt.X, pt.Y);

            ATGTestInput.Input.MoveTo(pt);
            ATGTestInput.Input.SendMouseInput(0, 0, 0, ATGTestInput.SendMouseInputFlags.LeftDown);
            ATGTestInput.Input.SendMouseInput(0, 0, 0, ATGTestInput.SendMouseInputFlags.LeftUp);

            m_TestStep++;
        }

        #endregion MouseInput


        /// -------------------------------------------------------------------
        /// <summary></summary>
        /// -------------------------------------------------------------------
        internal void TSC_VerifyProperty(object actual, object expected, bool shouldBeEqual, AutomationIdentifier propName, CheckType checkType) { TSC_VerifyProperty(actual, expected, shouldBeEqual, Helpers.GetProgrammaticName(propName), checkType); }
        internal void TSC_VerifyPropertyEqual(object actual, object expected, AutomationIdentifier propName, CheckType checkType) { TSC_VerifyProperty(actual, expected, true, Helpers.GetProgrammaticName(propName), checkType); }
        internal void TSC_VerifyPropertyNotEqual(object actual, object expected, AutomationIdentifier propName, CheckType checkType) { TSC_VerifyProperty(actual, expected, false, Helpers.GetProgrammaticName(propName), checkType); }

        /// -------------------------------------------------------------------
        /// <summary></summary>
        /// -------------------------------------------------------------------
        internal void TSC_VerifyProperty(object actual, object expected, bool shouldBeEqual, string strPropName, CheckType checkType)
        {
            string shouldBeBuffer = shouldBeEqual == true ? "does" : "does not";

            string expectedBuffer;

            if (expected == null)
            {
                Comment("Verify that {0} for element ({2}) returns null", strPropName, shouldBeBuffer, Library.GetUISpyLook(m_le));
            }
            else
            {
                expectedBuffer = expected is string ? "\"" + expected.ToString() + "\"" : expected.ToString();
                Comment("Verify that {0} for element ({3}) {1} returns {2}", strPropName, shouldBeBuffer, expectedBuffer, Library.GetUISpyLook(m_le));
            }

            if (shouldBeEqual)
            {
                if (actual == null && expected == null)
                {
                }
                else if ((actual == null && expected != null) || (actual != null && expected == null))
                {
                    ThrowMe(checkType);
                }
                else
                {
                    if (!actual.Equals(expected))
                        ThrowMe(checkType);
                }
            }
            else // Should not be equal
            {
                if (actual == null && (expected == null))
                {
                    ThrowMe(checkType);
                }
                if (actual.Equals(expected))
                    ThrowMe(checkType);
            }

            string actualValue = actual == null ? "NULL" : actual.ToString();

            Comment("Element({0})'s property {1} is {2}", Library.GetUISpyLook(m_le), strPropName, actual);
            m_TestStep++;
        }

        /// -------------------------------------------------------------------
        /// <summary>
        /// Determines if an element, or one of it's children have a specific
        /// automation element set or not set according to "shouldBeEqual".
        /// </summary>
        /// -------------------------------------------------------------------
        internal void TS_VerifyOnElementOrChild(AutomationElement element, AutomationProperty property, object compareValue, bool shouldBeEqual, CheckType checkType)
        {
            AutomationElement el = element.FindFirst(TreeScope.Subtree, new PropertyCondition(property, compareValue));

            if (shouldBeEqual && (el == null))
                ThrowMe(checkType, "Could not find any element or child whos property is \"{0}\" is \"{1}\"", property.ProgrammaticName, compareValue.ToString());
            else if (!shouldBeEqual && (el != null))
                ThrowMe(checkType, "Found an element or child who's property \"{0}\" is \"{1}\"", property.ProgrammaticName, compareValue.ToString());

            m_TestStep++;
        }

        /// -------------------------------------------------------------------
        /// <summary>
        /// Calls FromPoint and increments test pointer
        /// </summary>
        /// -------------------------------------------------------------------
        internal void TS_FromPoint(ref AutomationElement element, Point pt, CheckType ct)
        {
            Comment("Calling FromPoint(" + pt + ")");
            element = AutomationElement.FromPoint(pt);
            Comment("FromPoint(" + pt + ") returned " + Library.GetUISpyLook(element));
            m_TestStep++;
        }

        /// -------------------------------------------------------------------
        /// <summary>
        /// Calls FromPoint and increments test pointer
        /// </summary>
        /// -------------------------------------------------------------------
        internal void TS_FromHandle(ref AutomationElement element, IntPtr handle, CheckType ct)
        {
            Comment("Calling FromHandle(" + handle + ")");
            element = AutomationElement.FromHandle(handle);
            Comment("FromHandle(" + handle + ") returned " + Library.GetUISpyLook(element));
            m_TestStep++;
        }

        /// -------------------------------------------------------------------
        /// <summary>
        /// Verify that the lement that has the focus is either the element pased as a parameter, 
        /// or a child of the element passed as a parameter.
        /// </summary>
        /// -------------------------------------------------------------------
        internal void TSC_VerifyElementOrChildHasFocus(AutomationElement element, string varElement, CheckType checkType)
        {
            AutomationElement aa = AutomationElement.FocusedElement;
            Comment(Library.GetUISpyLook(AutomationElement.FocusedElement) + " has the focus");

            for (AutomationElement le = AutomationElement.FocusedElement; Automation.Compare(element, le).Equals(false); le = TreeWalker.ControlViewWalker.GetParent(le))
            {
                if (Automation.Compare(le, AutomationElement.RootElement).Equals(true))
                    ThrowMe(checkType, "Element(" + Library.GetUISpyLook(AutomationElement.FocusedElement) + ") is not expected to have the focus");
            }

            m_TestStep++;
        }


        /// -------------------------------------------------------------------
        /// <summary>
        /// Used in the eventing to allow the process to release and let the 
        /// event occur.
        /// </summary>
        /// -------------------------------------------------------------------
        static System.Threading.ManualResetEvent m_NotifiedEvent = new System.Threading.ManualResetEvent(false);

        /// -------------------------------------------------------------------
        /// <summary></summary>
        /// -------------------------------------------------------------------
        void OnFocusChanged(object o, AutomationFocusChangedEventArgs e)
        {
            AutomationElement focusedElement = (AutomationElement)o;
            m_FocusEvents.Add(focusedElement);
            m_NotifiedEvent.Set();
        }

        /// -------------------------------------------------------------------
        /// <summary>
        /// Can we set focus to an automation element? Only one combination of
        /// IsKeyboardFocusable and IsOffscreen allows us to set focus.
        /// ?</summary>
        /// -------------------------------------------------------------------
        internal void pattern_IsFocusable(AutomationElement autoElement, CheckType checkType)
        {
            bool isKeyBoardFocusable = autoElement.Current.IsKeyboardFocusable;
            bool isOffScreen = autoElement.Current.IsOffscreen;

            Comment("AutomationElement.IsKeyboardFocusable = " + isKeyBoardFocusable + " and AutomationElement.IsOffscreen = " + isOffScreen);

            if ((isKeyBoardFocusable == false) || (isOffScreen == true))
                ThrowMe(checkType, "AutomationElement cannot receive focus.");
        }


        /// -------------------------------------------------------------------
        /// <summary></summary>
        /// -------------------------------------------------------------------
        internal void pattern_SetFocus(AutomationElement element, Type expectedException, CheckType checkType)
        {
            string call = "SetFocus()";
            try
            {
                Comment("Calling SetFocus(" + Library.GetUISpyLook(element) + ")");
                element.SetFocus();
                Thread.Sleep(1000);
            }
            catch (Exception actualException)
            {
                if (Library.IsCriticalException(actualException))
                    throw;


                TestException(expectedException, actualException, call, checkType);
                return;
            }
            TestNoException(expectedException, call, checkType);

        }

        /// -------------------------------------------------------------------
        /// <summary></summary>
        /// -------------------------------------------------------------------
        internal void TS_SetFocus(AutomationElement element, Type expectedException, CheckType checkType)
        {
            Comment(string.Format("Setting focus to {0}", Library.GetUISpyLook(element)));
            pattern_SetFocus(element, expectedException, checkType);
            m_TestStep++;
        }

        /// -------------------------------------------------------------------
        /// <summary>
        /// Test that the element's enable state is the same as 'isEnabled' argument
        /// </summary>
        /// -------------------------------------------------------------------
        internal void TS_ElementEnabled(AutomationElement element, bool isEnabled, CheckType checkType)
        {
            bool enabled;

            if (m_useCurrent)
            {
                enabled = (bool)element.GetCurrentPropertyValue(AutomationElement.IsEnabledProperty);
            }
            else
            {
                enabled = (bool)element.GetCachedPropertyValue(AutomationElement.IsEnabledProperty);
            }

            if (enabled != isEnabled)
            {
                string errorStr;
                // Adding switch instead of if statements since this is most likely
                // an enumeration instead of a t/f such as soft focus states to be add
                // later.
                switch (isEnabled)
                {
                    case true:
                        errorStr = "Element is not enabled and is expected to be enabled";
                        break;
                    case false:
                        errorStr = "Element is enabled and is expected not to be enabled";
                        break;
                    default:
                        throw new ArgumentException("Need to handle " + isEnabled);
                }
                ThrowMe(checkType, errorStr);
            }

            m_TestStep++;

        }

        /// -------------------------------------------------------------------
        /// <summary>
        /// Selected another element that can be found from the RootElement other than the element passed
        /// in as an argument.
        /// </summary>
        /// -------------------------------------------------------------------
        internal void TS_RandomSetFocusToOtherControl(AutomationElement element, CheckType checkType)
        {
            Condition cond = new PropertyCondition(AutomationElement.IsKeyboardFocusableProperty, true);

            AutomationElementCollection aec = AutomationElement.RootElement.FindAll(TreeScope.Children, cond);

            if (aec.Count < 1)
                ThrowMe(CheckType.Verification, "Could not find one element that could be selected!");

            foreach (AutomationElement ae in aec)
            {
                if (Automation.Compare(element, ae) != true)
                {
                    if (!DirectParent(ae, element))
                    {
                        //  ensure the top level window has a name and is keyboard focusable
                        if (ae.Current.IsKeyboardFocusable && !String.IsNullOrEmpty(ae.Current.Name))
                        {
                            TS_SetFocus(ae, null/*typeof(InvalidOperationException)*/, CheckType.Verification);

                            // NOTE: Don't increment m_TestStep++, TS_SetFocus will do that
                            return;
                        }
                    }
                }
            }
            ThrowMe(CheckType.Verification, "Could not find one element that could be selected!");
        }

        /// -------------------------------------------------------------------
        /// <summary></summary>
        /// -------------------------------------------------------------------
        internal void TS_GetOtherSelectionItem(AutomationElement thisOne, out AutomationElement other, bool shouldBeSelected, int tries, CheckType checkType)
        {
            Comment("Looking for something other than {0}", Library.GetUISpyLook(thisOne));
            string thisUISpyLook = Library.GetUISpyLook(thisOne);

            if (false == (bool)thisOne.GetCurrentPropertyValue(AutomationElement.IsSelectionItemPatternAvailableProperty))
                ThrowMe(checkType, "{0} does not support SelectionItemPattern", thisUISpyLook);

            AutomationElement selectionContainer = ((SelectionItemPattern)thisOne.GetCurrentPattern(SelectionItemPattern.Pattern)).Current.SelectionContainer;
            if (selectionContainer == null)
                ThrowMe(checkType, "{0} does not support SelectionItemPattern", thisUISpyLook);

            // Get all the elements desired
            AutomationElementCollection aec = selectionContainer.FindAll(TreeScope.Subtree, new AndCondition(
                new Condition[]{
                new PropertyCondition(AutomationElement.IsSelectionItemPatternAvailableProperty, true),
                new PropertyCondition(SelectionItemPattern.IsSelectedProperty, shouldBeSelected)
                }));

            int tryIndex = 0;
            do
            {
                if (tryIndex++ > tries)
                    ThrowMe(checkType, "Could not find other element in {0} ties", tries);

                other = aec[(int)Helpers.RandomValue(0, aec.Count - 1)];
                Comment("Found AutomationElement({0})", Library.GetUISpyLook(other));
            } while (Automation.Compare(other, thisOne).Equals(true));

            m_TestStep++;
        }

        /// -------------------------------------------------------------------
        /// <summary>
        /// Determines if the elements are direct anscestors
        /// </summary>
        /// -------------------------------------------------------------------
        bool DirectParent(AutomationElement older, AutomationElement younger)
        {
            while (younger != null)
            {
                if (Automation.Compare(older, younger) == true)
                {
                    return true;
                }
                else
                {
                    younger = TreeWalker.ControlViewWalker.GetParent(younger);
                }
            }
            return false;
        }

        /// -------------------------------------------------------------------
        /// <summary>
        /// Call SetFocus() and verify the it does get focus by verifying
        /// that the event does occur.  Even if you are not testing for events,
        /// we use the event handler to determine that the element got
        /// the focus.
        /// </summary>
        /// -------------------------------------------------------------------
        internal void TSC_SetFocusVerifyWithEvent(AutomationElement element, CheckType checkType)
        {
            AutomationFocusChangedEventHandler handler = null;
            bool fired = false;

            try
            {
                m_FocusEvents = new ArrayList();
                Comment("Adding AutomationFocusChangedEventHandler");
                handler = new AutomationFocusChangedEventHandler(OnFocusChanged);

                Automation.AddAutomationFocusChangedEventHandler(handler);

                Comment("Calling SetFocus() on " + Library.GetUISpyLook(element));

                // Set the notifier 
                m_NotifiedEvent.Reset();

                element.SetFocus();

                Comment("Wait 1 second for event to happen");
                m_NotifiedEvent.WaitOne(1000, false);
                Comment("Stopped waiting for event to happen");

                Comment("Calling RemoveAutomationFocusChangedEventHandler()");
                Automation.RemoveAutomationFocusChangedEventHandler(handler);
                Comment("Called RemoveAutomationFocusChangedEventHandler()");

                try
                {
                    // Lock this so we don't get anymore events fired.
#pragma warning suppress 6517
                    lock (this)
                    {
                        foreach (AutomationElement tempElement in m_FocusEvents)
                        {
                            AutomationElement el = tempElement;
                            Comment("History of events fired: " + el.Current.Name);

                            // Check to see if any of the elements are the element, or one of the children
                            while (!Automation.Compare(el, AutomationElement.RootElement) && !Automation.Compare(el, element))
                                el = TreeWalker.ControlViewWalker.GetParent(el);

                            if (Automation.Compare(el, element))
                            {
                                fired = true;
                                break;
                            }
                        }
                    }
                }
                finally
                {

                }
            }
            finally
            {

            }
            if (!fired)
                ThrowMe(CheckType.Verification, "Element or one of it's children did not fire the FocusChange event");

            m_TestStep++;
        }

        #region Properties

        /// -------------------------------------------------------------------
        /// <summary>
        /// Library function that will verify the value of a property passed in
        /// </summary>
        /// -------------------------------------------------------------------
        protected bool VerifyPropertyValue(AutomationProperty ap, object ExpectedValue)
        {
            return VerifyPropertyValue(m_le, ap, ExpectedValue);
        }

        /// -------------------------------------------------------------------
        /// <summary>
        /// Library function that will verify the value of a property passed in
        /// </summary>
        /// -------------------------------------------------------------------
        protected bool VerifyPropertyValue(AutomationElement element, AutomationProperty ap, object ExpectedValue)
        {
            string propName = Helpers.GetProgrammaticName(ap);
            Comment("Checking the property value of \"(" + Library.GetUISpyLook(element) + ")." + propName + "\" and expect it to be \"" + ExpectedValue + "\"");

            object ActualValue = element.GetCurrentPropertyValue(ap);

            Comment("Property value (" + Library.GetUISpyLook(element) + ")." + propName + " is \"" + ActualValue + "\"");
            if (!element.GetCurrentPropertyValue(ap).Equals(ExpectedValue))
                return false;
            else
                return true;
        }

        #endregion Properties

        #endregion TestCode

        /// -------------------------------------------------------------------
        /// <summary></summary>
        /// -------------------------------------------------------------------
        internal void TestException(Type expectedExceptionType, Exception actualException, string call, CheckType checkType, string bugId)
        {
            Type actualExceptionType = actualException.GetType();
            if (actualExceptionType == expectedExceptionType)
            {
                Comment(call + " threw " + actualException.GetType() + " as expected");
                return;
            }
            else
            {
                string strExpectedExceptionType = expectedExceptionType == null ? "none" : expectedExceptionType.ToString();
                ThrowMe(checkType, actualException, "The following expection was thrown incorrectly : " + actualExceptionType.FullName + " and expected: " + strExpectedExceptionType);
            }
        }

        /// -------------------------------------------------------------------------
        /// <summary>
        /// Called from the pattern_* methods
        /// </summary>
        /// -------------------------------------------------------------------------
        internal void TestException(Type expectedExceptionType, Exception actualException, string call, CheckType checkType)
        {
            TestException(expectedExceptionType, actualException, call, checkType, "");
        }

        /// -------------------------------------------------------------------------
        /// <summary>
        /// Called from the pattern_* methods, doesn't comment on success
        /// </summary>
        /// -------------------------------------------------------------------------
        internal void TestNoExceptionQuiet(Type exceptionExpected, string functionCall, CheckType checkType)
        {
            if (exceptionExpected != null)
                ThrowMe(checkType, functionCall + " did not throw an '" + exceptionExpected + "' expection as expected");

        }

        /// -------------------------------------------------------------------------
        /// <summary>
        /// Called from the pattern_* methods
        /// </summary>
        /// -------------------------------------------------------------------------
        internal void TestNoException(Type exceptionExpected, string functionCall, CheckType checkType)
        {
            TestNoExceptionQuiet(exceptionExpected, functionCall, checkType);

            Comment("Successfully called " + functionCall + " without an exception thrown");
        }

    }
}