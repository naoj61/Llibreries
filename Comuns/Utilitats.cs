using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Management;
using System.Net;
using System.Net.Mail;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Comuns
{
    public class Utilitats
    {
        public static List<string> NumSerieHd()
        {
            List<string> numS = new List<string>();

            ManagementObjectSearcher searcher = new ManagementObjectSearcher("select * from Win32_DiskDrive");
            foreach (ManagementObject share in searcher.Get())
            {
                try
                {
                    string numSerie = (string)share.Properties["SerialNumber"].Value;
                    numS.Add(numSerie);
                }
                catch (Exception)
                {
                }
            }

            return numS;
        }


        /// <summary>
        /// Torna una llista de les MACs del ordinador.
        /// </summary>
        /// <returns></returns>
        static List<string> NumMac()
        {
            List<string> numS = new List<string>();

            ManagementObjectSearcher searcher = new ManagementObjectSearcher("select * from Win32_NetworkAdapter");
            foreach (ManagementObject share in searcher.Get())
            {
                try
                {
                    string numSerie = (string)share.Properties["MACAddress"].Value;
                    if (!String.IsNullOrEmpty(numSerie))
                    {
                        numS.Add(numSerie);
                    }
                }
                catch
                {
                }
            }

            return numS;
        }

        /// <summary>
        /// Torna una llista de les MACs unificades del ordinador.
        /// </summary>
        /// <returns></returns>
        public static List<string> NumMacUnificades()
        {
            return NumMac().Select(s => UnificaMac(s)).ToList();
        }

        /// <summary>
        /// Elimina els separador de la MAC i converteix tot a majuscules.
        /// </summary>
        /// <param name="mac"></param>
        /// <returns></returns>
        public static string UnificaMac(string mac)
        {
            if(mac==null)
                return null;
            if(mac.Length != 17)
                throw new ApplicationException("Longitud de l'adreça MAC incorrecta");

            string macUnificada = mac.Substring(0, 2) + mac.Substring(3, 2) + mac.Substring(6, 2) + mac.Substring(9, 2) + mac.Substring(12, 2) + mac.Substring(15, 2);

            return macUnificada.ToUpper();
        }


        public static string GetMd5Hash(MD5 md5Hash, string input)
        {
            // Convert the input string to a byte array and compute the hash. 
            byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));

            // Create a new Stringbuilder to collect the bytes 
            // and create a string.
            StringBuilder sBuilder = new StringBuilder();

            // Loop through each byte of the hashed data  
            // and format each one as a hexadecimal string. 
            for (int i = 0; i < data.Length; i++)
            {
                sBuilder.Append(data[i].ToString("x2"));
            }

            // Return the hexadecimal string. 
            return sBuilder.ToString();
        }


        public static bool VerifyMd5Hash(MD5 md5Hash, string input, string hash)
        {
            // Hash the input. 
            string hashOfInput = GetMd5Hash(md5Hash, input);

            // Create a StringComparer an compare the hashes.
            StringComparer comparer = StringComparer.OrdinalIgnoreCase;

            return 0 == comparer.Compare(hashOfInput, hash);
        }

        public static string ConvertToUnSecureString(SecureString sp)
        {
            IntPtr unmanagedString = IntPtr.Zero;
            try
            {
                unmanagedString = Marshal.SecureStringToGlobalAllocUnicode(sp);
                return Marshal.PtrToStringUni(unmanagedString);
            }
            finally
            {
                Marshal.ZeroFreeGlobalAllocUnicode(unmanagedString);
            }
        }


        public static void EnviaEmail(string origen, string desti, string subject, string body)
        {
            MailMessage mail = new MailMessage(origen, desti);
            mail.Subject = subject;
            mail.Body = body;

            // A través e gmail
            SmtpClient smtp = new SmtpClient();
            smtp.Host = "smtp.gmail.com"; // the host name
            smtp.Port = 25; //port number
            smtp.EnableSsl = true; //whether your smtp server requires SSL
            smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
            smtp.UseDefaultCredentials = false;
            smtp.Credentials = new NetworkCredential(origen, "gfcdf74wUoVBTJYfvreTKIO7gh8HY7regferui");
            smtp.Timeout = 20000;
            smtp.Send(mail);
        }


        /// <summary>
        /// Preserva l'StackTrace per poder reenviar una excepcio amb throw.
        /// Despres de cridar aqiest mètode, s'ha de fer throw.
        /// </summary>
        /// <param name="exception">Excepció a preservar</param>
        public static void PreserveStackTrace(Exception exception)
        {
            //http://weblogs.asp.net/fmarguerie/archive/2008/01/02/rethrowing-exceptions-and-preserving-the-full-call-stack-trace.aspx

            System.Reflection.MethodInfo preserveStackTrace = typeof(Exception).GetMethod("InternalPreserveStackTrace",
                                                                                           System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            preserveStackTrace.Invoke(exception, null);
        }


        public static string ReadSetting(string key)
        {
            var appSettings = ConfigurationManager.AppSettings;
            return appSettings[key];
        }

    }
}