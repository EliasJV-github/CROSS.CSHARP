namespace CROSS.AUTH.TOKEN.Exceptions
{
    public class ModelReadJwt
    {
        public string Nombre { get; set; } = string.Empty;

        public string Matricula { get; set; } = string.Empty;

        public List<string> Politicas { get; set; }
    }
}
