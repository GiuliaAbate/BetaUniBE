using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;

namespace BetaUni.Other
{
    //Classe che contiene metodi vari di base
    public class Services
    {
        //Metodo per generare l'id del prof che deve essere composto da 6 lettere maiuscole
        public string GenerateProfId(int length = 6)
        {
            try
            {
                const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

                var random = new Random();
                return new string(Enumerable.Repeat(chars, length)
                    .Select(s => s[random.Next(s.Length)]).ToArray());
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        //Metodo per generare l'id dello studente che deve essere composto da 6 numeri
        public string GenerateStudId(int length = 6)
        {
            try
            {
                const string numbers = "0123456789";

                var random = new Random();
                return new string(Enumerable.Repeat(numbers, length)
                    .Select(n => n[random.Next(n.Length)]).ToArray());
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        //Metodo per criptare le password tramite HASH256
        //HASH256 = hash (la stringa criptata) + salt(la sequenza che cripta la sequenza)
        public static KeyValuePair<string, string> SaltEncryption(string pass)  
        {
            try
            {
                KeyValuePair<string, string> crypted;
                byte[] bytesSalt = new byte[16];
                RandomNumberGenerator.Fill(bytesSalt); //riempire con numeri casuali l'array di byte

                //Si converte un'array in una stringa
                //KeyDerivation da algoritmi per l'esecuzione della derivazione della chiave.
                string hashValue = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                    password: pass,
                    salt: bytesSalt,
                    prf: KeyDerivationPrf.HMACSHA256, //si specifica il prF che deve essere usato per la derivazione della chiave.
                    iterationCount: 1000, //numero di iterazioni
                    numBytesRequested: 32 //vale 32 byte
                    ));

                crypted = new KeyValuePair<string, string>(hashValue, Convert.ToBase64String(bytesSalt));
                return crypted;
            }
            catch (Exception ex)
            {
                throw;
            }
        }


        //Metodo per criptare la password usando un salt esistente (per il login)
        public static KeyValuePair<string, string> SaltEncryption(string pass, string salt)
        {
            try
            {
                byte[] bytesSalt = Convert.FromBase64String(salt);

                string hashValue = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                    password: pass,
                    salt: bytesSalt,
                    prf: KeyDerivationPrf.HMACSHA256,
                    iterationCount: 1000,
                    numBytesRequested: 32
                ));

                return new KeyValuePair<string, string>(hashValue, salt);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        //Metodo per verificare la password
        public static bool VerifyPassword(string inputPassword, string storedHash, string storedSalt)
        {
            try
            {
                var encrypted = SaltEncryption(inputPassword, storedSalt);
                return encrypted.Key == storedHash;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

    }
}
