using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace rotmg_latency_tester
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public delegate void NoArgDelegate();
        public delegate void ProgBarSetDelegate(int arg);

        public List<Server> _Servers = new List<Server>();
        public List<Server> Servers { get { return _Servers; } }

        private List<Label> pingLabels = new List<Label>();
        private List<Label> jitterLabels = new List<Label>();

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            NoArgDelegate fetcher = new NoArgDelegate(this.GetServers);
            fetcher.BeginInvoke(null, null);
        }

        public class Server
        {
            public string Name { get; set; }
            public string IP { get; set; }
            public string Usage { get; set; }
            public List<double> Ping { get; set; }
        }

        private void GetServers()
        {
            Task<List<Server>> getServers = Task<List<Server>>.Factory.StartNew(() => ServerParser.GetServers());
            _Servers = getServers.Result;

            Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new NoArgDelegate(SetGridRows));
            Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal,new ProgBarSetDelegate(UpdateProgressBar),10);

            foreach (Server server in Servers)
            {
                server.Ping = PingHelper.Ping(server.IP);
                Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new NoArgDelegate(UpdateProgressBar));
            }

            _Servers = Servers.OrderBy(o => o.Ping.Average()).ToList();

            Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new NoArgDelegate(SetGridText));
            Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new NoArgDelegate(AnimateAfterLoad));
        }

        private void SetGridRows()
        {
            for (int i = 0; i < 2; i++)
            {
                serverListPing.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(serverListPing.Width / 2) });
                serverListJitter.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(serverListPing.Width / 2) });
            }

            int gridRows = (int)(Math.Ceiling((double)(Servers.Count / 2))) + 1;

            for (int i = 0; i < gridRows; i++)
            {
                serverListPing.RowDefinitions.Add(new RowDefinition { Height = new GridLength(serverListPing.Height / gridRows) });
                serverListJitter.RowDefinitions.Add(new RowDefinition { Height = new GridLength(serverListPing.Height / gridRows) });
            }
        }

        private void SetGridText()
        {
            for (int i = 0; i < Servers.Count; i++)
            {
                Server server = Servers[i];
                int gridRows = (int)(Math.Ceiling((double)(Servers.Count / 2)));
                double ping = Math.Round(server.Ping.Average(), 2);
                double jitter = Math.Round(server.Ping.Max() - server.Ping.Min(),2);

                Label newPingLabel = new Label
                {
                    Name = server.Name,
                    Content = server.Name + ": " + ping + "ms",
                    Width = serverListPing.Width / 2,
                    Height = serverListPing.Height / gridRows,
                    VerticalContentAlignment = VerticalAlignment.Center,
                    FontSize = 16
                };

                Label newJitterLabel = new Label
                {
                    Name = server.Name,
                    Content = server.Name + ": " + jitter + "ms",
                    Width = serverListPing.Width / 2,
                    Height = serverListPing.Height / gridRows,
                    VerticalContentAlignment = VerticalAlignment.Center,
                    FontSize = 16
                };

                int col = i % 2;
                int row = (int)Math.Floor((double)(i / 2));

                if (col == 1)
                {
                    newPingLabel.HorizontalContentAlignment = HorizontalAlignment.Right;
                    newJitterLabel.HorizontalContentAlignment = HorizontalAlignment.Right;
                }
                else
                {
                    newPingLabel.HorizontalContentAlignment = HorizontalAlignment.Left;
                    newJitterLabel.HorizontalContentAlignment = HorizontalAlignment.Left;
                }

                if (ping > 300.00)
                    newPingLabel.Foreground = new SolidColorBrush(Colors.IndianRed);
                else if (ping > 150.00)
                    newPingLabel.Foreground = new SolidColorBrush(Colors.Khaki);
                else
                    newPingLabel.Foreground = new SolidColorBrush(Colors.LightGreen);

                if (jitter > 50.00)
                    newJitterLabel.Foreground = new SolidColorBrush(Colors.IndianRed);
                else if (jitter > 15.00)
                    newJitterLabel.Foreground = new SolidColorBrush(Colors.Khaki);
                else
                    newJitterLabel.Foreground = new SolidColorBrush(Colors.LightGreen);

                pingLabels.Add(newPingLabel);
                jitterLabels.Add(newJitterLabel);

                Grid.SetColumn(newPingLabel, col);
                Grid.SetRow(newPingLabel, row);
                Grid.SetColumn(newJitterLabel, col);
                Grid.SetRow(newJitterLabel, row);

                serverListPing.Children.Add(newPingLabel);
                serverListJitter.Children.Add(newJitterLabel);
            }
        }

        private void DeleteLabels()
        {
            foreach (Label label in pingLabels)
            {
                serverListPing.Children.Remove(label);
            }
            foreach (Label label in jitterLabels)
            {
                serverListJitter.Children.Remove(label);
            }
        }

        private void UpdateProgressBar(int Progress)
        {
            progBar.Value = Progress;
        }

        private void UpdateProgressBar()
        {
            progBar.Value += 90 / Math.Ceiling(Convert.ToDouble(Servers.Count));
        }

        private void AnimateAfterLoad()
        {
            Spinner.Visibility = System.Windows.Visibility.Visible;
            progBar.Visibility = System.Windows.Visibility.Visible;

            var fadeAnim = new DoubleAnimation
            {
                From = 1.0,
                To = 0.0,
                FillBehavior = FillBehavior.Stop,
                BeginTime = TimeSpan.FromSeconds(0),
                Duration = new Duration(TimeSpan.FromSeconds(0.5))
            };
            
            var storyboard = new Storyboard();

            storyboard.Children.Add(fadeAnim);
            Storyboard.SetTarget(fadeAnim, Spinner);
            Storyboard.SetTarget(fadeAnim, progBar);
            Storyboard.SetTargetProperty(fadeAnim, new PropertyPath(OpacityProperty));

            storyboard.Completed += delegate { Spinner.Visibility = System.Windows.Visibility.Hidden; progBar.Visibility = System.Windows.Visibility.Hidden; };
            storyboard.Begin();

            serverListTabsBlur.Radius = 0.0;
            serverListTabs.IsEnabled = true;
            buttonRefresh.IsEnabled = true;

            Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new ProgBarSetDelegate(UpdateProgressBar), 0); // reset prog bar
        }

        private void AnimateRefresh()
        {
            buttonRefresh.IsEnabled = false;
            Spinner.Visibility = System.Windows.Visibility.Hidden;
            progBar.Visibility = System.Windows.Visibility.Hidden;

            var fadeAnim = new DoubleAnimation
            {
                From = 0.0,
                To = 1.0,
                FillBehavior = FillBehavior.Stop,
                BeginTime = TimeSpan.FromSeconds(0),
                Duration = new Duration(TimeSpan.FromSeconds(0.5))
            };

            var storyboard = new Storyboard();

            storyboard.Children.Add(fadeAnim);
            Storyboard.SetTarget(fadeAnim, Spinner);
            Storyboard.SetTarget(fadeAnim, progBar);
            Storyboard.SetTargetProperty(fadeAnim, new PropertyPath(OpacityProperty));

            storyboard.Completed += delegate { Spinner.Visibility = System.Windows.Visibility.Visible; progBar.Visibility = System.Windows.Visibility.Visible; };
            storyboard.Begin();
            serverListTabsBlur.Radius = 7.0;
            serverListTabs.IsEnabled = false;
            Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new NoArgDelegate(DeleteLabels));
        }
 
        private void ButtonRefresh_Click(object sender, RoutedEventArgs e)
        {
            Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new NoArgDelegate(AnimateRefresh));
            NoArgDelegate fetcher = new NoArgDelegate(this.GetServers);
            fetcher.BeginInvoke(null, null);
        }
    }
}
