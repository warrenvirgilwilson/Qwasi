using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Qwasi.GraphUI
{
    public delegate void QGRefreshCallback(object caller);

    public static class QGUserInterface
    {
        private static Dictionary<object, int> s_ObjectLockDictionary = new Dictionary<object, int>();
        private static Dictionary<object, QGRefreshCallback> s_CallbackDictionary = new Dictionary<object, QGRefreshCallback>();

        public static void LockGlobalRefresh(object caller)
        {
            if (!s_ObjectLockDictionary.ContainsKey(caller))
                s_ObjectLockDictionary.Add(caller, 1);
            else
                s_ObjectLockDictionary[caller]++;
        }

        public static void UnlockGlobalRefresh(object caller)
        {
            if (!s_ObjectLockDictionary.ContainsKey(caller))
                throw new Exception("Cannot unlock global refresh for an object that hasn't registered a lock.");

            s_ObjectLockDictionary[caller]--;
            if (s_ObjectLockDictionary[caller] == 0)
                s_ObjectLockDictionary.Remove(caller);

            if (s_ObjectLockDictionary.Count == 0)
            {
                foreach (var kvp in s_CallbackDictionary)
                    kvp.Value(kvp.Key);

                s_CallbackDictionary.Clear();
            }
        }

        public static bool IsGlobalRefreshLocked => s_ObjectLockDictionary.Count > 0;

        public static void PostRefreshMethod(object caller, QGRefreshCallback callbackMethod)
        {
            if (!IsGlobalRefreshLocked)
                callbackMethod(caller);
            else if (!s_CallbackDictionary.ContainsKey(caller))
                s_CallbackDictionary.Add(caller, callbackMethod);
            else if (!s_CallbackDictionary[caller].GetInvocationList().Contains(callbackMethod))
                s_CallbackDictionary[caller] += callbackMethod;
        }
    }
}
