namespace CoreBot.Models
{
    public class FAQModel
    {
        public string Id { get; set; }

        public string Faq { get; set; }

        public string Answer { get; set; }

        public object[] Categories { get; set; }
    }
}
