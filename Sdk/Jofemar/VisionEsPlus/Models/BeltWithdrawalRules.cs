using System.Collections.Generic;
using System.Linq;

namespace Filuet.Hardware.Dispensers.SDK.Jofemar.VisionEsPlus.Models
{
    public class BeltWithdrawalRules : List<BeltWithdrawalRule>
    {
        /// <summary>
        /// 1 means the last item is allowed to be extracted
        /// </summary>
        /// <param name="sku"></param>
        /// <returns></returns>
        public bool this[string sku]
            => this.FirstOrDefault(x => x.Sku == sku)?.TakeLast ?? false;

        public BeltWithdrawalRules(IEnumerable<string> sku) {
            foreach (var x in sku)
                Add(new BeltWithdrawalRule { Sku = x, TakeLast = true });
        }
    }
}
