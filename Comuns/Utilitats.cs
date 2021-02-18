using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Data.Entity.Core.EntityClient;
using System.Data.Entity.Validation;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Management;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Comuns.Properties;
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
        #region *** Utilitats1 ***

        public enum Errors
        {
            LongitudAdrecaMacIncorrecta,
            LlicenciaCaducada
        }

        public enum Monedes
        {
            EUR,
            USD
        }


        public static FileInfo _FitxerLog
        {
            get { return new FileInfo(ConverteixVariablesEntornDeCadena(ConfigurationManager.AppSettings["FitxerLog"])); }
        }

        public static DateTime DataBd(DbContext dbContext)
        {
            return dbContext.Database.SqlQuery<DateTime>("Select GetDate()").First();
        }


        public static List<string> NumSerieHd()
        {
            List<string> numS = new List<string>();

            ManagementObjectSearcher searcher = new ManagementObjectSearcher("select * from Win32_DiskDrive");
            foreach (ManagementObject share in searcher.Get())
            {
                try
                {
                    string numSerie = (string) share.Properties["SerialNumber"].Value;
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
        private static List<string> NumMac()
        {
            List<string> numS = new List<string>();

            ManagementObjectSearcher searcher = new ManagementObjectSearcher("select * from Win32_NetworkAdapter");
            foreach (ManagementObject share in searcher.Get())
            {
                try
                {
                    string numSerie = (string) share.Properties["MACAddress"].Value;
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
            if (mac == null)
                return null;
            if (mac.Length != 17)
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

            MethodInfo preserveStackTrace = typeof (Exception).GetMethod("InternalPreserveStackTrace",
                BindingFlags.Instance | BindingFlags.NonPublic);
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
        /// Crea la clau per gravar dades en el registre de Windows
        /// </summary>
        /// <returns></returns>
        public static string CreaClauRegistre()
        {
            // *** Crea la clau per gravar el registre de Windows. ***
            var versionInfo = FileVersionInfo.GetVersionInfo(Assembly.GetEntryAssembly().Location);
            var companyName = versionInfo.CompanyName;
            var productName = versionInfo.ProductName;
            return "Software" + Path.DirectorySeparatorChar + companyName + Path.DirectorySeparatorChar + productName;
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
                string x = LlegeixVariableRegistre(Registry.CurrentUser, claureg, "Lic");

                if (x == "2")
                    throw new UtilitatsException(Errors.LlicenciaCaducada); //MediaTypeNames.Application.Exit();
                else
                {
                    //DateTime dt = new DateTime(2015, 04, 10, 0, 0, 0);
                    DateTime dataAvui;
                    try
                    {
                        var s = HoraInternet();
                        dataAvui = s;
                    }
                    catch
                    {
                        dataAvui = DateTime.Now;
                    }

                    if (dataAvui > data)
                    {
                        GravaVariableRegistre(Registry.CurrentUser, claureg, "Lic", "2");
                        throw new UtilitatsException(Errors.LlicenciaCaducada); //Comuns.Utilitats.Missatge("Llicència caducada", true, false, true);
                        //MediaTypeNames.Application.Exit();
                    }
                }
            }
            catch
            {
            }
        }

        #endregion   *** Utilitats1 ***


        #region *** Utilitats2 ***

        [DllImport("user32.dll")]
        public static extern void SwitchToThisWindow(IntPtr hWnd, bool fAltTab);


        /// <summary>
        /// Torna la versio de la dll.
        /// </summary>
        /// <param name="dllType">És Type de la dll que es vol la versió.</param>
        /// <returns></returns>
        public static Version VersioDll(Type dllType)
        {
            return Assembly.GetAssembly(dllType).GetName().Version;
        }

        /// <summary>
        /// Torna el directori on està l'assembly.
        /// </summary>
        /// <param name="dllType"></param>
        /// <returns></returns>
        public static string DirectoriAssembly(Type dllType)
        {
            return Path.GetDirectoryName(dllType.Assembly.Location);
        }


        /// <summary>
        /// Substitueix DesignMode que no funciona en subcontrols.
        /// </summary>
        /// <returns></returns>
        public static bool IsInDesignMode()
        {
            return Application.ExecutablePath.IndexOf("devenv.exe", StringComparison.OrdinalIgnoreCase) > -1;
        }


        public static void MostraFinestraAmbError(Exception ex)
        {
            string msg;
            if (ex.InnerException == null)
                msg = ex.Message;
            else
                msg = ex.Message + Environment.NewLine + ex.InnerException.Message;

            MessageBox.Show(msg, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }


        public static Exception ExtreuInnerException(Exception ex)
        {
            if (ex.InnerException == null)
                return ex;
            else
                 return ExtreuInnerException(ex.InnerException);
        }



        /// <summary>
        /// Torna la data del dia anterior laborable.
        /// No té calendari de festius, només te en compte dissabtes i diumenges.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static DateTime AnteriorDiaLaborable(DateTime data)
        {
            DateTime dataAnt = data;
            do
            {
                dataAnt = dataAnt.AddDays(-1);

            } while (dataAnt.DayOfWeek == DayOfWeek.Saturday || dataAnt.DayOfWeek == DayOfWeek.Sunday);

            return dataAnt;
        }

        /// <summary>
        /// Genera string de connexió per a Entity Framework.
        /// </summary>
        /// <param name="nomServidor"></param>
        /// <param name="nomBaseDades"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        public static string ConnString(string nomServidor, string nomBaseDades, string model)
        {
            return ConnString(nomServidor, nomBaseDades, model, "UsuariNgloba", "340$Uuxwp2Mcxo9$Khy");
        }

        /// <summary>
        /// Genera string de connexió per a Entity Framework.
        /// </summary>
        /// <param name="nomServidor"></param>
        /// <param name="nomBaseDades"></param>
        /// <param name="model"></param>
        /// <param name="usuari"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public static string ConnString(string nomServidor, string nomBaseDades, string model, string usuari, string password)
        {
            // Initialize the connection string builder for the
            // underlying provider.
            SqlConnectionStringBuilder sqlBuilder = new SqlConnectionStringBuilder();

            // Set the properties for the data source.
            sqlBuilder.DataSource = nomServidor;
            sqlBuilder.InitialCatalog = nomBaseDades;

            #region ***** Connexió amb i sense UsuariNgloba/password *****

            // Usuari/password SqlServer: sa/qwerty
            //sqlBuilder.IntegratedSecurity = true; // No necessita Usuari/Password
            sqlBuilder.IntegratedSecurity = false; // false si vull connectar amb un usuari determinat.
            sqlBuilder.UserID = usuari;
            sqlBuilder.Password = password;

            #endregion ***** Connexió amb i sense usuari/password *****


            // Initialize the EntityConnectionStringBuilder.
            EntityConnectionStringBuilder entityBuilder = new EntityConnectionStringBuilder();

            //Set the provider name.
            entityBuilder.Provider = "System.Data.SqlClient";

            // Set the provider-specific connection string.
            entityBuilder.ProviderConnectionString = sqlBuilder.ToString();

            // Set the Metadata location.
            //entityBuilder.Metadata = @"res://*/Model2.csdl|res://*/Model2.ssdl|res://*/Model2.msl";
            entityBuilder.Metadata = String.Format(@"res://*/{0}.csdl|res://*/{0}.ssdl|res://*/{0}.msl", model);

            return entityBuilder.ToString();
        }

        public static string TitolFinestra(Form form, string nomModul)
        {
            return String.Format(" -{3}- {0}. {1}={2}", nomModul, Resources.Versio, Assembly.GetAssembly(form.GetType()).GetName().Version, form.Text);
        }

        /// <summary>
        /// Comprova si ctrl o un del controls que hi penjen, tenen el focus.
        /// </summary>
        /// <param name="control"></param>
        /// <returns></returns>
        public static bool TeFocus(Control control)
        {
            // **** El codi comentat fa el mateix que l'implementat. ****
            //if(control == null)
            //    return false;
            //foreach (Control ctrl in control.Controls)
            //{
            //    if (teFocus(ctrl))
            //        return true;
            //}
            //return control.Focused;

            return control != null && (control.Controls.Cast<Control>().Any(TeFocus) || control.Focused);
        }

        /// <summary>
        /// Comprova si la expresio conté un valor numèric
        /// </summary>
        /// <param name="expresio"></param>
        /// <returns></returns>
        public static bool EsNumeric(string expresio)
        {
            double retNum;
            return Double.TryParse(Convert.ToString(expresio), NumberStyles.Any, NumberFormatInfo.InvariantInfo, out retNum);
        }


        #region Compara cadenes. Per camps string del ERP.

        /// <summary>
        /// Compara dues cadenes, elimina espais i ignora majuscules per defecte.
        /// </summary>
        /// <param name="s1"></param>
        /// <param name="s2"></param>
        /// <param name="stringComp"></param>
        /// <returns></returns>
        public static bool SonIguals(string s1, string s2, StringComparison stringComp = StringComparison.CurrentCultureIgnoreCase)
        {
            if (s1 != null)
                s1 = s1.Trim();

            if (s2 != null)
                s2 = s2.Trim();

            return String.Equals(s1, s2, stringComp);
        }

        /// <summary>
        /// Comprova si els números son iguals eliminant els decimals que queden més enlla de la tolerància.
        /// </summary>
        /// <param name="numero1"></param>
        /// <param name="numero2"></param>
        /// <param name="decimalsTolerància"></param>
        /// <returns></returns>
        public static bool SonIguals(double numero1, double numero2, uint decimalsTolerància = 5)
        {
            return EsZero(numero1 - numero2, decimalsTolerància);
        }

        /// <summary>
        /// Compara numero1 i numero2 eliminant la tolerància.
        /// </summary>
        /// <param name="numero1"></param>
        /// <param name="numero2"></param>
        /// <param name="decimalsTolerància"></param>
        /// <returns>0 si son iguals. -1 si numero1 és més petit. 1 si numero1 és més gran.</returns>
        public static int ComparaNumeros(double numero1, double numero2, uint decimalsTolerància = 5)
        {
            return SonIguals(numero1, numero2, decimalsTolerància) ? 0 : numero1.CompareTo(numero2);
        }


        /// <summary>
        /// Comprova si un número és zero eliminant els decimals que queden més enlla de la tolerància.
        /// </summary>
        /// <param name="numero">Número a comprovar.</param>
        /// <param name="decimalsTolerància">Son els decimals que es tintran en compte. Si = 0 només es compara la part entera.</param>
        /// <returns></returns>
        public static bool EsZero(double numero, uint decimalsTolerància = 5)
        {
            var tolerancia = Math.Pow(10, -decimalsTolerància);
            var result = Math.Abs(numero) < tolerancia;
            return result;
        }


        /// <summary>
        /// Forma DateTime a partir dels paràmetres data i hora.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="hora">Si null, posa la hora actual.</param>
        /// <returns></returns>
        public static DateTime FormaData(DateTime data, TimeSpan? hora)
        {
            return data.Date + (hora.HasValue ? hora.Value : DateTime.Now.TimeOfDay);
        }


        /// <summary>
        /// Si dataFinal és d'avui i la hora és 0 poso hora actual, si és null torno MaxValue, sino no faig res
        /// </summary>
        /// <param name="dataFinal">Si null, torno DateTime.MaxValue</param>
        /// <returns></returns>
        public static DateTime PosoHora(DateTime? dataFinal)
        {
            DateTime data;

            if (!dataFinal.HasValue)
                return DateTime.MaxValue;

            if (dataFinal.Value.Date == DateTime.Today)
            {
                // Data d'avui si hora és 0 poso l'hora actual, sino conservo la que té.
                data = dataFinal.Value.Second > 0 ? dataFinal.Value : DateTime.Now;
            }
            else
                // Data no és d'avui, poso hora final dia.
                data = DataHoraFinalDia(dataFinal.Value);

            return data;
        }


        /// <summary>
        /// Posa la hora 00:00:00 a la data.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static DateTime DataHoraIniciDia(DateTime data)
        {
            return data.Date;
        }

        /// <summary>
        /// Posa la hora 23:59:59 a la data.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static DateTime DataHoraFinalDia(DateTime data)
        {
            // return  data < DateTime.MaxValue ? data.Date.AddDays(1).AddTicks(-1) : DateTime.MaxValue;
            return data.Date + DateTime.MaxValue.TimeOfDay;
        }

        /// <summary>
        /// Torna la data hora de l'últim milisegon de l'any.
        /// </summary>
        /// <returns></returns>
        public static DateTime DataHoraFinalAny(int any)
        {
            return DataHoraFinalDia(new DateTime(any, 12, 31));
        }

        /// <summary>
        /// Comprova si s1 conté s2. Elimina espais de s2. No té en compte majúscules per defecte.
        /// </summary>
        /// <param name="s1"></param>
        /// <param name="s2"></param>
        /// <param name="stringComp"></param>
        /// <returns></returns>
        public static bool ConteString(string s1, string s2, StringComparison stringComp = StringComparison.CurrentCultureIgnoreCase)
        {
            if (String.Equals(s1, s2, StringComparison.CurrentCulture))
                return true;

            if (String.IsNullOrWhiteSpace(s1) || String.IsNullOrWhiteSpace(s2))
                return false;

            return s1.IndexOf(s2.Trim(), stringComp) >= 0;
        }


        /// <summary>
        /// Extreu un valor de una cadena.
        /// </summary>
        /// <param name="stringConnexio"></param>
        /// <param name="p"></param>
        /// <returns></returns>
        public static string ExtreuValor(string stringConnexio, string p)
        {
            string valor = null;
            foreach (string va in stringConnexio.Split(';').Where(va => va.IndexOf(p, StringComparison.OrdinalIgnoreCase) >= 0))
            {
                valor = va.Remove(0, p.Length);
            }

            return valor;
        }

        #endregion


        #region Llegeix fitxer configuració

        /// <summary>
        /// Llegeix una clau del fitxer en execució.
        /// </summary>
        /// <param name="clau"></param>
        /// <returns></returns>
        public static string LlegeixConfig(string clau)
        {
            try
            {
                return ConfigurationManager.AppSettings[clau];
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Llegeix una clau del fitxer .config
        /// </summary>
        /// <param name="type">DLL de la que colem llegir el fitxer config..</param>
        /// <param name="clau">Clau a llegir</param>
        /// <returns></returns>
        public static string LlegeixConfig(Type type, string clau)
        {
            return LlegeixConfig(type.Assembly.Location, clau);
        }

        /// <summary>
        /// Llegeix una clau del fitxer .config
        /// </summary>
        /// <param name="pathFitxerConfig">Path nom del fitxer config.</param>
        /// <param name="clau">Clau a llegir</param>
        /// <returns></returns>
        public static string LlegeixConfig(string pathFitxerConfig, string clau)
        {
            ////Open the configuration file using the dll location
            Configuration myDllConfig = ConfigurationManager.OpenExeConfiguration(pathFitxerConfig);

            //// Get the appSettings section
            AppSettingsSection myDllConfigAppSettings = (AppSettingsSection) myDllConfig.GetSection("appSettings");

            var llistaElements = myDllConfigAppSettings.Settings[clau];
            return llistaElements == null ? null : llistaElements.Value;
        }

        #endregion


        #region Tracta fitxer log

        /// <summary>
        /// Torna el fitxer log d'una DLL
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static FileInfo LlegeixFitxerLog(Type type)
        {
            FileInfo fitxerLog = null;

            try
            {
                fitxerLog = new FileInfo(LlegeixConfig(type, "FitxerLog"));
            }
            catch
            {
                // Ho deixo pel finally
            }
            finally
            {
                if (fitxerLog == null || fitxerLog.Directory == null || !fitxerLog.Directory.Exists)
                    fitxerLog = new FileInfo(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Ngloba.log"));
            }

            return fitxerLog;
        }

        /// <summary>
        /// Torna el fitxer log de Ngloba. 
        /// </summary>
        /// <returns></returns>
        public static FileInfo LlegeixFitxerLog()
        {
            return LlegeixFitxerLog(NomProcesActual() + ".log");
        }

        /// <summary>
        /// Torna el fitxer log del fitxer en execució.
        /// </summary>
        /// <returns></returns>
        public static FileInfo LlegeixFitxerLog(string nomFitxer)
        {
            FileInfo fitxerLog = null;

            try
            {
                fitxerLog = _FitxerLog;
            }
            catch
            {
                // Ho deixo pel finally
            }
            finally
            {
                if (fitxerLog == null || fitxerLog.Directory == null || !fitxerLog.Directory.Exists)
                    fitxerLog = new FileInfo(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), nomFitxer));
            }

            return fitxerLog;
        }


        /// <summary>
        /// Escriu en el fitxer log i mostra finestra al usuari.
        /// </summary>
        /// <param name="ex"></param>
        /// <param name="fitxerLog"></param>
        /// <param name="versio"></param>
        public static void EscriuLog(Exception ex, FileInfo fitxerLog, Version versio)
        {
            EscriuLog(ex, UltimaInnerExceptio(ex).Message, fitxerLog, versio, true, true);
        }

        private static Exception UltimaInnerExceptio(Exception ex)
        {
            return ex.InnerException == null ? ex : UltimaInnerExceptio(ex.InnerException);
        }


        /// <summary>
        /// Escriu en el fitxer log sense mostrar cap finestra al usuari.
        /// </summary>
        /// <param name="ex"></param>
        /// <param name="mostraFinestra">True mostrara la finestra amb l'error.</param>
        /// <param name="preguntaPerObrirLog">True donarà l'opció d'obrir el log.</param>
        public static FileInfo EscriuLog(Exception ex, bool mostraFinestra = false, bool preguntaPerObrirLog = false)
        {
            string missatge = mostraFinestra ? ex.Message : null;
            var log = EscriuLog(ex, missatgeFinestra: missatge, fitxerLog: _FitxerLog, versio: null);

            if (preguntaPerObrirLog && log != null && log.Exists)
            {
                if (MessageBox.Show("Vols obrir fitxer log?", "Avís", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    Process.Start(log.FullName);
                }
            }
            return log;
        }

        public static FileInfo EscriuLog(string missatge)
        {
            return EscriuLog(missatge, fitxerLog: _FitxerLog);
        }

        /// <summary>
        /// Escriu en el fitxer log sense mostrar cap finestra al usuari.
        /// </summary>
        /// <param name="ex">Excepció que s'escriurà en el log.</param>
        /// <param name="missatgeFinestra">Missatge que es mostrarà al usuari en una finestra. Si null, no es mostra res.</param>
        /// <param name="fitxerLog"></param>
        /// <param name="versio"></param>
        /// <param name="mostraData"></param>
        /// <param name="mostraTraça"></param>
        /// <param name="esInnerException"></param>
        /// <returns></returns>
        public static FileInfo EscriuLog(Exception ex, string missatgeFinestra, FileInfo fitxerLog, Version versio, bool mostraData = true, bool mostraTraça = false, bool esInnerException = false)
        {
            string missatge = String.Empty;
            FileInfo log = fitxerLog;

            if (esInnerException)
                missatge += "InnerException: ";

            missatge += ex.GetType() + Environment.NewLine;

            //missatge += "HResult==" + ex.HResult + Environment.NewLine;
            missatge += "Message=" + ex.Message + Environment.NewLine;
            missatge += "Source=" + ex.Source + Environment.NewLine;

            if (mostraTraça)
            {
                missatge += "StackTrace:" + Environment.NewLine + ex.StackTrace;
            }


            // Fa que la finestra es mostri després d'haver gravat el fitxer log.
            var missFin = ex.InnerException == null ? missatgeFinestra : null;

            if (ex.InnerException == null)
            {

                DbEntityValidationException ex2 = ex as DbEntityValidationException;
                if (ex2 != null)
                {
                    if (ex2.EntityValidationErrors.Any())
                        missatge += "\nEntityValidationErrors:";

                    foreach (var err2 in ex2.EntityValidationErrors.SelectMany(err => err.ValidationErrors))
                    {
                        missatge += String.Format("\nPropietat: {0}. Error: {1}", err2.PropertyName, err2.ErrorMessage);
                    }
                }

                log = EscriuLog(missatge, missFin, fitxerLog, mostraData, versio);
            }

            else
            {
                // Crida recursiva
                log = EscriuLog(ex.InnerException, missatgeFinestra, fitxerLog, versio, false, mostraTraça, true);
            }

            return log;
        }


        //public static void EscriuLog(DbEntityValidationException ex, string missatgeFinestra, FileInfo fitxerLog, Version versio)
        //{
        //    bool mostraData = true;
        //    foreach (var eve in ex.EntityValidationErrors)
        //    {
        //        string xx = String.Format("Entity of type '{0}' in state '{1}' has the following validation errors:",
        //            eve.Entry.Entity.GetType().Name, eve.Entry.State);
        //        EscriuLog(xx, missatgeFinestra, fitxerLog, mostraData, versio);
        //        missatgeFinestra = null;
        //        mostraData = false;
        //        foreach (var ve in eve.ValidationErrors)
        //        {
        //            xx = String.Format("- Property: \"{0}\", Error: \"{1}\"", ve.PropertyName, ve.ErrorMessage);

        //            EscriuLog(xx, fitxerLog);
        //        }
        //    }
        //}


        /// <summary>
        /// Escriu en el fitxer log sense mostrar cap finestra al usuari.
        /// </summary>
        /// <param name="text">Text que s'escriurà.</param>
        /// <param name="fitxerLog"></param>
        /// <param name="versio"></param>
        public static FileInfo EscriuLog(string text, FileInfo fitxerLog)
        {
            return EscriuLog(text, null, fitxerLog, false, null);
        }

        /// <summary>
        /// Escriu en el fitxer log.
        /// </summary>
        /// <param name="text">Text que s'escriurà.</param>
        /// <param name="missatgeFinestra">Missatge que es mostrarà al usuari en una finestra. Si null, no es mostra res.</param>
        /// <param name="fitxerLog">Si null, només mostra finestra.</param>
        /// <param name="mostraData"></param>
        /// <param name="versio"></param>
        public static FileInfo EscriuLog(string text, string missatgeFinestra, FileInfo fitxerLog, bool mostraData, Version versio)
        {
            if (fitxerLog != null)
            {
                using (StreamWriter w = fitxerLog.AppendText())
                {
                    if (mostraData)
                    {
                        //w.WriteLine(Environment.NewLine + "--- Error. " + DateTime.Now.ToLongDateString() + " " + DateTime.Now.ToLocalTime());
                        w.WriteLine(Environment.NewLine + String.Format("--- Error. {0} {1}. Versión: {2}", DateTime.Now.ToLongDateString(), DateTime.Now.ToLocalTime(), versio));
                    }
                    w.WriteLine(text);
                }

                if (missatgeFinestra != null)
                    MessageBox.Show(missatgeFinestra + "\nConsultar el fichero: " + fitxerLog.FullName, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                    missatgeFinestra += "\n" + text;

                if (missatgeFinestra != null)
                    MessageBox.Show(missatgeFinestra, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return fitxerLog;
        }

        #endregion


        public static DateTime ArrodoneixoDataASegons(DateTime dataHora)
        {
            return new DateTime(dataHora.Year, dataHora.Month, dataHora.Day, dataHora.Hour, dataHora.Minute, dataHora.Second, 0);
        }

        /// <summary>
        /// Comprova "nomInstancia" ja està en marxa.
        /// </summary>
        /// <param name="nomInstancia"></param>
        /// <returns></returns>
        public static bool ComprovaSiHiHaUnaInstanciaEnMarxa(string nomInstancia)
        {
            bool noHiHaInstanciaEnMarxa;
            using (Mutex mutex = new Mutex(true, "Inversions", out noHiHaInstanciaEnMarxa)) { }
            return !noHiHaInstanciaEnMarxa;
        }

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetForegroundWindow(IntPtr hWnd);


        /// <summary>
        /// Si la mateixa aplicació ja està en marxa l'enfoca.
        /// </summary>
        /// <param name="current"></param>
        public static void ActivaInstanciaEnMarxa(Process current)
        {
            foreach (Process process in Process.GetProcessesByName(current.ProcessName))
            {
                if (process.Id != current.Id)
                {
                    SetForegroundWindow(process.MainWindowHandle);
                    break;
                }
            }
        }


        /// <summary>
        /// Torna el nom del procés actual, si executo des de Visual Studio, elimino ".VSHOST".
        /// </summary>
        /// <returns></returns>
        public static string NomProcesActual()
        {
            try
            {
                var nomProces = Process.GetCurrentProcess().ProcessName;
                return nomProces.Replace(".VSHOST", "");
            }
            catch (Exception)
            {
                return null;
            }
        }


        /// <summary>
        /// Si en text hi ha variables d'entorn (entre '%') les converteix al seu valor real.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string ConverteixVariablesEntornDeCadena(string text)
        {
            if (text == null)
                return null;

            var array = text.Split('%');

            if (array.Length == 1)
                return array[0];

            string resultat = "";
            if (array.Length > 1)
            {
                bool esVariableEntorn = false;
                foreach (var elem in array)
                {
                    resultat += esVariableEntorn ? Environment.GetEnvironmentVariable(elem) : elem;
                    esVariableEntorn = !esVariableEntorn;
                }
            }
            return resultat;
        }

        #endregion *** Utilitats2 ***
    }
}