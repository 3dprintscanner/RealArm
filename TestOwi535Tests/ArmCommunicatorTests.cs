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

            Assert.AreEqual((ElbowUp + ShoulderDown + WristUp),processedMoves.First().opCode1);
            Assert.AreEqual((ElbowUp + ShoulderDown + WristUp + WristDown), processedMoves.ElementAt(1).opCode1);
            Assert.AreEqual((ElbowUp + ShoulderUp + ShoulderDown), processedMoves.ElementAt(2).opCode1);
            Assert.AreEqual((ElbowDown + ElbowUp), processedMoves.ElementAt(3).opCode1);
            Assert.AreEqual(0x00, processedMoves.ElementAt(4).opCode1);
            
        }
        [Test]
        public void ProcessedMovesListSecondByte()
        {
            ArmCommunicator communicator = new ArmCommunicator();
            List<Move> movesList = new List<Move>();
            movesList.Add(new Move() { direction = ArmCommunicator.POSITIVE, JointId = JointID.BASE, time = 500 });
            movesList.Add(new Move() { direction = ArmCommunicator.POSITIVE, JointId = JointID.ELBOW, time = 350 });
            movesList.Add(new Move() { direction = ArmCommunicator.NEGATIVE, JointId = JointID.SHOULDER, time = 300 });
            movesList.Add(new Move() { direction = ArmCommunicator.POSITIVE, JointId = JointID.WRIST, time = 280 });

            var processedMoves = communicator.ProcessedMovesList(movesList);

            Assert.AreEqual(0x01, processedMoves.First().opCode2);
            Assert.AreEqual(0x01, processedMoves.ElementAt(1).opCode2);
            Assert.AreEqual(0x01, processedMoves.ElementAt(2).opCode2);
            Assert.AreEqual(0x01, processedMoves.ElementAt(3).opCode2);
            Assert.AreEqual(0x00, processedMoves.ElementAt(4).opCode2);
        }

        [Test]
        public void ProcessedMovesListTimes()
        {
            ArmCommunicator communicator = new ArmCommunicator();
            List<Move> movesList = new List<Move>();
            movesList.Add(new Move() { direction = ArmCommunicator.POSITIVE, JointId = JointID.BASE, time = 500 });
            movesList.Add(new Move() { direction = ArmCommunicator.POSITIVE, JointId = JointID.ELBOW, time = 350 });
            movesList.Add(new Move() { direction = ArmCommunicator.NEGATIVE, JointId = JointID.SHOULDER, time = 300 });
            movesList.Add(new Move() { direction = ArmCommunicator.POSITIVE, JointId = JointID.WRIST, time = 280 });

            var processedMoves = communicator.ProcessedMovesList(movesList);

            Assert.AreEqual(280, processedMoves.First().time);
            Assert.AreEqual(20, processedMoves.ElementAt(1).time);
            Assert.AreEqual(50, processedMoves.ElementAt(2).time);
            Assert.AreEqual(150, processedMoves.ElementAt(3).time);
            Assert.AreEqual(10, processedMoves.ElementAt(4).time);
        }

        [Test]
        public void ProcessedMovesWithSameTimes()
        {
            ArmCommunicator communicator = new ArmCommunicator();
            List<Move> movesList = new List<Move>();
            movesList.Add(new Move() { direction = ArmCommunicator.POSITIVE, JointId = JointID.BASE, time = 500 });
            movesList.Add(new Move() { direction = ArmCommunicator.POSITIVE, JointId = JointID.ELBOW, time = 300 });
            movesList.Add(new Move() { direction = ArmCommunicator.NEGATIVE, JointId = JointID.SHOULDER, time = 300 });
            movesList.Add(new Move() { direction = ArmCommunicator.POSITIVE, JointId = JointID.WRIST, time = 300 });

            var processedMoves = communicator.ProcessedMovesList(movesList);

            Assert.AreEqual(0, processedMoves.First().time);
            Assert.AreEqual(300, processedMoves.ElementAt(1).time);
            Assert.AreEqual(300, processedMoves.ElementAt(2).time);
            Assert.AreEqual(300, processedMoves.ElementAt(3).time);
            Assert.AreEqual(500, processedMoves.ElementAt(4).time);

        }
    }
}
