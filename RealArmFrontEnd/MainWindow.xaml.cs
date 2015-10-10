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
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;
using RealArm;
using TestOwi535;

namespace RealArmFrontEnd
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly MovementController _movementController;
        private PXCMImage.ImageData colorData = null;
        private Bitmap colourBitMap;
        private Thread thread;
        private int frameCounter = 0;
        private bool calibrated;
        private string labelContent;

        
        public MainWindow()
        {
            InitializeComponent();
            _movementController = new MovementController(new PXCMSenseManager.Handler
            {
                onModuleProcessedFrame = OnModuleProcessedFrame,
                onNewSample = OnNewSample,
                onConnect = OnConnect
            },
            OnFiredGesture,
            OnFiredAlert);
            calibrated = false;
            DisableMovementButtons();
        }

        private pxcmStatus OnConnect(PXCMCapture.Device device, bool connected)
        {

            Dispatcher.Invoke(() => sensorbutton.Content = "Deactivate Sensor");
            
            return pxcmStatus.PXCM_STATUS_NO_ERROR;
        }

        private void OnFiredGesture(PXCMHandData.GestureData gestureData)
        {
            switch (gestureData.state)
            {
                case PXCMHandData.GestureStateType.GESTURE_STATE_START:
                    break;
                case PXCMHandData.GestureStateType.GESTURE_STATE_IN_PROGRESS:
                    break;
                case PXCMHandData.GestureStateType.GESTURE_STATE_END:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (gestureData.name == "full_pinch")
            {
                _movementController.ActivateGripper();
            }
            if (gestureData.name == "thumb_up")
            {
                _movementController.BlinkLight();
            }
        }

        private void OnFiredAlert(PXCMHandData.AlertData alertData)
        {
            switch (alertData.label)
            {
                case PXCMHandData.AlertType.ALERT_HAND_DETECTED:
                    calibrated = true;
                    break;
                case PXCMHandData.AlertType.ALERT_HAND_NOT_DETECTED:
                    calibrated = false;
                    break;
                case PXCMHandData.AlertType.ALERT_HAND_TRACKED:
                    calibrated = true;
                    break;
                case PXCMHandData.AlertType.ALERT_HAND_NOT_TRACKED:
                    calibrated = false;
                    break;
                case PXCMHandData.AlertType.ALERT_HAND_CALIBRATED:
                    calibrated = true;
                    break;
                case PXCMHandData.AlertType.ALERT_HAND_NOT_CALIBRATED:
                    calibrated = false;
                    break;
                case PXCMHandData.AlertType.ALERT_HAND_OUT_OF_BORDERS:
                    calibrated = false;
                    break;
                case PXCMHandData.AlertType.ALERT_HAND_INSIDE_BORDERS:
                    calibrated = true;
                    break;
                case PXCMHandData.AlertType.ALERT_HAND_OUT_OF_LEFT_BORDER:
                    calibrated = false;
                    break;
                case PXCMHandData.AlertType.ALERT_HAND_OUT_OF_RIGHT_BORDER:
                    calibrated = false;
                    break;
                case PXCMHandData.AlertType.ALERT_HAND_OUT_OF_TOP_BORDER:
                    calibrated = false;
                    break;
                case PXCMHandData.AlertType.ALERT_HAND_OUT_OF_BOTTOM_BORDER:
                    calibrated = false;
                    break;
                case PXCMHandData.AlertType.ALERT_HAND_TOO_FAR:
                    calibrated = false;
                    break;
                case PXCMHandData.AlertType.ALERT_HAND_TOO_CLOSE:
                    calibrated = false;
                    break;
                case PXCMHandData.AlertType.ALERT_HAND_LOW_CONFIDENCE:
                    calibrated = false;
                    break;
                default:
                    calibrated = false;
                    throw new ArgumentOutOfRangeException();
            }
        }

        private async void startStream()
        {
                await Task.Run(() => _movementController.Listen());
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
            if (calibrated)
            {
                frameCounter++;
                if (frameCounter % 100 == 0)
                {
                    this.Dispatcher.Invoke((() =>
                    {
                        var position = _movementController.GetHandPosition();
                        outputbox.Text = AppendOutputboxText(position);
                    }));
                    _movementController.AssertArmMovement();
                    frameCounter = 0;                                                   
                }
            }

            
            return pxcmStatus.PXCM_STATUS_NO_ERROR;
        }

        private string AppendOutputboxText(string text)
        {
            return text + "\n" + outputbox.Text;
        }



        private void sensorbutton_Click(object sender, RoutedEventArgs e)
        {
            // activate the realsense sensor
           
            if (!_movementController.sensorActive)
            {
                startStream();
                
            }
            else
            {
                sensorbutton.Content = "Activate Sensor";
                stopStream();
            }                      
        }

        private async void stopStream()
        {
            await Task.Run(() => _movementController.UnListen());
        }

        private void armbutton_Click(object sender, RoutedEventArgs e)
        {
            if (!_movementController.armActive)
            {
                _movementController.ActivateActuator();
                Dispatcher.Invoke(() => armbutton.Content = "Deactivate Arm");
                EnableMovementButtons();
            }
            else
            {
                _movementController.DeactivateActuator();
                Dispatcher.Invoke(() => armbutton.Content = "Activate Arm");
                DisableMovementButtons();
            }
        }

        private void DisableMovementButtons()
        {
            shoulder_down_button.IsEnabled = false;
            shoulder_up_buton.IsEnabled = false;
            wrist_down_button.IsEnabled = false;
            wrist_up_button.IsEnabled = false;
            base_left_button.IsEnabled = false;
            base_right_button.IsEnabled = false;
            grab_button.IsEnabled = false;
            light_button.IsEnabled = false;
            calibratezero.IsEnabled = false;
            elbow_down_button.IsEnabled = false;
            elbow_up_button.IsEnabled = false;
            queryposition.IsEnabled = false;
            zerobutton.IsEnabled = false;


        }

        private void EnableMovementButtons()
        {
            shoulder_down_button.IsEnabled = true;
            shoulder_up_buton.IsEnabled = true;
            wrist_down_button.IsEnabled = true;
            wrist_up_button.IsEnabled = true;
            base_left_button.IsEnabled = true;
            base_right_button.IsEnabled = true;
            grab_button.IsEnabled = true;
            light_button.IsEnabled = true;
            calibratezero.IsEnabled = true;
            elbow_down_button.IsEnabled = true;
            elbow_up_button.IsEnabled = true;
            queryposition.IsEnabled = true;
            zerobutton.IsEnabled = true;
        }

       
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //deregister event handlers
            if(_movementController != null) _movementController.Dispose();
            colorData = null;
            if(colourBitMap != null) colourBitMap.Dispose();
            Environment.Exit(0);
        }

        private void calibratezero_click(object sender, RoutedEventArgs e)
        {
            _movementController.ResetCalibration();
            Dispatcher.Invoke(() => outputbox.Text = AppendOutputboxText("Arm Zeroed"));
        }

        private void grab_button_Click(object sender, RoutedEventArgs e)
        {
            _movementController.ActivateGripper();
        }

        private void light_button_Click(object sender, RoutedEventArgs e)
        {
            _movementController.BlinkLight();
        }

        private void elbow_up_button_Click(object sender, RoutedEventArgs e)
        {
            _movementController.Move(JointID.ELBOW, ArmCommunicator.POSITIVE);
        }

        private void elbow_down_button_Click(object sender, RoutedEventArgs e)
        {
            _movementController.Move(JointID.ELBOW, ArmCommunicator.NEGATIVE);
        }

        private void shoulder_up_buton_Click(object sender, RoutedEventArgs e)
        {
            _movementController.Move(JointID.SHOULDER, ArmCommunicator.POSITIVE);
        }

        private void shoulder_down_button_Click(object sender, RoutedEventArgs e)
        {
            _movementController.Move(JointID.SHOULDER, ArmCommunicator.NEGATIVE);
        }

        private void base_left_button_Click(object sender, RoutedEventArgs e)
        {
            _movementController.Move(JointID.BASE, ArmCommunicator.POSITIVE);
        }

        private void base_right_button_Click(object sender, RoutedEventArgs e)
        {
            _movementController.Move(JointID.BASE, ArmCommunicator.NEGATIVE);
        }

        private void wrist_up_button_click(object sender, RoutedEventArgs e)
        {
            _movementController.Move(JointID.WRIST, ArmCommunicator.POSITIVE);
        }

        private void wrist_down_button_Click(object sender, RoutedEventArgs e)
        {
            _movementController.Move(JointID.WRIST, ArmCommunicator.NEGATIVE);
        }

        private void queryposition_Click(object sender, RoutedEventArgs e)
        {
             var positionString = _movementController.GetArmPosition();
             Dispatcher.Invoke(() => outputbox.Text = AppendOutputboxText("Position is: " + positionString));
        }


        private void zerobutton_Click(object sender, RoutedEventArgs e)
        {
            _movementController.ZeroArm();
        }
    }
}