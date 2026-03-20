namespace BA.Backend.Application.Exceptions
{
    public class InvalidCredentialsExeption : Exception
    {
        public InvalidCredentialsExeption(string message) 
                  : base("Email o contraseña inválidos.")
        {
        }
    }
}