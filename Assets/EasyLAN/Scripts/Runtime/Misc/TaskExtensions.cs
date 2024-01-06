using System;
using System.Diagnostics;
using System.Threading.Tasks;
namespace AillieoUtils.EasyLAN
{
    public static class TaskExtensions
    {
        public static async void Await(this Task task)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            var stackTrace = new StackTrace();
            try {
                await task;
            }
            catch(Exception e)
            {
                string msg = $"{e}\n{stackTrace}";
                UnityEngine.Debug.LogError(msg);
                throw;
            }
#else
            await task;
#endif
        }
    }
}
