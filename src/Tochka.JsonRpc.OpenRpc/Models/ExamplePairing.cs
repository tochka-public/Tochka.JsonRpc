namespace Tochka.JsonRpc.OpenRpc.Models
{
    public class ExamplePairing
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Summary { get; set; }
        public Example Params { get; set; }
        public Example Result { get; set; }
    }
}