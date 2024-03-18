namespace Albo1125.Common
{
    //Thank you LMS for your help in writing this class.
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.InteropServices;

    using mscoree;

    [Serializable]
    public class AppDomainHelper
    {

        public delegate void CrossAppDomainCallDelegate(params object[] payload);
        public delegate object CrossAppDomainCallRetValueDelegate(params object[] payload);

        public static AppDomain GetAppDomainByName(string name)
        {
            return GetAppDomains().FirstOrDefault(appDomain => appDomain.FriendlyName == name);
        }

        public static void InvokeOnAppDomain(AppDomain appDomain, CrossAppDomainCallDelegate targetFunc, params object[] payload)
        {
            appDomain.SetData(appDomain.FriendlyName + "_payload", payload);
            appDomain.SetData(appDomain.FriendlyName + "_func", targetFunc);
            appDomain.DoCallBack(InvokedOnAppDomain);
        }

        public static T InvokeOnAppDomain<T>(AppDomain appDomain, CrossAppDomainCallRetValueDelegate targetFunc, params object[] payload)
        {
            appDomain.SetData(appDomain.FriendlyName + "_payload", payload);
            appDomain.SetData(appDomain.FriendlyName + "_func", targetFunc);
            appDomain.DoCallBack(InvokedOnAppDomainRet);

            return (T)Convert.ChangeType(appDomain.GetData("result"), typeof(T));
        }

        private static void InvokedOnAppDomain()
        {
            var currentAppDomain = AppDomain.CurrentDomain;
            var nameString = currentAppDomain.FriendlyName;

            // Grab payload.
            var payload = (object[])currentAppDomain.GetData(nameString + "_payload");
            var callDelegate = (CrossAppDomainCallDelegate)currentAppDomain.GetData(nameString + "_func");

            callDelegate.Invoke(payload);
        }

        private static void InvokedOnAppDomainRet()
        {
            var currentAppDomain = AppDomain.CurrentDomain;
            var friendlyNameString = currentAppDomain.FriendlyName;

            // Grab payload.
            var payload = (object[])currentAppDomain.GetData(friendlyNameString + "_payload");
            var retValueDelegate = (CrossAppDomainCallRetValueDelegate)currentAppDomain.GetData(friendlyNameString + "_func");

            var result = retValueDelegate.Invoke(payload);
            currentAppDomain.SetData("result", result);
        }

        public static IList<AppDomain> GetAppDomains()
        {
            IList<AppDomain> domains = new List<AppDomain>();
            var enumHandlePtr = IntPtr.Zero;
            ICorRuntimeHost host = new CorRuntimeHost();

            try
            {
                host.EnumDomains(out enumHandlePtr);
                while (true)
                {
                    host.NextDomain(enumHandlePtr, out var domain);
                    if (domain == null) break;
                    var appDomain = (AppDomain)domain;
                    domains.Add(appDomain);
                }

                return domains;
            }
            finally
            {
                host.CloseEnum(enumHandlePtr);
                Marshal.ReleaseComObject(host);
            }
        }
    }
}
