namespace Oocx.Pkcs
{
    internal class ContextSpecific : Asn1Object
    {
        public ContextSpecific() : base(0xa0)
        {
            Data = new byte[0];
        }
    }
}