using System;
using System.Linq;
using System.Management;

namespace uGovernor
{
    class FingerPrint : IFingerPrint
    {
        public byte[] Get() => NativeMethods.GetBytes(_fingerPrint.Value);

        static Lazy<string> _fingerPrint = new Lazy<string>(() =>
        {
            const string CPUQry = "SELECT UniqueId, ProcessorId, Name, Description, Manufacturer FROM Win32_Processor";
            const string MoboQry = "SELECT Manufacturer, Product, Name, SerialNumber FROM Win32_BaseBoard";

            return $"{RunQuery(CPUQry, MoboQry)}";
        });

        static string RunQuery(params string[] queries)
        {
            var result = queries.AsParallel()
                                .AsSequential()
                                .Select(qry => new SelectQuery(qry))
                                .Select(qry => new ManagementObjectSearcher(qry))
                                .Select(searcher => searcher.Get())
                                .Select(results => results.Cast<ManagementObject>()
                                                          .SelectMany(x => x.Properties.Cast<PropertyData>())
                                                          .Select(x => x.Value)
                                                          .Where(x => x != null)
                                                          .Aggregate("", (current, next) => current + next));

            return string.Join(">>", result);
        }
    }
}
