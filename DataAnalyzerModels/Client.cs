namespace DesafioIlegraModels
{
    public class Client : IEntity
    {
        public int Id { get; set; }
        public string CNPJ { get; set; }
        public string Name { get; set; }
        public string BusinessArea { get; set; }
    }
}
