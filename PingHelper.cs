using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.NetworkInformation;
using System.ComponentModel;
using System.Threading;
using System.Collections;
using System.Windows.Threading;
using System.Net.Sockets;
using System.Diagnostics;
using System.Windows;

namespace rotmg_latency_tester
{
    class PingHelper
    {
        public static List<double> Ping(String IP)
        {
            /*
             * Because ROTMG servers have ICMP blocked for security,
             * we perform a TCP ping using Socket.
             */

            List<double> times = new List<double>();

            try { 
                for (int i = 0; i < 4; i++)
                {
                    Stopwatch timer = new Stopwatch();
                    Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
                    {
                        Blocking = true
                    };

                    timer.Start();
                    socket.Connect(IP, 2050);
                    timer.Stop();

                    double time = timer.Elapsed.TotalMilliseconds;
                    times.Add(time);

                    socket.Close();

                    Thread.Sleep(25);
                }
            }
            catch
            {
                MessageBox.Show("An error occurred while pinging a server. Please restart the program or try again later.", "ROTMG Latency Tester", MessageBoxButton.OK, MessageBoxImage.Error);
                Environment.Exit(1);
            }
            return times; 
        }
    }
}