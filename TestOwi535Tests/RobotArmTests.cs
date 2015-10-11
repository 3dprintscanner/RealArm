using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TestOwi535;
using NUnit.Framework;
namespace TestOwi535.Tests
{
    [TestFixture()]
    public class RobotArmTests
    {
        [Test()]
        public void moveToTest()
        {
            Assert.Fail();
        }

        [Test()]
        public void RobotArmTest()
        {
            Assert.Fail();
        }

        [Test()]
        public void closeTest()
        {
            Assert.Fail();
        }

        [Test()]
        public void moveToZeroTest()
        {
            Assert.Fail();
        }

        [Test()]
        public void turnByOffsetTest()
        {
            Assert.Fail();
        }

        [Test()]
        public void turnToAngleTest()
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
        public void waitTest()
        {
            Assert.Fail();
        }

        [Test()]
        public void showAnglesTest()
        {
            Assert.Fail();
        }

        [Test()]
        public void moveToTest1()
        {
            Assert.Fail();
        }

        [Test()]
        public void moveToTest2()
        {
            Assert.Fail();
        }

        [Test()]
        public void getCoordTest()
        {
            Assert.Fail();
        }

        [Test()]
        public void moveItemTest()
        {
            Assert.Fail();
        }

        [Test()]
        public void ResetJointCalibrationTest()
        {
            Assert.Fail();
        }

        [Test()]
        public void MainTest()
        {
            Assert.Fail();
        }

        [Test]
        public void SimpleMoveToCompositeTest()
        {

            RobotArm arm = new RobotArm();
            arm.moveToComposite(new Coord3D(){X = 0, Y = 229, Z = 196});
            Thread.Sleep(1000);
            arm.moveToZero();
        }

        [Test]
        public void SimpleMoveToCoordTest()
        {

            RobotArm arm = new RobotArm();
            arm.moveTo(new Coord3D() { X = 0, Y = 229, Z = 196 });
            Thread.Sleep(1000);
            arm.moveToZero();
        }

    }
}
