using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Runtime.InteropServices;
using System.Threading;

namespace TactorMatching
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        #region Tactor Methods
        [DllImport(@"C:\Users\minisim\Desktop\Tactors\TDKAPI_1.0.6.0\libraries\Windows\TactorInterface.dll")]
        public static extern IntPtr GetVersionNumber();

        [DllImport(@"C:\Users\minisim\Desktop\Tactors\TDKAPI_1.0.6.0\libraries\Windows\TactorInterface.dll",
            CallingConvention = CallingConvention.Cdecl)]
        public static extern int Discover(int type);

        [DllImport(@"C:\Users\minisim\Desktop\Tactors\TDKAPI_1.0.6.0\libraries\Windows\TactorInterface.dll",
            CallingConvention = CallingConvention.Cdecl)]
        public static extern int Connect(string name, int type, IntPtr _callback);

        [DllImport(@"C:\Users\minisim\Desktop\Tactors\TDKAPI_1.0.6.0\libraries\Windows\TactorInterface.dll",
            CallingConvention = CallingConvention.Cdecl)]
        public static extern int InitializeTI();

        [DllImport(@"C:\Users\minisim\Desktop\Tactors\TDKAPI_1.0.6.0\libraries\Windows\TactorInterface.dll",
            CallingConvention = CallingConvention.Cdecl)]
        public static extern int Pulse(int deviceID, int tacNum, int msDuration, int delay);

        [DllImport(@"C:\Users\minisim\Desktop\Tactors\TDKAPI_1.0.6.0\libraries\Windows\TactorInterface.dll",
            CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr GetDiscoveredDeviceName(int index);

        [DllImport(@"C:\Users\minisim\Desktop\Tactors\TDKAPI_1.0.6.0\libraries\Windows\TactorInterface.dll",
            CallingConvention = CallingConvention.Cdecl)]
        public static extern int DiscoverLimited(int type, int amount);

        [DllImport(@"C:\Users\minisim\Desktop\Tactors\TDKAPI_1.0.6.0\libraries\Windows\TactorInterface.dll",
           CallingConvention = CallingConvention.Cdecl)]
        public static extern int ChangeGain(int deviceID, int tacNum, int gainval, int delay);

        [DllImport(@"C:\Users\minisim\Desktop\Tactors\TDKAPI_1.0.6.0\libraries\Windows\TactorInterface.dll",
            CallingConvention = CallingConvention.Cdecl)]
        public static extern int CloseAll();

        [DllImport(@"C:\Users\minisim\Desktop\Tactors\TDKAPI_1.0.6.0\libraries\Windows\TactorInterface.dll",
            CallingConvention = CallingConvention.Cdecl)]
        public static extern int UpdateTI();
        #endregion

        #region Variables/Constants
        private const int SEAT_BELT_HIGH = 225;
        private const int SEAT_BELT_LOW = 75;

        private enum Type{
            Pan = 1,
            wear = 2,
            back = 3
        };

        private int[] seat_back_highs;
        private int[] seat_back_lows;
        private int[] seat_pan_highs;
        private int[] seat_pan_lows;
        private int[] seat_steering_highs;
        private int[] seat_steering_lows;
        private int[] seat_wear_highs;
        private int[] seat_wear_lows;
        private bool[] completed;
        private Button[] startButtons;

        private int completedTasks;
        private int iterations;
        private int currentType;
        private int LowOrHigh;
        private bool startedTask;
        private int minGain;
        private int maxGain;
        private int currentGain;
        #endregion

        public void testVibrations()
        {
            for (int x = 1; x <= 16; x++)
            {
                Pulse(0, x, 300, 0);
                Thread.Sleep(500);
            }
        }

        public void setupTactors()
        {
            if (InitializeTI() == 0)
            {
                System.Diagnostics.Debug.WriteLine("TDK Initialized");
            }

            int tactors = Discover(1);
            string name = Marshal.PtrToStringAnsi((IntPtr)GetDiscoveredDeviceName(0));

            if (Connect(name, 1, IntPtr.Zero) >= 0)
            {
                System.Diagnostics.Debug.WriteLine("Connected");
            }
        }

        public void InitializeStart()
        {
            seat_pan_highs = new int[3];
            seat_pan_lows = new int[3];
            seat_back_highs = new int[3];
            seat_back_lows = new int[3];
            seat_wear_highs = new int[3];
            seat_wear_lows = new int[3];
            completed = new bool[3];

            completedTasks = 0;
            startedTask = false;

            StartHighMatch.Visibility = Visibility.Hidden;
            StartLowMatch.Visibility = Visibility.Hidden;

            iterations = 0;

            startButtons = new Button[]
            {
                StartPanMatch,
                StartWearMatch,
                StartBackMatch
            };
        }

        public void HideStartButtons()
        {
            StartBackMatch.Visibility = Visibility.Hidden;
            StartPanMatch.Visibility = Visibility.Hidden;
            StartWearMatch.Visibility = Visibility.Hidden;
        }

        public void ToggleLowAndHighBtns()
        {
            StartHighMatch.Visibility = StartHighMatch.Visibility == Visibility.Hidden ? Visibility.Visible : Visibility.Hidden;
            StartLowMatch.Visibility = StartLowMatch.Visibility == Visibility.Hidden ? Visibility.Visible : Visibility.Hidden;
        }

        public void SetTactorsGain()
        {
            switch (currentType)
            {
                case (int)Type.Pan:
                    ChangeGain(0, 13, minGain, 0);
                    ChangeGain(0, 14, minGain, 0);
                    ChangeGain(0, 15, minGain, 0);
                    ChangeGain(0, 16, minGain, 0);
                    break;
                case (int)Type.wear:
                    ChangeGain(0, 7, minGain, 0);
                    ChangeGain(0, 8, minGain, 0);
                    break;
                case (int)Type.back:
                    ChangeGain(0, 9, minGain, 0);
                    ChangeGain(0, 10, minGain, 0);
                    ChangeGain(0, 11, minGain, 0);
                    ChangeGain(0, 12, minGain, 0);
                    break;
            }
        }


        public MainWindow()
        {
            InitializeComponent();
            InitializeStart();
            setupTactors();
            
        }

        private void SeatBeltHigh_Click(object sender, RoutedEventArgs e)
        {
            ChangeGain(0, 1, SEAT_BELT_HIGH, 0);
            ChangeGain(0, 2, SEAT_BELT_HIGH, 0);
            ChangeGain(0, 3, SEAT_BELT_HIGH, 0);
            Pulse(0, 1, 500, 0);
            Pulse(0, 2, 500, 0);
            Pulse(0, 3, 500, 0);
        }

        private void SeatBeltLow_Click(object sender, RoutedEventArgs e)
        {
            ChangeGain(0, 1, SEAT_BELT_LOW, 0);
            ChangeGain(0, 2, SEAT_BELT_LOW, 0);
            ChangeGain(0, 3, SEAT_BELT_LOW, 0);
            Pulse(0, 1, 500, 0);
            Pulse(0, 2, 500, 0);
            Pulse(0, 3, 500, 0);
        }

        private void StartBackMatch_Click(object sender, RoutedEventArgs e)
        {
            /* TODO: implement back tactors matching */
            InstructionLbl.Content = "Use the 'A' and 'D' keys to control the vibration" + Environment.NewLine
                         + "Press 'Start High' or 'Start Low' to begin";
            currentType = (int)Type.back;
            startedTask = true;
            HideStartButtons();
            ToggleLowAndHighBtns();
        }

        private void StartWearMatch_Click(object sender, RoutedEventArgs e)
        {
            /* TODO: implement wearable tactors matching */
            InstructionLbl.Content = "Use the 'A' and 'D' keys to control the vibration" + Environment.NewLine
                         + "Press 'Start High' or 'Start Low' to begin";
            currentType = (int)Type.wear;
            startedTask = true;
            HideStartButtons();
            ToggleLowAndHighBtns();
        }

        private void StartPanMatch_Click(object sender, RoutedEventArgs e)
        {
            /* TODO: implement pan tactors matching */
            InstructionLbl.Content = "Use the 'A' and 'D' keys to control the vibration" + Environment.NewLine
                         + "Press 'Start High' or 'Start Low' to begin";
            currentType = (int)Type.Pan;
            startedTask = true;
            HideStartButtons();
            ToggleLowAndHighBtns();
        }


        private void StartHighMatch_Click(object sender, RoutedEventArgs e)
        {
            startedTask = true;
            LowOrHigh = 1;
            currentGain = 150;
            minGain = 150;
            maxGain = 255;
            InstructionLbl.Content = "0/3 completed";
            //ToggleLowAndHighBtns(); /* TODO: This somehow prevents the tactors from vibrating until a button is clicked */
        }

        private void StartLowMatch_Click(object sender, RoutedEventArgs e)
        {
            startedTask = true;
            LowOrHigh = 0;
            currentGain = 45;
            minGain = 45;
            maxGain = 105;
            InstructionLbl.Content = "0/3 completed";
            //ToggleLowAndHighBtns(); /* TODO: This somehow prevents the tactors from vibrating until a button is clicked */

        }

        private void Submit_Click(object sender, RoutedEventArgs e)
        {
            switch (currentType)
            {
                case (int)Type.Pan:
                    if(LowOrHigh == 1)
                    {
                        seat_pan_highs[iterations] = currentGain;
                    }
                    else
                    {
                        seat_pan_lows[iterations] = currentGain;
                    }
                    break;
                case (int)Type.wear:
                    if (LowOrHigh == 1)
                    {
                        seat_wear_highs[iterations] = currentGain;
                    }
                    else
                    {
                        seat_wear_lows[iterations] = currentGain;
                    }
                    break;
                case (int)Type.back:
                    if (LowOrHigh == 1)
                    {
                        seat_back_highs[iterations] = currentGain;
                    }
                    else
                    {
                        seat_back_lows[iterations] = currentGain;
                    }
                    break;
            }

            InstructionLbl.Content = iterations+1 + "/3 completed";
            iterations++;
            if (iterations == 3)
            {
                startedTask = false;
                completed[currentType - 1] = true;
                for(int x = 0; x < completed.Length; x++)
                {
                    if (!completed[x])
                    {
                        startButtons[x].Visibility = Visibility.Visible;
                    }
                }

                ToggleLowAndHighBtns();
                iterations = 0;
                completedTasks++;

            }

            currentGain = minGain;
        }

        private void Grid_KeyDown(object sender, KeyEventArgs e)
        {
            if (startedTask)
            {
                if (e.Key == Key.A)
                {
   
                    currentGain -= 15;
                    if (currentGain <= minGain)
                    {
                        currentGain = minGain;
                    }
                }
                if (e.Key == Key.D)
                {
                    currentGain += 15;
                    if (currentGain >= maxGain)
                    {
                        currentGain = maxGain;
                    }
                }

                switch (currentType)
                {
                    case (int)Type.Pan:
                        ChangeGain(0, 13, currentGain, 0);
                        ChangeGain(0, 14, currentGain, 0);
                        ChangeGain(0, 15, currentGain, 0);
                        ChangeGain(0, 16, currentGain, 0);
                        Pulse(0, 13, 500, 0);
                        Pulse(0, 14, 500, 0);
                        Pulse(0, 15, 500, 0);
                        Pulse(0, 16, 500, 0);
                        break;
                    case (int)Type.wear:
                        ChangeGain(0, 7, currentGain, 0);
                        ChangeGain(0, 8, currentGain, 0);
                        Pulse(0, 7, 500, 0);
                        Pulse(0, 8, 500, 0);
                        break;
                    case (int)Type.back:
                        ChangeGain(0, 9, currentGain, 0);
                        ChangeGain(0, 10, currentGain, 0);
                        ChangeGain(0, 11, currentGain, 0);
                        ChangeGain(0, 12, currentGain, 0);
                        Pulse(0, 9, 500, 0);
                        Pulse(0, 10, 500, 0);
                        Pulse(0, 11, 500, 0);
                        Pulse(0, 12, 500, 0);
                        break;
                }
            }
        }
    }
}
