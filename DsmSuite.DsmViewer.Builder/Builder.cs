using System.Diagnostics;
using DsmSuite.DsmViewer.Model.Model;
using DsmSuite.DsmViewer.Util;

namespace DsmSuite.DsmViewer.Builder
{
    class Builder
    {
        private readonly BuilderSettings _builderSettings;
        private readonly IDsmModel _model;

        public Builder(IDsmModel model, BuilderSettings builderSettings)
        {
            _builderSettings = builderSettings;
            _model = model;
        }

        public void BuildModel()
        {
            Logger.LogUserMessage("Build model");
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            _model.Import(_builderSettings.InputFilename);

            Process currentProcess = Process.GetCurrentProcess();
            const long million = 1000000;
            long peakPagedMemMb = currentProcess.PeakPagedMemorySize64 / million;
            long peakVirtualMemMb = currentProcess.PeakVirtualMemorySize64 / million;
            long peakWorkingSetMb = currentProcess.PeakWorkingSet64 / million;
            Logger.LogUserMessage($" peak physical memory usage {peakWorkingSetMb:0.000}MB");
            Logger.LogUserMessage($" peak paged memory usage    {peakPagedMemMb:0.000}MB");
            Logger.LogUserMessage($" peak virtual memory usage  {peakVirtualMemMb:0.000}MB");

            stopWatch.Stop();
            Logger.LogUserMessage($" total elapsed time = {stopWatch.Elapsed}");
        }
    }
}
