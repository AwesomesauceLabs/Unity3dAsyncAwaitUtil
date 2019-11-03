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
    /// Tests correct functioning async/await/IEnumerator integration in Edit
    /// Mode.
    ///
    /// Note 1:
    ///
    /// In Edit Mode, Unity does not automatically advance coroutines during each
    /// update, presumably because this happens in the Unity game loop.  In
    /// Edit Mode, we solve this issue by calling `MoveNext` ourselves
    /// on each coroutine, in response to the `EditorApplication.update`
    /// event. (See `EditModeAsyncCoroutineRunner` class for the
    /// implementation.)
    ///
    /// Note 2:
    ///
    /// As of Unity 2017.1.1f1, the version of NUnit shipped with
    /// Unity does not support annotation of `async` methods
    /// with `[Test]`. I worked around this issue by implementing an
    /// extra `Wrapper` method for each test.
    /// </summary>
    public class EditModeAsyncUtilTests
    {
        private IEnumerator NullCoroutine()
        {
            yield return null;
        }

        private async void AwaitNullCoroutineWrapper()
        {
            await NullCoroutine();
        }

        [Test]
        public void AwaitNullCoroutineTest()
        {
            Debug.Log("AwaitNullCoroutineTest()");
            AwaitNullCoroutineWrapper();
        }

        private IEnumerator WaitForSecondsRealtimeCoroutine()
        {
            yield return new WaitForSecondsRealtime(2.0f);
        }

        private async void AwaitWaitForSecondsRealtimeWrapper()
        {
            await WaitForSecondsRealtimeCoroutine();
        }

        [Test]
        public void AwaitWaitForSecondsRealtimeTest()
        {
            Debug.Log("AwaitWaitForSecondsRealtimeTest()");
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            AwaitWaitForSecondsRealtimeWrapper();
            stopwatch.Stop();
            Assert.IsTrue(stopwatch.Elapsed.Seconds >= 2.0f);
        }

    }

}
#endif
