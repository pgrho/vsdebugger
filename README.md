#Subprocess Debugging Helper for .NET Framework/Visual Studio#

Attaches Visual Studio Debugger to a subprocess that is within debugging sln.

#Usage#

1. Import `Shipwreck.VSDebugger` namespace.
2. Get `DebuggerInfo` of main process by calling `DebuggerInfoProvider.GetDebuggerInfo()`.
3. Launch some process.
4. Attach the `DebuggerInfo` to new process by `DebuggerInfoProvider.AttachTo(DebuggerInfo, int processId)`.

#NuGet#

https://www.nuget.org/packages/Shipwreck.VSDebugger/
