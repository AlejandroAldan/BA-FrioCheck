namespace BA.Backend.Application.Exceptions
{
    public class UserNotFoundExeption : Exception
    {
        public UserNotFoundExeption(string userId)
         : base($"id del Usuario '{userId}' no encontrado.")
        {
        }
    }
}