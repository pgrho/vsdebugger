using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;

namespace Shipwreck.VSDebugger
{
    internal static class NativeMethods
    {
        [DllImport("Kernel32")]
        internal static extern uint GetTickCount();

        /// <summary>
        /// Returns a pointer to an implementation of <see cref="T:System.Runtime.InteropServices.ComTypes.IBindCtx"/> (a bind context object).
        /// This object stores information about a particular moniker-binding operation.
        /// </summary>
        /// <param name="reserved">This parameter is reserved and must be <c>0</c>.</param>
        /// <param name="ppbc">Address of an <see cref="T:System.Runtime.InteropServices.ComTypes.IBindCtx"/>* pointer variable that receives
        /// the interface pointer to the new bind context object. When the function is
        /// successful, the caller is responsible for calling Release on the bind context.
        /// A NULL value for the bind context indicates that an error occurred.</param>
        /// <returns>This function can return the standard return values E_OUTOFMEMORY and S_OK.</returns>
        [DllImport("ole32.dll")]
        internal static extern int CreateBindCtx(uint reserved, out IBindCtx ppbc);


        [DllImport("ntdll.dll")]
        internal static extern int NtQueryInformationProcess(IntPtr processHandle, int processInformationClass, ref ProcessBasicInformation processInformation, int processInformationLength, out int returnLength);
    }
}
