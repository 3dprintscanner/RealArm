using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestOwi535;

namespace RealArm
{
    public class MovementController : MovementControllerBase
    {

        public IArm arm { get; set; }
        public ISensor sensor { get; set; }
        public PXCMSession Session;
        private bool _moveInProgress = false;
        private bool gripperOpen = true;
        private RobotArm _robotArm;
        private int _frameCounter = 25;
        private int _currentFrame;
        private PXCMPoint3DF32 _referencePosition;
        private PXCMHandConfiguration _handConfiguration;
        private PXCMHandModule _handModule;
        private PXCMSenseManager _senseManager;
        private PXCMSenseManager.Handler _handler;
        private PXCMHandData.IHand _iHand;
        private PXCMHandData _handData;
          

        public IArmConfiguration ArmConfiguration { get; set; }

        public MovementController(IArm arm)
        {
            this.arm = arm;

            // register event handlers
            arm.MoveComplete += OnMoveComplete;
            arm.StopReached += OnStopReached;
            BlinkLight();
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


        private void OnGestureReceived(PXCMHandData.GestureData data)
        {
            // check to see whether the received gesture is a pinch movement, if so call the pinch on the arm.
            if (data.name.CompareTo("full_pinch") == 0)
            {
                if (_moveInProgress) return;
                _robotArm.openGripper(gripperOpen);
            }
        }



        public override void Listen()
        {
            // attach the controller to the PXCM sensor
            _senseManager = Session.CreateSenseManager();
            _handler = new PXCMSenseManager.Handler
            {
                onModuleProcessedFrame = OnModuleProcessedFrame,
                onConnect = OnConnect
            };
            _handler.onModuleProcessedFrame = OnModuleProcessedFrame;
            _senseManager.EnableHand();
            _handModule = _senseManager.QueryHand();
            _handData = _handModule.CreateOutput();
            _handConfiguration = _handModule.CreateActiveConfiguration();
            _handConfiguration.SubscribeGesture(OnGestureReceived);
            _handConfiguration.SubscribeAlert(OnAlertReceived);
            _senseManager.Init();
            
        }

        private void OnAlertReceived(PXCMHandData.AlertData alertData)
        {
            
        }

        private pxcmStatus OnConnect(PXCMCapture.Device device, bool connected)
        {
            if (connected)
            {
                Console.WriteLine("Device Connected");
                return pxcmStatus.PXCM_STATUS_NO_ERROR;
            }
            return pxcmStatus.PXCM_STATUS_DEVICE_FAILED;
            
        }

        private pxcmStatus OnModuleProcessedFrame(int mid, PXCMBase module, PXCMCapture.Sample sample)
        {
            _currentFrame++;
            if (_currentFrame % _frameCounter != 0 && !_moveInProgress) return pxcmStatus.PXCM_STATUS_NO_ERROR;
            _currentFrame = 0;
            _handData.Update();
            Int32 handId;
            _handData.QueryHandId(PXCMHandData.AccessOrderType.ACCESS_ORDER_NEAR_TO_FAR, 0, out handId);
            _handData.QueryHandDataById(handId, out _iHand);
            var currentHandPosition = _iHand.QueryMassCenterWorld();
            AssertArmMovement(_referencePosition,currentHandPosition);
            _referencePosition = currentHandPosition;
            return pxcmStatus.PXCM_STATUS_NO_ERROR;

        }

        public override void UnListen()
        {
            
        }

        public override void ActivateActuator()
        {
            _robotArm = new RobotArm();
            _robotArm.moveToZero();
            
        }

        public override void DeactivateActuator()
        {
            _robotArm.moveToZero();
            _robotArm.close();
            _robotArm = null;
        }

        private void BlinkLight()
        {
            _robotArm.setLight(true);
            Task.Delay(1000);
            _robotArm.setLight(false);
        }

        private void AssertArmMovement(PXCMPoint3DF32 referencePosition, PXCMPoint3DF32 handPosition)
        {
            // calculate the difference between the processed frames and move the arm if a threshold for sensitivity is reached
            var xDifference = (int) Math.Ceiling(handPosition.x - referencePosition.x);
            var yDifference = (int) Math.Ceiling(handPosition.y - referencePosition.y);
            var zDifference = (int)Math.Ceiling(handPosition.z- referencePosition.z);
            if(xDifference >50 && yDifference >50 && zDifference > 50)
            {
                _robotArm.moveTo((int) Math.Ceiling(handPosition.x), (int) Math.Ceiling(handPosition.y),
                (int) Math.Ceiling(handPosition.z));
                _moveInProgress = true;
            }
        }

    }
}
