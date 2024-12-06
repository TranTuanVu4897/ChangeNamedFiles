using System;

namespace ChangeNamedFiles.Util
{
    public static class Common
    {
        internal static void LogsInfo(string v, bool isError, Action<string, bool> updateLogs)
        {
            updateLogs?.Invoke(v, isError);
        }
    }
}
