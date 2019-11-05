using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEditor;
using UnityEngine;

namespace UnityAsyncAwaitUtil
{
    /// <summary>
    /// A better version of Unity's `WaitForSecondsRealtime` that works in both
    /// Edit Mode and Play Mode.
    ///
    /// Unity's `WaitForSecondsRealtime` does not work as expected in Edit Mode
    /// because it relies on `Time.realtimeSinceStartup`, which is not correctly
    /// updated in Edit Mode (for reasons I don't understand).
    /// </summary>
    public class SafeWaitForSeconds : CustomYieldInstruction
    {
        private Stopwatch _stopwatch;
        private float _secondsToWait;

        public SafeWaitForSeconds(float seconds)
        {
            _secondsToWait = seconds;
            _stopwatch = new Stopwatch();
            _stopwatch.Start();
        }

        public override bool keepWaiting
        {
            get
            {
                _stopwatch.Stop();
                if (_stopwatch.Elapsed.Seconds >= _secondsToWait)
                    return false;
                _stopwatch.Start();
                return true;
            }
        }
    }
}
