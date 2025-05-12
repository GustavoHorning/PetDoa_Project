namespace PetDoa.Services
{
    public class ServiceResponse<T>
    {
        public T Data { get; set; }
        public string Message { get; set; }
        public bool Success { get; set; }
        public List<string> Errors { get; set; }  // Nova propriedade para erros de senha

    }

}
