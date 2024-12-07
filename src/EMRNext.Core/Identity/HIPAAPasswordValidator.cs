using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace EMRNext.Core.Identity
{
    public class HIPAAPasswordValidator<TUser> : IPasswordValidator<TUser> where TUser : class
    {
        public async Task<IdentityResult> ValidateAsync(UserManager<TUser> manager, TUser user, string password)
        {
            var errors = new List<IdentityError>();

            // Check for common passwords
            var commonPasswords = new[] { "password", "admin123", "letmein", "welcome" };
            if (commonPasswords.Contains(password.ToLower()))
            {
                errors.Add(new IdentityError
                {
                    Code = "CommonPassword",
                    Description = "The password provided is too common and easily guessable."
                });
            }

            // Check for sequential characters
            if (HasSequentialCharacters(password))
            {
                errors.Add(new IdentityError
                {
                    Code = "SequentialCharacters",
                    Description = "Password cannot contain sequential characters (e.g., '123', 'abc')."
                });
            }

            // Check for repeated characters
            if (HasRepeatedCharacters(password))
            {
                errors.Add(new IdentityError
                {
                    Code = "RepeatedCharacters",
                    Description = "Password cannot contain more than 2 repeated characters in a row."
                });
            }

            // Check for username in password
            var userName = await manager.GetUserNameAsync(user);
            if (password.ToLower().Contains(userName.ToLower()))
            {
                errors.Add(new IdentityError
                {
                    Code = "PasswordContainsUsername",
                    Description = "Password cannot contain your username."
                });
            }

            return errors.Count == 0 ? IdentityResult.Success : IdentityResult.Failed(errors.ToArray());
        }

        private bool HasSequentialCharacters(string password)
        {
            const string sequences = "abcdefghijklmnopqrstuvwxyz01234567890";
            var reverseSequences = new string(sequences.Reverse().ToArray());

            for (int i = 0; i < password.Length - 2; i++)
            {
                var chunk = password.Substring(i, 3).ToLower();
                if (sequences.Contains(chunk) || reverseSequences.Contains(chunk))
                    return true;
            }

            return false;
        }

        private bool HasRepeatedCharacters(string password)
        {
            for (int i = 0; i < password.Length - 2; i++)
            {
                if (password[i] == password[i + 1] && password[i] == password[i + 2])
                    return true;
            }

            return false;
        }
    }
}
