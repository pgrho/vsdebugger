using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Shipwreck.VSDebugger
{
    /// <summary>
    /// デバッガーに対する操作を提供します。
    /// </summary>
    public abstract class DebuggerInfoProvider
    {
        #region フィールド

        /// <summary>
        /// 登録されている<see cref="DebuggerInfoProvider" />のリスト。
        /// </summary>
        private static readonly List<DebuggerInfoProvider> _Providers;

        #endregion

        #region コンストラクター

        /// <summary>
        /// <see cref="DebuggerInfoProvider" />のクラスを初期化します。
        /// </summary>
        static DebuggerInfoProvider()
        {
            _Providers = new List<DebuggerInfoProvider>();
            _Providers.Add(new VisualStudioDebuggerInfoProvider());
        }

        #endregion

        #region メソッド

        #region クラス メソッド

        /// <summary>
        /// 指定した<see cref="DebuggerInfoProvider"/>を登録します。
        /// </summary>
        /// <param name="provider">登録する<see cref="DebuggerInfoProvider"/>。</param>
        public static void Register(DebuggerInfoProvider provider)
        {
            lock (_Providers)
            {
                _Providers.Add(provider);
            }
        }

        /// <summary>
        /// 現在のプロセスに関連付けられたデバッガーの情報を返します。
        /// </summary>
        /// <returns>
        /// 現在のプロセスに関連付けられたデバッガーの情報。
        /// 情報を取得できない場合は<c>null</c>。
        /// </returns>
        public static DebuggerInfo GetDebuggerInfo()
        {
            lock (_Providers)
            {
                return _Providers
                            .Select(p => p.GetCurrentDebuggerInfo())
                            .FirstOrDefault(r => r != null);
            }
        }

        /// <summary>
        /// 現在のプロセスを指定したデバッガーにアタッチします。
        /// </summary>
        /// <param name="debuggerInfo">現在のプロセスをアタッチするデバッガー情報。</param>
        /// <returns>アタッチに成功した場合は<c>True</c>。その他の場合は<c>False</c>。</returns>
        public static bool AttachTo(DebuggerInfo debuggerInfo)
            => AttachTo(debuggerInfo, Process.GetCurrentProcess().Id);

        /// <summary>
        /// 指定したプロセスを指定したデバッガーにアタッチします。
        /// </summary>
        /// <param name="debuggerInfo">プロセスをアタッチするデバッガー情報。</param>
        /// <param name="processId">アタッチするプロセスのプロセスID。</param>
        /// <returns>アタッチに成功した場合は<c>True</c>。その他の場合は<c>False</c>。</returns>
        public static bool AttachTo(DebuggerInfo debuggerInfo, int processId)
        {
            if (debuggerInfo == null)
            {
                return false;
            }
            if (processId == Process.GetCurrentProcess().Id && Debugger.IsAttached)
            {
                return false;
            }
            lock (_Providers)
            {
                return _Providers.Any(p => p.AttachToDebugger(debuggerInfo, processId));
            }
        }

        #endregion

        #region インスタンス メソッド

        /// <summary>
        /// 派生クラスで実装された場合、実行中のプロセスにアタッチされた現在のインスタンスが表すデバッガーの情報を返します。
        /// </summary>
        /// <returns>
        /// 実行中のプロセスにアタッチされた現在のインスタンスが表すデバッガーの情報。
        /// 情報を取得できない場合は<c>null</c>。
        /// </returns>
        public abstract DebuggerInfo GetCurrentDebuggerInfo();

        /// <summary>
        /// 派生クラスで実装された場合、指定したプロセスを指定したデバッガーにアタッチします。
        /// </summary>
        /// <param name="debuggerInfo">プロセスをアタッチするデバッガー情報。</param>
        /// <param name="processId">アタッチするプロセスのプロセスID。</param>
        /// <returns><paramref name="debuggerInfo" />が現在のインスタンスのデバッガー情報で、アタッチに成功した場合は<c>True</c>。その他の場合は<c>False</c>。</returns>
        public abstract bool AttachToDebugger(DebuggerInfo debuggerInfo, int processId);

        #endregion

        #endregion
    }
}
