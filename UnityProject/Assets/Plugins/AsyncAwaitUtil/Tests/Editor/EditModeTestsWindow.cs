
using Task = System.Threading.Tasks.Task;
#if UNITY_EDITOR
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using Debug = UnityEngine.Debug;

using UnityEditor;
using UnityEditor.VersionControl;

namespace UnityAsyncAwaitUtil
{
    /// <summary>
    /// Tests async/await/IEnumerator integration in Edit Mode.
    ///
    /// In Edit Mode, Unity does not automatically advance coroutines during
    /// each update, presumably because this happens in the Unity game loop.
    /// In Edit Mode, we solve this issue by calling `MoveNext` ourselves on
    /// each coroutine, in response to the `EditorApplication.update` event.
    /// (See `EditModeAsyncCoroutineRunner` class for the implementation.)
    ///
    /// It would be much nicer if these tests were implemented as standard/
    /// unit tests that could be run with the Unity Test Runner. However, as of
    /// Unity 2017.1.1f1, the version of NUnit shipped with Unity does not
    /// properly support for `async`/`await`.
    /// </summary>
    public class EditModeTestsWindow : EditorWindow
    {
        private TestButtonHandler _buttonHandler;

        [MenuItem("Window/AsyncAwaitUtil/EditModeTests")]
        public static void ShowWindow()
        {
            GetWindow<EditModeTestsWindow>();
        }

        public void Awake()
        {
            TestButtonHandler.Settings settings
                = new TestButtonHandler.Settings();

            settings.NumPerColumn = 4;
            settings.VerticalMargin = 10;
            settings.VerticalSpacing = 10;
            settings.HorizontalSpacing = 10;
            settings.HorizontalMargin = 10;
            settings.ButtonWidth = 200;
            settings.ButtonHeight = 30;

            _buttonHandler = new TestButtonHandler(settings);
        }

        public void OnGUI()
        {
            _buttonHandler.Restart();

            if (_buttonHandler.Display("Test await seconds"))
            {
               RunAwaitSecondsTestAsync().WrapErrors(); 
            }

            if (_buttonHandler.Display("Test return value"))
            {
                RunReturnValueTestAsync().WrapErrors();
            }

            if (_buttonHandler.Display("Test try-catch exception"))
            {
                RunTryCatchExceptionTestAsync().WrapErrors();
            }

        }

        async Task RunAwaitSecondsTestAsync()
        {
            Debug.Log("Waiting 1 second...");
            await new WaitForSeconds(1.0f);
            Debug.Log("Done!");
        }

        async Task RunReturnValueTestAsync()
        {
            Debug.Log("Waiting to get value...");
            var result = await GetValueExampleAsync();
            Debug.Log("Got value: " + result);
        }

        async Task<string> GetValueExampleAsync()
        {
            await new WaitForSeconds(1.0f);
            return "asdf";
        }

        async Task RunTryCatchExceptionTestAsync()
        {
            try
            {
                await NestedRunAsync();
            }
            catch (Exception e)
            {
                Debug.Log("Caught exception! " + e.Message);
            }
        }

        async Task NestedRunAsync()
        {
            await new WaitForSeconds(1);
            throw new Exception("foo");
        }

#if comment
        private IEnumerator Await(IEnumerator coroutine)
        {
            
        }

        private IEnumerator NullCoroutine()
        {
            yield return null;
        }

        [Test]
        public void NullCoroutineTest()
        {
            NullCoroutine().GetAwaiter().GetResult();
        }

        private IEnumerator WaitForSecondsRealtimeCoroutine()
        {
            yield return new WaitForSecondsRealtime(2.0f);
        }

        [Test]
        public void UniversalWaitForSecondsRealtimeTest()
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            stopwatch.Stop();
            Debug.LogFormat("stopwatch.Elapsed.Seconds: {0}", stopwatch.Elapsed.Seconds);
            Assert.IsTrue(stopwatch.Elapsed.Seconds >= 2.0f);
        }
#endif

    }

}
#endif
