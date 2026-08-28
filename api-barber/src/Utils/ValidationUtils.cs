using System.Text.RegularExpressions;

namespace api_barber.Utils
{
    public static class ValidationUtils
    {
        public static string CleanDigits(string? input)
        {
            if (string.IsNullOrWhiteSpace(input)) return string.Empty;
            return Regex.Replace(input, @"[^\d]", "");
        }

        public static bool IsValidEmail(string? email)
        {
            if (string.IsNullOrWhiteSpace(email)) return false;
            try
            {
                var addr = new System.Net.Mail.MailAddress(email.Trim());
                return addr.Address == email.Trim() && email.Contains('.') && !email.EndsWith(".");
            }
            catch
            {
                return false;
            }
        }

        public static bool IsValidPhone(string? phone)
        {
            string digits = CleanDigits(phone);
            if (digits.Length != 10 && digits.Length != 11) return false;
            int ddd = int.Parse(digits.Substring(0, 2));
            if (ddd < 11 || ddd > 99) return false;
            if (digits.Distinct().Count() == 1) return false;
            return true;
        }

        public static bool IsValidCpf(string? cpf)
        {
            string digits = CleanDigits(cpf);
            if (digits.Length != 11) return false;
            if (digits.Distinct().Count() == 1) return false;

            int[] multiplicador1 = [10, 9, 8, 7, 6, 5, 4, 3, 2];
            int[] multiplicador2 = [11, 10, 9, 8, 7, 6, 5, 4, 3, 2];

            string tempCpf = digits.Substring(0, 9);
            int soma = 0;

            for (int i = 0; i < 9; i++)
            {
                soma += int.Parse(tempCpf[i].ToString()) * multiplicador1[i];
            }

            int resto = soma % 11;
            int digito1 = resto < 2 ? 0 : 11 - resto;

            tempCpf += digito1;
            soma = 0;

            for (int i = 0; i < 10; i++)
            {
                soma += int.Parse(tempCpf[i].ToString()) * multiplicador2[i];
            }

            resto = soma % 11;
            int digito2 = resto < 2 ? 0 : 11 - resto;

            return digits.EndsWith(digito1.ToString() + digito2.ToString());
        }

        public static bool IsValidCnpj(string? cnpj)
        {
            string digits = CleanDigits(cnpj);
            if (digits.Length != 14) return false;
            if (digits.Distinct().Count() == 1) return false;

            int[] multiplicador1 = [5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2];
            int[] multiplicador2 = [6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2];

            string tempCnpj = digits.Substring(0, 12);
            int soma = 0;

            for (int i = 0; i < 12; i++)
            {
                soma += int.Parse(tempCnpj[i].ToString()) * multiplicador1[i];
            }

            int resto = soma % 11;
            int digito1 = resto < 2 ? 0 : 11 - resto;

            tempCnpj += digito1;
            soma = 0;

            for (int i = 0; i < 13; i++)
            {
                soma += int.Parse(tempCnpj[i].ToString()) * multiplicador2[i];
            }

            resto = soma % 11;
            int digito2 = resto < 2 ? 0 : 11 - resto;

            return digits.EndsWith(digito1.ToString() + digito2.ToString());
        }

        public static bool IsValidDocument(string? document)
        {
            string digits = CleanDigits(document);
            if (digits.Length == 11) return IsValidCpf(digits);
            if (digits.Length == 14) return IsValidCnpj(digits);
            return false;
        }
    }
}
