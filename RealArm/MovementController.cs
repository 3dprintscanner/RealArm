using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestOwi535;

namespace RealArm
{
    public class MovementController : MovementControllerBase, IDisposable
    {

        public IArm arm { get; set; }
        public ISensor sensor { get; set; }
        public PXCMSession Session;
        private bool _moveInProgress = false;
        private bool gripperOpen = true;
        public bool armActive = false;
        public bool sensorActive = false;
        private RobotArm _robotArm;
        private int _frameCounter = 25;
        private int _currentFrame;
        private PXCMPoint3DF32 _referencePosition;
        private PXCMHandConfiguration _handConfiguration;
        private PXCMHandModule _handModule;
        private PXCMSenseManager _senseManager;
        private PXCMSenseManager.Handler _handler;
        private readonly PXCMHandConfiguration.OnFiredGestureDelegate _handGestureHandler;
        private readonly PXCMHandConfiguration.OnFiredAlertDelegate _handAlertHandler;
        private PXCMHandData.IHand _iHand;
        private PXCMHandData _handData;
        
          

        public IArmConfiguration ArmConfiguration { get; set; }

        public MovementController(PXCMSession session, PXCMSenseManager.Handler handler,
            PXCMHandConfiguration.OnFiredGestureDelegate handGestureHandler,
            PXCMHandConfiguration.OnFiredAlertDelegate handAlertHandler)
        {
            
            // register event handlers
            this.Session = session;
            this._handler = handler;
            _handGestureHandler = handGestureHandler;
            _handAlertHandler = handAlertHandler;
            Session = PXCMSession.CreateInstance();
            _referencePosition = new PXCMPoint3DF32(0.0f,0.0f,0.0f);
        
        }

        protected void OnMoveComplete(object sender, EventArgs e)
        {
            // free the move in progress to allow another movement
            
            _moveInProgress = false;
        }


        protected void OnStopReached(object sender, EventArgs e) 
        {
            // the stop has been reached, give an error??? do not allow the arm to move further in one direction
            throw new MovementControllerException("Stop Reached");
        }


        public override void Listen()
        {
            // attach the controller to the PXCM sensor
            _senseManager = Session.CreateSenseManager();      
            _senseManager.EnableStream(PXCMCapture.StreamType.STREAM_TYPE_COLOR, 640, 480, 30);
            _senseManager.EnableHand();
            _handModule = _senseManager.QueryHand();
            _handData = _handModule.CreateOutput();
            _handConfiguration = _handModule.CreateActiveConfiguration();
            _handConfiguration.SubscribeGesture(_handGestureHandler);
            _handConfiguration.SubscribeAlert(_handAlertHandler);
            _handConfiguration.EnableAlert(PXCMHandData.AlertType.ALERT_HAND_TRACKED);
            _handConfiguration.EnableAlert(PXCMHandData.AlertType.ALERT_HAND_CALIBRATED);
            _handConfiguration.EnableGesture("full_pinch");
            _handConfiguration.ApplyChanges();
            _senseManager.Init(_handler);
            sensorActive = true;
            _senseManager.StreamFrames(true);
            _senseManager.Close();
            

        }

        private void OnAlertReceived(PXCMHandData.AlertData alertData)
        {
            Console.WriteLine("Alert recieved");
        }

        public string GetHandPosition()
        {
            PXCMPoint3DF32 handLocation = GetHandPXCMPoint32();
            return String.Format("X={0}, Y={1}, Z={2}", (handLocation.x*1000).ToString(), (handLocation.y*1000).ToString(),
                (handLocation.z*1000).ToString());
        }

        private PXCMPoint3DF32 GetHandPXCMPoint32()
        {
            _handData.Update();

            // retrieve the hand identifier

            Int32 handId;

            _handData.QueryHandId(PXCMHandData.AccessOrderType.ACCESS_ORDER_NEAR_TO_FAR, 0, out handId);

            // You can keep the unique hand ID for use throughout the session

            // retrieve the hand data by unique hand ID

            PXCMHandData.IHand ihand;

            _handData.QueryHandDataById(handId, out ihand);

            return ihand.QueryMassCenterWorld();
        }

        public void ActivateGripper()
        {
            if (_moveInProgress || !armActive) return;
            _robotArm.openGripper(!gripperOpen);
            gripperOpen = !gripperOpen;
        }

        public override void UnListen()
        {
            _senseManager.StreamFrames(false);
            _senseManager.Close();
            _senseManager = null;
            sensorActive = false;
        }

        public override void ActivateActuator()
        {
            _robotArm = new RobotArm();
            _robotArm.setLight(true);
            armActive = true;

        }

        public override void DeactivateActuator()
        {
            _robotArm.moveToZero();
            _robotArm.setLight(false);
            _robotArm.close();
            _robotArm = null;
            armActive = false;
        }

        private void BlinkLight()
        {
            _robotArm.setLight(true);
            Task.Delay(1000);
            _robotArm.setLight(false);
        }

        public void AssertArmMovement()
        {
            if (!armActive) return;
            // calculate the difference between the processed frames and move the arm if a threshold for sensitivity is reached
            _handData.Update();
            var handPosition = GetHandPXCMPoint32();
            var transformPosition = GetTransformPosition(handPosition);
            _robotArm.moveTo(transformPosition);
            //_robotArm.moveTo(150, 250,70);
        }

        private Coord3D GetTransformPosition(PXCMPoint3DF32 handPosition)
        {
            // x +- 0.25, y +- 0.15 z =0.2- 0.6
            // 
            
            handPosition.x *= 1000;
            handPosition.y *= 2000;
            handPosition.z *= 250;
            return new Coord3D((int)Math.Ceiling(handPosition.x),(int)Math.Ceiling(handPosition.y),(int)Math.Ceiling(handPosition.z));
        }

        public void Dispose()
        {
            if (_senseManager != null)
            {
                _senseManager.Close();
                _senseManager = null;
                Session = null;
            }
        }

        public void ZeroArm()
        {
            _robotArm.moveToZero();
        }

        public void TestMove()
        {
            _robotArm.moveTo(150, 250, 65);
        }
    }
}
