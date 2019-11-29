using ORMi;
using System;
using System.Diagnostics;
using System.Linq;

namespace uGovernor
{
    static class FingerPrint
    {
        [WMIClass("Win32_Processor")]
        public class Processor : WMIInstance
        {
            public string UniqueId { get; set; }
            public string ProcessorId { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
            public string Manufacturer { get; set; }

            public override string ToString()
            {
                return string.Join("-", UniqueId, ProcessorId, Name, Description, Manufacturer);
            }
        }

        [WMIClass("Win32_BaseBoard")]
        public class BaseBoard : WMIInstance
        {
            public string Manufacturer { get; set; }
            public string Product { get; set; }
            public string Name { get; set; }
            public string SerialNumber { get; set; }

            public override string ToString()
            {
                return string.Join("-", Manufacturer, Product, Name, SerialNumber);
            }
        }


        static Lazy<byte[]> _fingerPrint = new Lazy<byte[]>(RunQuery);

        public static byte[] Value => _fingerPrint.Value;

        static byte[] RunQuery()
        {
            var helper = new WMIHelper("root\\CimV2");
            var processors = helper.Query<Processor>().Select(x => x.ToString());
            var baseBoards = helper.Query<BaseBoard>().Select(x => x.ToString());

            var result = processors.Concat(baseBoards).Aggregate(">>", (current, next) => current + next);

            return NativeMethods.GetBytes(result);
        }
    }
}
