using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Shipwreck.VSDebugger
{
    /// <summary>
    /// Visual Studioデバッガーに対する操作を提供します。
    /// </summary>
    public sealed class VisualStudioDebuggerInfoProvider : DebuggerInfoProvider
    {
        /// <summary>
        /// 派生クラスで実装された場合、実行中のプロセスにアタッチされたVisual Studioデバッガーの情報を返します。
        /// </summary>
        /// <returns>
        /// 実行中のプロセスにアタッチされたVisual Studioデバッガーの情報。
        /// 情報を取得できない場合は<c>null</c>。
        /// </returns>
        public override DebuggerInfo GetCurrentDebuggerInfo()
        {
            if (!Debugger.IsAttached)
            {
                return null;
            }

            var p = Process.GetCurrentProcess();
            while (p != null)
            {
                if (p.ProcessName.Equals("devenv", StringComparison.InvariantCultureIgnoreCase))
                {
                    return new DebuggerInfo()
                    {
                        DebuggerType = GetType(),
                        ProcessId = p.Id
                    };
                }

                var pbi = new ProcessBasicInformation();
                int returnLength;
                var status = NativeMethods.NtQueryInformationProcess(
                                                p.Handle,
                                                0,
                                                ref pbi,
                                                Marshal.SizeOf(typeof(ProcessBasicInformation)),
                                                out returnLength);
                if (status != 0)
                {
                    return null;
                }

                p = Process.GetProcessById(pbi.InheritedFromUniqueProcessId.ToInt32());
            }
            return null;
        }

        /// <summary>
        /// 指定したプロセスを指定したVisual Studioデバッガーにアタッチします。
        /// </summary>
        /// <param name="debuggerInfo">プロセスをアタッチするデバッガー情報。</param>
        /// <param name="processId">アタッチするプロセスのプロセスID。</param>
        /// <returns><paramref name="debuggerInfo" />がVisual Studioのデバッガー情報で、アタッチに成功した場合は<c>True</c>。その他の場合は<c>False</c>。</returns>
        public override bool AttachToDebugger(DebuggerInfo debuggerInfo, int processId)
        {
            if (debuggerInfo == null
                || debuggerInfo.DebuggerType != typeof(VisualStudioDebuggerInfoProvider)
                || !(debuggerInfo.ProcessId > 0))
            {
                return false;
            }

            IBindCtx bc = null;
            IRunningObjectTable rot = null;
            IEnumMoniker enumMoniker = null;
            try
            {
                var r = NativeMethods.CreateBindCtx(0, out bc);
                Marshal.ThrowExceptionForHR(r);
                if (bc == null)
                {
                    throw new Win32Exception();
                }
                bc.GetRunningObjectTable(out rot);
                if (rot == null)
                {
                    throw new Win32Exception();
                }

                rot.EnumRunning(out enumMoniker);
                if (enumMoniker == null)
                {
                    throw new Win32Exception();
                }

                var dteSuffix = ":" + debuggerInfo.ProcessId;

                var moniker = new IMoniker[1];
                while (enumMoniker.Next(1, moniker, IntPtr.Zero) == 0 && moniker[0] != null)
                {
                    string dn;

                    moniker[0].GetDisplayName(bc, null, out dn);

                    if (dn.StartsWith("!VisualStudio.DTE.") && dn.EndsWith(dteSuffix))
                    {
                        object dte, dbg, lps;
                        rot.GetObject(moniker[0], out dte);

                        for (var i = 0; i < 10; i++)
                        {
                            try
                            {
                                dbg = dte.GetType().InvokeMember("Debugger", BindingFlags.GetProperty, null, dte, null);
                                lps = dbg.GetType().InvokeMember("LocalProcesses", BindingFlags.GetProperty, null, dbg, null);
                                var lpn = (System.Collections.IEnumerator)lps.GetType().InvokeMember("GetEnumerator", BindingFlags.InvokeMethod, null, lps, null);

                                while (lpn.MoveNext())
                                {
                                    var pn = Convert.ToInt32(lpn.Current.GetType().InvokeMember("ProcessID", BindingFlags.GetProperty, null, lpn.Current, null));

                                    if (pn == processId)
                                    {
                                        lpn.Current.GetType().InvokeMember("Attach", BindingFlags.InvokeMethod, null, lpn.Current, null);
                                        return true;
                                    }
                                }
                            }
                            catch (COMException)
                            {
                                Thread.Sleep(250);
                            }
                        }
                        Marshal.ReleaseComObject(moniker[0]);

                        break;
                    }

                    Marshal.ReleaseComObject(moniker[0]);
                }
                return false;
            }
            finally
            {
                if (enumMoniker != null)
                {
                    Marshal.ReleaseComObject(enumMoniker);
                }
                if (rot != null)
                {
                    Marshal.ReleaseComObject(rot);
                }
                if (bc != null)
                {
                    Marshal.ReleaseComObject(bc);
                }
            }
        }
    }
}
