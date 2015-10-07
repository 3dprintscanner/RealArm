using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
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
using RealArm;

namespace RealArmFrontEnd
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly MovementController _movementController;
        public PXCMSession Session;
        private PXCMImage.ImageData colorData = null;
        private Bitmap colourBitMap;
        private Thread thread;
        private int frameCounter = 0;
        private bool calibrated = false;
        private string labelContent;
        
        public MainWindow()
        {
            InitializeComponent();
            _movementController = new MovementController(new PXCMSenseManager.Handler
            {
                onModuleProcessedFrame = OnModuleProcessedFrame,
                onNewSample = OnNewSample
            },
            OnFiredGesture,
            OnFiredAlert);
            
            
        }

        private void OnFiredGesture(PXCMHandData.GestureData gesturedata)
        {
            if (gesturedata.name == "full_pinch")
            {
                _movementController.ActivateGripper();
            }
        }

        private void OnFiredAlert(PXCMHandData.AlertData alertData)
        {          
            if (alertData.label == PXCMHandData.AlertType.ALERT_HAND_CALIBRATED) calibrated = true;
           
        }

        private void startStream()
        {
                _movementController.Listen();
        }

        private pxcmStatus OnNewSample(int mid, PXCMCapture.Sample sample)
        {
            var _sample = sample;
            _sample.color.AcquireAccess(PXCMImage.Access.ACCESS_READ, PXCMImage.PixelFormat.PIXEL_FORMAT_RGB24,
             out colorData);
            colourBitMap = colorData.ToBitmap(0, sample.color.info.width, sample.color.info.height);
            UpdateUI(colourBitMap);
            _sample.color.ReleaseAccess(colorData);
            return pxcmStatus.PXCM_STATUS_NO_ERROR;
        }

        private void UpdateUI(Bitmap bitmap)
        {
            this.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(delegate()
            {
                if (colourBitMap != null)
                {
                    imgColourStream.RenderTransformOrigin = new System.Windows.Point(0.5, 0.5);
                    ScaleTransform mainTransform = new ScaleTransform();
                    mainTransform.ScaleX = 1;
                    mainTransform.ScaleY = 1;
                    imgColourStream.RenderTransform = mainTransform;

                    imgColourStream.Source = ConvertBitmap.BitmapToBitmapSource(colourBitMap);                  
                    }
            }));
        }

        private pxcmStatus OnModuleProcessedFrame(int mid, PXCMBase module, PXCMCapture.Sample sample)
        {
            // use this sample to work on the hand data.
            if (calibrated == true)
            {
                frameCounter++;
                if (frameCounter % 100 == 0)
                {
                    
                    this.Dispatcher.Invoke((() =>
                    {
                        var position = _movementController.GetHandPosition();
                        handPosition.Content = position;
                    }));
                    _movementController.AssertArmMovement();   
                    frameCounter = 0;                
                }
            }

            
            return pxcmStatus.PXCM_STATUS_NO_ERROR;
        }

    
        private void sensorbutton_Click(object sender, RoutedEventArgs e)
        {
            // activate the realsense sensor
           
            if (!_movementController.sensorActive)
            {
                thread = new Thread(new ThreadStart(startStream));
                thread.Start();
            }
            else
            {
                thread.Abort();
                sensorbutton.Content = "Activate Sensor";
            }                      
        }

        private void armbutton_Click(object sender, RoutedEventArgs e)
        {
            if (!_movementController.armActive)
            {
                _movementController.ActivateActuator();
            }
            else
            {
                _movementController.DeactivateActuator();
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            _movementController.ZeroArm();
        }

        private void MoveArm_Click(object sender, RoutedEventArgs e)
        {
            _movementController.TestMove();
        }
    }
}