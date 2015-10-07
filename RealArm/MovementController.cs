using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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
        private PXCMHandConfiguration _handConfiguration;
        private PXCMHandModule _handModule;
        private PXCMSenseManager _senseManager;
        private PXCMSenseManager.Handler _handler;
        private readonly PXCMHandConfiguration.OnFiredGestureDelegate _handGestureHandler;
        private readonly PXCMHandConfiguration.OnFiredAlertDelegate _handAlertHandler;
        private PXCMHandData.IHand _iHand;
        private PXCMHandData _handData;
        
          

        public IArmConfiguration ArmConfiguration { get; set; }

        public MovementController(PXCMSenseManager.Handler handler,
            PXCMHandConfiguration.OnFiredGestureDelegate handGestureHandler,
            PXCMHandConfiguration.OnFiredAlertDelegate handAlertHandler)
        {
            
            // register event handlers
            this._handler = handler;
            _handGestureHandler = handGestureHandler;
            _handAlertHandler = handAlertHandler;
            Session = PXCMSession.CreateInstance();
        
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
            _handConfiguration.EnableGesture("thumb_up");
            _handConfiguration.ApplyChanges();
            _senseManager.Init(_handler);
            sensorActive = true;
            _senseManager.StreamFrames(true);
            _senseManager.Close();
            

        }

        public string GetHandPosition()
        {
            try
            {
                Coord3D handLocation = GetTransformPosition(GetHandPXCMPoint32());
                return String.Format("X={0}, Y={1}, Z={2}", (handLocation.X), (handLocation.Y),
                    (handLocation.Z));
            }
            catch (HandNotFoundException exception)
            {
                return "Hand Not Found";

            }
            
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

            if (ihand != null)
            {
                return ihand.QueryMassCenterWorld();
            }
            else
            {
                throw new HandNotFoundException();
            }


        }

        public void ActivateGripper()
        {
            if (_moveInProgress || !armActive) return;
            _robotArm.openGripper(!gripperOpen);
            gripperOpen = !gripperOpen;
            BlinkLight();
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

        public void BlinkLight()
        {
            _robotArm.setLight(true);
            Task.Delay(1000);
            _robotArm.setLight(false);
            Task.Delay(1000);
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
            // The limits of the system will be determined by the length of the two arm bones and the angle of the base, the range of vertical motion of the base
            // The Z height limit will be maximum of the arm length and minimum of the length of the forearm and hand.
            // The co-ordinates are positive
            // e.g x+0.25 *maxlengthX and so forth.

            // 300,300,150

            
            handPosition.x = (handPosition.x + 0.25f)*(300/0.25f);
            handPosition.y *= (handPosition.y + 0.15f) * (300 / 0.15f);
            handPosition.z *= (handPosition.z) * (150 / 0.55f);
            return new Coord3D((int)Math.Ceiling(handPosition.x),(int)Math.Ceiling(handPosition.y),(int)Math.Ceiling(handPosition.z));
        }

        public void Dispose()
        {
            _senseManager.Close();
            Session.Dispose();
            _senseManager.Dispose();
            _handConfiguration.Dispose();
            _handModule.Dispose();
            
            _handData.Dispose();
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

    public class HandNotFoundException : Exception
    {

    }
}