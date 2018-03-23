namespace ECommerceUpo.Data
{
    //bean per mantenere il carrello in sessione
    public class CartBean
    {
        public int ProductId { get; set; }

        public string Title { get; set; }

        public string Description { get; set; }

        public double Price { get; set; }

        public double Discount { get; set; }

        public byte[] Image { get; set; }

        public int Quantity { get; set; }
    }
}
