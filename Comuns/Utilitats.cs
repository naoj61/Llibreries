using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Management;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace Comuns
{
    public class UtilitatsException : Exception
    {
        public UtilitatsException(Utilitats.Errors error) : base("Error Utilitats")
        {
            Error = error;
        }

        public readonly Utilitats.Errors Error;
    }

    public class Utilitats
    {
           public enum Errors
           {
               LongitudAdrecaMacIncorrecta,
               LlicenciaCaducada
           }

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
                throw new UtilitatsException(Errors.LongitudAdrecaMacIncorrecta);
            
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


        public static DateTime HoraInternet()
        {
            var client = new TcpClient("time.nist.gov", 13);
            DateTime localDateTime;
            using (var streamReader = new StreamReader(client.GetStream()))
            {
                var response = streamReader.ReadToEnd();
                var utcDateTimeString = response.Substring(7, 17);
                localDateTime = DateTime.ParseExact(utcDateTimeString, "yy-MM-dd hh:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal);
            }

            return localDateTime;
        }



        /// <summary>
        /// Grava un valor en el registre de Windows.
        /// </summary>
        /// <param name="baseKey">  És l'element principal del registre de Windows. </param>
        /// <param name="clauRegistre">És la carpeta del registre.</param>
        /// <param name="nom">Nom de la variable.</param>
        /// <param name="valor">Valor de la variable.</param>
        public static void GravaVariableRegistre(RegistryKey baseKey, string clauRegistre, string nom, object valor)
        {
            using (RegistryKey OurKey1 = baseKey.OpenSubKey(clauRegistre, RegistryKeyPermissionCheck.ReadWriteSubTree))
            {
                if (OurKey1 == null)
                {
                    // La clau no existeix, la creo.
                    using (RegistryKey OurKey2 = baseKey.CreateSubKey(clauRegistre, RegistryKeyPermissionCheck.ReadWriteSubTree))
                    {
                        if (OurKey2 != null)
                            OurKey2.SetValue(nom, valor);
                    }
                }
                else
                    OurKey1.SetValue(nom, valor);
            }
        }


        /// <summary>
        /// Llegeig el valor d'una variable en el registre de Windows.
        /// </summary>
        /// <param name="baseKey">  És l'element principal del registre de Windows. </param>
        /// <param name="clauRegistre">És la carpeta del registre.</param>
        /// <param name="nom">Nom de la variable.</param>
        /// <returns></returns>
        public static string LlegeixVariableRegistre(RegistryKey baseKey, string clauRegistre, string nom)
        {
            using (RegistryKey OurKey1 = baseKey.OpenSubKey(clauRegistre, RegistryKeyPermissionCheck.ReadSubTree))
            {
                if (OurKey1 == null)
                    return null;

                object valor = OurKey1.GetValue(nom);
                return valor == null ? null : valor.ToString();
            }
        }


        /// <summary>
        /// Valida que la llicència estigui vigent, sino llença una exepció.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="nomCompanyia"></param>
        public static void ComprovaLlicencia(DateTime data, string nomCompanyia)
        {
            try
            {
                //string claureg = "Software" + Path.DirectorySeparatorChar + MediaTypeNames.Application.CompanyName + Path.DirectorySeparatorChar + "CIP3";
                string claureg = "Software" + Path.DirectorySeparatorChar + nomCompanyia + Path.DirectorySeparatorChar + "CIP3";
                string x = Comuns.Utilitats.LlegeixVariableRegistre(Microsoft.Win32.Registry.CurrentUser, claureg, "Lic");

                if (x == "2")
                    throw new UtilitatsException(Errors.LlicenciaCaducada); //MediaTypeNames.Application.Exit();
                else
                {
                    //DateTime dt = new DateTime(2015, 04, 10, 0, 0, 0);
                    DateTime dataAvui;
                    try
                    {
                        var s = Comuns.Utilitats.HoraInternet();
                        dataAvui = s;
                    }
                    catch
                    {
                        dataAvui = DateTime.Now;
                    }

                    if (dataAvui > data)
                    {
                        Comuns.Utilitats.GravaVariableRegistre(Microsoft.Win32.Registry.CurrentUser, claureg, "Lic", "2");
                        throw new UtilitatsException(Errors.LlicenciaCaducada); //Comuns.Utilitats.Missatge("Llicència caducada", true, false, true);
                        //MediaTypeNames.Application.Exit();
                    }
                }
            }
            catch
            {
            }
        }
    }
}