using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Shipwreck.VSDebugger
{
    /// <summary>
    /// 実行中のプロセスにアタッチされたデバッガーの情報を表します。
    /// </summary>
    [DataContract]
    public class DebuggerInfo
    {
        #region プロパティ

        /// <summary>
        /// デバッガーに関連付けられた<see cref="DebuggerInfoProvider" />の型を取得または設定します。
        /// </summary>
        [IgnoreDataMember]
        [DefaultValue(null)]
        public Type DebuggerType { get; set; }

        /// <summary>
        /// デバッガーに関連付けられた<see cref="DebuggerInfoProvider" />の完全修飾名を取得または設定します。
        /// </summary>
        [DataMember]
        [DefaultValue(null)]
        public string DebuggerName
        {
            get
            {
                if (DebuggerType == null)
                {
                    return null;
                }
                return DebuggerType.AssemblyQualifiedName;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    DebuggerType = null;
                }
                else
                {
                    DebuggerType = Type.GetType(value);
                }
            }
        }

        /// <summary>
        /// デバッガーのプロセスIDを取得または設定します。
        /// </summary>
        [DataMember]
        [DefaultValue(0)]
        public int ProcessId { get; set; }

        #endregion

        /// <inheritdoc/>
        public override string ToString()
        {
            return string.Format("{{DebuggerType={0}, ProcessId={1}}}", DebuggerType == null ? "null" : DebuggerType.Name, ProcessId);
        }
    }
}
