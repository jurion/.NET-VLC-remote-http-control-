using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Windows.Storage;

namespace VlcRemotePhone.Settings
{
    public sealed class VlcConnectSettings
    {
        private const string IpAdressKey = "IpAdress";
        private const string PortKey = "Port";
        private const string PasswordKey = "Password";

        private string ipAdress = "";
        private int port = 0;
        private string password = "";
        private bool isLoadedFromStorage = true;

        public VlcConnectSettings()
        {
            var settings = GetSettings();
            if (settings.Values.ContainsKey(IpAdressKey))
            {
                this.ipAdress = settings.Values[IpAdressKey].ToString();
            }
            else
            {
                this.IpAdress = "192.168.2.2";
                this.IsLoadedFromStorage = false;
            }
            if (settings.Values.ContainsKey(PortKey))
            {
                this.port = int.Parse(settings.Values[PortKey].ToString());
            }
            else
            {
                this.Port = 8080;
                this.IsLoadedFromStorage = false;
            }
            if (settings.Values.ContainsKey(PasswordKey))
            {
                this.password = settings.Values[PasswordKey].ToString();
            }
            else
            {
                this.Password = "jurik";
                this.IsLoadedFromStorage = false;
            }
        }

        public string GetUrl()
        {
            return "http://" + this.ipAdress + ":" + this.Port.ToString() + "/";
        }


        public string IpAdress
        {
            get
            {
                return ipAdress;
            }

            set
            {
                if (value != ipAdress)
                {
                    ipAdress = value;
                    var settings = GetSettings();
                    settings.Values[IpAdressKey] = value;
                }
            }
        }

        public int Port
        {
            get
            {
                return port;
            }

            set
            {
                if (value != port)
                {
                    port = value;
                    var settings = GetSettings();
                    settings.Values[PortKey] = value.ToString();
                }

            }
        }

        public string Password
        {
            get
            {
                return password;
            }

            set
            {
                if (value != password)
                {
                    password = value;
                    var settings = GetSettings();
                    settings.Values[PasswordKey] = value;
                }

            }
        }

        public bool IsLoadedFromStorage
        {
            get
            {
                return isLoadedFromStorage;
            }

            set
            {
                isLoadedFromStorage = value;
            }
        }


        private ApplicationDataContainer GetSettings()
        {
            return Windows.Storage.ApplicationData.Current.RoamingSettings;
        }
    }
}
