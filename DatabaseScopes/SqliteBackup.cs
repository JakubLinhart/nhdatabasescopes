using System;
using System.Data.SQLite;
using System.Reflection;
using System.Runtime.InteropServices;

namespace NHibernateDatabaseScope.DatabaseScopes
{
    public static class SqliteBackup
    {
        [DllImport("System.Data.SQLite.DLL", CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr sqlite3_backup_init(IntPtr destDb, byte[] destname, IntPtr srcDB, byte[] srcname);

        [DllImport("System.Data.SQLite.DLL", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int sqlite3_backup_step(IntPtr backup, int pages);

        [DllImport("System.Data.SQLite.DLL", CallingConvention = CallingConvention.Cdecl)]
        internal static extern int sqlite3_backup_finish(IntPtr backup);

        public static void Backup(SQLiteConnection source, SQLiteConnection destination)
        {
            IntPtr sourceHandle = GetConnectionHandle(source);
            IntPtr destinationHandle = GetConnectionHandle(destination);

            IntPtr backupHandle = sqlite3_backup_init(destinationHandle, SQLiteConvert.ToUTF8("main"), sourceHandle, SQLiteConvert.ToUTF8("main"));
            sqlite3_backup_step(backupHandle, -1);
            sqlite3_backup_finish(backupHandle);
        }

        private static IntPtr GetConnectionHandle(SQLiteConnection source)
        {
            object sqlLite3 = GetPrivateFieldValue(source, "_sql");
            object connectionHandle = GetPrivateFieldValue(sqlLite3, "_sql");
            IntPtr handle = (IntPtr)GetPrivateFieldValue(connectionHandle, "handle");

            return handle;
        }

        private static object GetPrivateFieldValue(object instance, string fieldName)
        {
            var filedType = instance.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance);

            object result = filedType.GetValue(instance);
            return result;
        }
    }
}
