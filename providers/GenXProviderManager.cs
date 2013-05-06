﻿using System;
using System.Collections.Generic;
using System.Xml;
using NBrightCore.TemplateEngine;

namespace NBrightCore.providers
{
    public class GenXProviderManager
    {

        static GenXProviderManager()
	    {
            Initialize();
	    }

        /// <summary>
        /// Returns the default configured data provider
        /// </summary>
        public static GenXProvider Default { get; private set; }

        public static Dictionary<string, GenXProvider> ProviderList { get; private set; }

        private static GenXProvider CreateProvider(string providerAssembyClass)
        {
            if (!string.IsNullOrEmpty(providerAssembyClass))
            {
                var prov = providerAssembyClass.Split(Convert.ToChar(","));
                var handle = Activator.CreateInstance(prov[0], prov[1]);
                return (GenXProvider)handle.Unwrap();
            }
            return null;
        }

        [Obsolete("AddProvider is deprecated,Use AddProvider(string providerAssembyClass) instead", true)]
        public static void AddProvider(int ProviderKey, string providerAssembyClass)
        {
            //providerkey param kept for backward compatiblity, we now use the assembly class as the key, to stop clashes.
            AddProvider(providerAssembyClass);
        }

        public static void AddProvider(string providerAssembyClass)
        {
            if (!ProviderList.ContainsKey(providerAssembyClass))
            {
                ProviderList.Add(providerAssembyClass, CreateProvider(providerAssembyClass));                
            }
        }

        public static void AddProvider(XmlDataDocument xmlProviderList)
        {
            if (xmlProviderList != null)
            {
                var nodList = xmlProviderList.SelectNodes("root/providers/*");
                if (nodList != null)
                {
                    foreach (XmlNode nod in nodList)
                    {
                        AddProvider(nod.InnerText);
                    }
                }
            }
        }


        /// <summary>
        /// Reads the configuration related to the set of configured 
        /// providers and sets the default and collection of providers and settings.
        /// </summary>
        private static void Initialize()
        {
            try
            {
                // Hardcode DNN extension provider 
                var providerConfigXml = "<root><providers><genx>NBrightDNN,NBrightDNN.render.GenXmlTemplateExt</genx></providers></root>";
                try
                {
                    var xmlDoc = new XmlDataDocument();
                    xmlDoc.LoadXml(providerConfigXml);

                    var xmlList = xmlDoc.SelectNodes("/root/providers/genx");
                    if (xmlList != null)
                    {
                        ProviderList = new Dictionary<String, GenXProvider>();
                        var lp = 0;
                        foreach (XmlNode xNod in xmlList)
                        {
                            ProviderList.Add(xNod.InnerText, CreateProvider(xNod.InnerText));
                            //set default as first in list
                            if (lp == 0) Default = ProviderList[xNod.InnerText];
                            lp = lp + 1;
                        }
                    }
                }
                catch (Exception e)
                {
                    // maybe not be setup, so ignore
                }

            }
            catch (Exception)
            {
                // CMS portal root does not exists if ran from scheduler. So ignore
            }
        }

    }
}
