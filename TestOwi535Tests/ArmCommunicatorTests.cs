using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestOwi535;
using NUnit.Framework;
namespace TestOwi535.Tests
{
    [TestFixture()]
    public class ArmCommunicatorTests
    {
        public const int WristUp = 0x08;
        public const int WristDown = 0x04;
        public const int ElbowUp = 0x20;
        public const int ElbowDown = 0x10;
        public const int ShoulderUp = 0x80;
        public const int ShoulderDown = 0x40;
        
        
        [SetUp]
        public void Setup()
        {


        }

        [Test()]
        public void ArmCommunicatorTest()
        {
            Assert.Fail();
        }

        [Test()]
        public void closeTest()
        {
            Assert.Fail();
        }

        [Test()]
        public void openGripperTest()
        {
            Assert.Fail();
        }

        [Test()]
        public void setLightTest()
        {
            Assert.Fail();
        }

        [Test()]
        public void turnTest()
        {
            Assert.Fail();
        }

        [Test()]
        public void waitTest()
        {
            Assert.Fail();
        }

        [Test()]
        public void MainTest()
        {
            Assert.Fail();
        }

        [Test()]
        public void compositeTurnTest()
        {
            Assert.Fail();
        }

        [Test()]
        public void ProcessedMovesListFirstByte()
        {
            // generate a list of legitimate moves
            ArmCommunicator communicator =  new ArmCommunicator();
            List<Move> movesList = new List<Move>();
            movesList.Add(new Move() {direction = ArmCommunicator.POSITIVE, JointId = JointID.BASE, time = 500});
            movesList.Add(new Move() {direction = ArmCommunicator.POSITIVE, JointId = JointID.ELBOW, time = 350});
            movesList.Add(new Move() {direction = ArmCommunicator.NEGATIVE, JointId = JointID.SHOULDER, time = 300});
            movesList.Add(new Move() {direction = ArmCommunicator.POSITIVE, JointId = JointID.WRIST, time = 280});
            
            var processedMoves = communicator.ProcessedMovesList(movesList);

            Assert.AreEqual((ElbowUp + ShoulderDown + WristUp),processedMoves[0][0]);
            Assert.AreEqual((ElbowUp + ShoulderDown + WristUp + WristDown), processedMoves[0][1]);
            Assert.AreEqual((ElbowUp + ShoulderUp + ShoulderDown), processedMoves[0][2]);
            Assert.AreEqual((ElbowDown + ElbowUp), processedMoves[0][3]);
            Assert.AreEqual(0x00, processedMoves[0][4]);
            



        }

        public void ProcessedMovesListSecondByte()
        {

        }
    }
}
