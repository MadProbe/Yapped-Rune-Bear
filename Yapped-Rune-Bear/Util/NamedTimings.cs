using SoulsFormats.Util;

namespace Chomp.Util {
    public static class NamedTimings {
        public static readonly Tracing.NamedTiming FileWriterFlushEncodeNamedTiming = new("FileWriter.Flush::encoder.GetBytes()");
        public static readonly Tracing.NamedTiming FileWriterFlushFileWriteNamedTiming = new("FileWriter.Flush::fileStream.Write()");
        public static readonly Tracing.NamedTiming LoadParamResultApplyParamdefNamedTiming = new("LoadParamResult::PARAM.ApplyParamdefCarefully()");
        public static readonly Tracing.NamedTiming LoadParamResultSetDataSourceNamedTiming = new("LoadParamResult::SetDataSource");
        public static readonly Tracing.NamedTiming LoadParamResultSoulsFilePARAMReadNamedTiming = new ("LoadParamResult::SoulsFile<PARAM>.Read()");
    }
}
