using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Xml;

namespace rotmg_latency_tester
{
    class ServerParser
    {
        public static List<MainWindow.Server> GetServers()
        {
            List<MainWindow.Server> servers = new List<MainWindow.Server>();

            String URLString = "https://www.realmofthemadgod.com/char/list";

            XmlDocument document = new XmlDocument();
            document.Load(URLString);
            XmlElement root = document.DocumentElement;
            XmlNodeList nodes = root.SelectNodes("/Chars/Servers/Server");

            foreach (XmlNode node in nodes)
            {
                servers.Add(new MainWindow.Server() { Name = node["Name"].InnerText, IP = node["DNS"].InnerText, Usage = node["Usage"].InnerText });
            }
            return servers;
        }
    }
}
