namespace Filuet.Hardware.Dispensers.SDK.Jofemar.VisionEsPlus.Models
{
    public class BeltWithdrawalRule
    {
        public string Sku { get; set; }
        /// <summary>
        /// The very last item of product can be extracted from the product
        /// </summary>
        public bool TakeLast { get; set; }
    }
}
