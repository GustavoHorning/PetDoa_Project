namespace PetDoa.Util
{
    public static class PasswordValidator
    {
        public static bool IsValid(string password, out List<string> errorMessages)
        {
            errorMessages = new List<string>();

            if (string.IsNullOrWhiteSpace(password))
            {
                errorMessages.Add("A senha não pode estar vazia.");
            }

            if (password.Length < 8)
            {
                errorMessages.Add("A senha deve ter pelo menos 8 caracteres.");
            }

            if (!password.Any(char.IsUpper))
            {
                errorMessages.Add("A senha deve conter pelo menos uma letra maiúscula.");
            }

            if (!password.Any(char.IsLower))
            {
                errorMessages.Add("A senha deve conter pelo menos uma letra minúscula.");
            }

            if (!password.Any(char.IsDigit))
            {
                errorMessages.Add("A senha deve conter pelo menos um número.");
            }

            if (!password.Any(c => "!@#$%^&*()-_=+[]{}|;:',.<>?/`~".Contains(c)))
            {
                errorMessages.Add("A senha deve conter pelo menos um caractere especial.");
            }

            // Se não houver erros, o retorno é verdadeiro
            return !errorMessages.Any();
        }
    }
}
