using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace ConsoleApp1
{
    // Token: 0x02000002 RID: 2
    public class EmbedTransformFile
    {
        // Token: 0x06000001 RID: 1
        [DllImport("msi.dll", CharSet = CharSet.Unicode, EntryPoint = "MsiOpenDatabaseW", ExactSpelling = true)]
        private static extern uint MsiOpenDatabase(string szDatabasePath, IntPtr szPersist, ref IntPtr phdatabase);

        // Token: 0x06000002 RID: 2
        [DllImport("msi.dll", CharSet = CharSet.Unicode, EntryPoint = "MsiDatabaseOpenViewW", ExactSpelling = true)]
        private static extern uint MsiDatabaseOpenView(IntPtr hDatabase, string szQuery, ref IntPtr phView);

        // Token: 0x06000003 RID: 3
        [DllImport("msi.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
        private static extern uint MsiDatabaseCommit(IntPtr hDatabase);

        // Token: 0x06000004 RID: 4
        [DllImport("msi.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
        private static extern uint MsiCloseHandle(IntPtr hAny);

        // Token: 0x06000005 RID: 5
        [DllImport("msi.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
        private static extern IntPtr MsiCreateRecord(uint cParams);

        // Token: 0x06000006 RID: 6
        [DllImport("msi.dll", CharSet = CharSet.Unicode, EntryPoint = "MsiRecordSetStringW", ExactSpelling = true)]
        private static extern uint MsiRecordSetString(IntPtr hRecord, uint iField, string szValue);

        // Token: 0x06000007 RID: 7
        [DllImport("msi.dll", CharSet = CharSet.Unicode, EntryPoint = "MsiRecordSetStreamW", ExactSpelling = true)]
        private static extern uint MsiRecordSetStream(IntPtr hRecord, uint iField, string szFilePath);

        // Token: 0x06000008 RID: 8
        [DllImport("msi.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
        private static extern uint MsiViewExecute(IntPtr hView, IntPtr hRecord);

        // Token: 0x06000009 RID: 9
        [DllImport("msi.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
        private static extern uint MsiViewModify(IntPtr hView, int eModifyMode, IntPtr hRecord);

        // Token: 0x0600000A RID: 10 RVA: 0x00002050 File Offset: 0x00000250
        private EmbedTransformFile()
        {
        }

        // Token: 0x0600000B RID: 11 RVA: 0x00002058 File Offset: 0x00000258
        public static int Excute(string msiFile, string transformFile)
        {
            try
            {
                string text = msiFile;
                if (!File.Exists(text))
                {
                    throw new ApplicationException(string.Format("MSI file \"{0}\" cannot be found", text));
                }
                string text2 = transformFile;
                if (!File.Exists(text2))
                {
                    throw new ApplicationException(string.Format("MST file \"{0}\" cannot be found", text2));
                }
                IntPtr zero = IntPtr.Zero;
                IntPtr szPersist = new IntPtr(1);
                if ((ulong)MsiOpenDatabase(text, szPersist, ref zero) != 0UL)
                {
                    throw new ApplicationException("Error opening database file");
                }
                IntPtr zero2 = IntPtr.Zero;
                if ((ulong)MsiDatabaseOpenView(zero, "SELECT `Name`,`Data` FROM _Storages", ref zero2) != 0UL)
                {
                    throw new ApplicationException("Error opening view for database file");
                }
                IntPtr hRecord = IntPtr.Zero;
                if ((hRecord = MsiCreateRecord(2u)) == IntPtr.Zero)
                {
                    throw new ApplicationException("Error while creating new record");
                }
                if ((ulong)MsiRecordSetString(hRecord, 1u, Path.GetFileName(text2)) != 0UL)
                {
                    throw new ApplicationException("Error while setting substorage name");
                }
                if ((ulong)MsiRecordSetStream(hRecord, 2u, text2) != 0UL)
                {
                    throw new ApplicationException("Error while storing the substorage");
                }
                if ((ulong)MsiViewExecute(zero2, hRecord) != 0UL)
                {
                    throw new ApplicationException("Error while executing the view");
                }
                if ((ulong)MsiViewModify(zero2, 3, hRecord) != 0UL)
                {
                    throw new ApplicationException("Error while modifying the view");
                }
                if ((ulong)MsiDatabaseCommit(zero) != 0UL)
                {
                    throw new ApplicationException("Error while committing the database");
                }
                MsiCloseHandle(zero2);
                MsiCloseHandle(zero);
            }
            catch (ApplicationException ex)
            {
                Console.Error.WriteLine("\r\nEmbedTransform.exe : fatal error EBTR0000: {0}", ex.Message);
                return 1;
            }
            catch (Exception ex2)
            {
                Console.Error.WriteLine("\r\nEmbedTransform.exe : fatal error EBTR0001: {0}\r\n\r\nStack Trace:\r\n{1}", ex2.Message, ex2.StackTrace);
                return 1;
            }
            return 0;
        }

        // Token: 0x04000001 RID: 1
        internal const short SUCCESS = 0;

        // Token: 0x04000002 RID: 2
        internal const short FAILURE = 1;

        // Token: 0x04000003 RID: 3
        internal const short MSIMODIFY_ASSIGN = 3;
    }
}
