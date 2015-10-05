using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestOwi535
{
/* Class for rotating the Maplin Robot Arm
   (http://www.maplin.co.uk/robotic-arm-kit-with-usb-pc-interface-266257) 
   It appears to be the UK version of the US OWI-535
   (http://www.imagesco.com/robotics/owi-535.html)

   The inverse kinematics code in calcIK() comes from
   // http://www.circuitsathome.com/mcu/robotic-arm-inverse-kinematics-on-arduino
*/


public class RobotArm
{
  // lengths of arm elements (in mm)
  private const int GRIPPER_LEN = 110;   
  private const int LOWER_ARM = 110;
  private const int UPPER_ARM = 90;
  private const int BASE_HEIGHT = 70;


  private Joint robotBase, wrist, shoulder, elbow;
  private Joint[] joints;    // so can iterate through all the joints easily
  private ArmCommunicator armComms;


  public RobotArm()
  {
     armComms = new ArmCommunicator();

     robotBase = new Joint(JointID.BASE, armComms);
     wrist = new Joint(JointID.WRIST, armComms);
     shoulder = new Joint(JointID.SHOULDER, armComms);
     elbow = new Joint(JointID.ELBOW, armComms);

     joints = new Joint[4];   // order is important; used by moveTo()
     joints[0] = robotBase;
     joints[1] = wrist;
     joints[2] = shoulder;
     joints[3] = elbow;
  }  // end of RobotArm()

	
  public void close()
  {
    foreach(var j in joints)
      j.saveState();
    armComms.close();
  }  // end of close()
	

  public void moveToZero()
  { 
    Console.WriteLine("Moving to zero...");
    for(int i=joints.Length-1; i >= 0; i--)
      joints[i].turnToAngle(0);     // process joints in reverse
  }   // end of moveToZero()



  public void turnByOffset(JointID jid, int offsetAngle)
  {
    if (jid ==  JointID.WRIST)
      wrist.turnByOffset(offsetAngle);
    else if (jid == JointID.ELBOW)
      elbow.turnByOffset(offsetAngle);
    else if (jid == JointID.SHOULDER)
      shoulder.turnByOffset(offsetAngle);
    else if (jid == JointID.BASE)
      robotBase.turnByOffset(offsetAngle);
    else 
      Console.WriteLine("Unknown joint ID: " + jid);
  } // end of turnByOffset()


  public void turnToAngle(JointID jid, int angle)
  {
    if (jid ==  JointID.WRIST)
      wrist.turnToAngle(angle);
    else if (jid == JointID.ELBOW)
      elbow.turnToAngle(angle);
    else if (jid == JointID.SHOULDER)
      shoulder.turnToAngle(angle);
    else if (jid == JointID.BASE)
      robotBase.turnToAngle(angle);
    else 
      Console.WriteLine("Unknown joint ID: " + jid);
  } // end of turnToAngle()



  public void openGripper(bool isOpen)
  { armComms.openGripper(isOpen); }


  public void setLight(bool turnOn)
  {  armComms.setLight(turnOn);  }


  public void wait(int ms)
  { armComms.wait(ms);  }


  public void showAngles()
  {
    Console.WriteLine("Current Angles:");
    foreach(var j in joints)
      Console.Write( "  " + j.getName() + ": " + j.getCurrAngle());
    Console.WriteLine();
  }  // end of showAngles()


  // ------------------- inverse kinematics -------------------------


  public bool moveTo(Coord3D pt)
  {  return moveTo(pt.X, pt.Y, pt.Z);  }


  public bool moveTo(int x, int y, int z)
  {
    Console.WriteLine("-----");
    Console.WriteLine("Moving to (" + x + ", " + y + ", " + z + ")...");

    int[] angles = calcIK(x, y, z);
    if (angles == null)
      return false;

    if (!withinRanges(angles)) {
      Console.WriteLine("Move Cancelled");
      return false;
    }
    else {
      for (int i=0; i < angles.Length; i++)
        joints[i].turnToAngle(angles[i]);
      return true;
    }
  }  // end of moveTo()


  private bool withinRanges(int[] angles)
  // report angles that are out of range;
  // angles order in array must be same as joints[] array
  {
    bool hasProbs = false;
    // Console.WriteLine("IK angles for (" + x + ", " + y + ", " + z + ")");
    for (int i=0; i < angles.Length; i++) {
      // Console.WriteLine("  " + joints[i].getName() + ": " + angles[i]);
      if (!joints[i].isInRange(angles[i])) {
        Console.WriteLine("  " + joints[i].getName() + " out of range");
        hasProbs = true;
      }
    }
    return !hasProbs;
  }  // end of withinRanges()



  private int[] calcIK(int x, int y, int z)
  /* Use (x,y,z) coordinate to calculate the IK
     angles for the base, elbow, shoulder and wrist joints. 
  */
  {
    int extent2 = (x*x) + (y*y);
    int maxExtent = GRIPPER_LEN + LOWER_ARM + UPPER_ARM;
    if (extent2 > (maxExtent*maxExtent)) {
      Console.WriteLine("Coordinate (" + x + ", " + y + ", " + z +
                           ") is too far away on the XY plane");
      return null;
    }

    // base angle and radial distance from x,y coordinates 
    double baseAngle = Math.Atan2(x,y) * 180.0 / Math.PI;
    double rdist = Math.Sqrt((x*x) + (y*y));
          // radial distance now treated as the y coordinate for the arm 
    
    // wrist position 
    double wristZ = z - BASE_HEIGHT; 
    double wristY = rdist - GRIPPER_LEN;
    // System.out.printf("wrist (y,z): (%.2f, %.2f)\n", wristY, wristZ);

    // shoulder-wrist squared distance (swDist2)
    double swDist2 = (wristZ * wristZ) + (wristY * wristY);
    // System.out.printf("swDist2: %.2f\n", swDist2);

    // shoulder-wrist angle to ground 
    double swAngle1 = Math.Atan2(wristZ, wristY);
  
    double triVal = (UPPER_ARM*UPPER_ARM + swDist2 - LOWER_ARM*LOWER_ARM) / 
                               (2 * UPPER_ARM * Math.Sqrt(swDist2));
    // System.out.printf("triVal: %.8f\n", triVal);
    if (triVal > 1.0) {
      Console.WriteLine("Arm not long enough to reach coordinate");
      return null;
    }
    double swAngle2 = Math.Acos(triVal);
    // System.out.printf("swAngles: %.2f, %.2f)\n", 
    //                       Math.toDegrees(swAngle1), Math.toDegrees(swAngle2));

    double shoulderAngle = 90.0 - ((swAngle1 + swAngle2) * 180.0 / Math.PI);
  
    // elbow angle 
    double ewAngle = Math.Acos(
              (UPPER_ARM*UPPER_ARM + LOWER_ARM*LOWER_ARM - swDist2) / 
              (2 * UPPER_ARM * LOWER_ARM) );
    double elbowAngle = 180.0 - (ewAngle * 180.0 / Math.PI);
  
    double wristAngle = 90.0 - (shoulderAngle + elbowAngle);

    // round angles to integers
    int baseAng = (int)Math.Round(baseAngle);
    int shoulderAng = (int)Math.Round(shoulderAngle);
    int elbowAng = (int)Math.Round(elbowAngle);
    int wristAng = (int)Math.Round(wristAngle);

    int[] angles = new int[] {baseAng, wristAng, shoulderAng, elbowAng};
    return angles;      // order must be same as joints[]
  }  // end of calcIK()


  // ------------------- forward kinematics -------------------------


  public Coord3D getCoord()
  {  
    int baseAngle = robotBase.getCurrAngle();
    int shoulderAngle = shoulder.getCurrAngle();
    int elbowAngle = elbow.getCurrAngle();
    // wristAngle not needed

    if (!robotBase.isInRange(baseAngle))
      Console.WriteLine("  base angle (" + baseAngle + ") out of range");

    if (!shoulder.isInRange(shoulderAngle))
      Console.WriteLine("  shoulder angle (" + shoulderAngle + ") out of range");

    if (!elbow.isInRange(elbowAngle))
      Console.WriteLine("  elbow angle (" + elbowAngle + ") out of range");

    double baseAng = baseAngle * Math.PI / 180.0; // to radians
    double shoulderAng = shoulderAngle * Math.PI / 180.0;
    double elbowAng = elbowAngle * Math.PI / 180.0;

    int seAngle = 180 - (shoulderAngle + elbowAngle);
    double seAng =  seAngle * Math.PI / 180.0;

    double radialDist = UPPER_ARM*Math.Sin(shoulderAng) +
                        LOWER_ARM*Math.Sin(seAng) + GRIPPER_LEN;
    int x = (int)Math.Round( radialDist*Math.Sin(baseAng) );
    int y = (int)Math.Round( radialDist*Math.Cos(baseAng) );

    int z = (int)Math.Round( BASE_HEIGHT + UPPER_ARM*Math.Cos(shoulderAng) -
                            LOWER_ARM*Math.Cos(seAng) );
   
    Coord3D pt = new Coord3D(x, y, z);
    // Console.WriteLine("getCoord() pos: " + pt);

    return pt;
  }  // end of getCoord()


  // -------------------------- item movement --------------------------

  public void moveItem(Coord3D fromPt, Coord3D toPt)
  {
    bool hasMoved = moveTo(fromPt);
    showAngles();
    Console.WriteLine("From Coord: " + getCoord() );

    if (hasMoved) {
      openGripper(false);
      
      turnByOffset(JointID.ELBOW, -30);     // so off the floor
      Console.WriteLine("Off-floor Coord: " + getCoord() );
      
      moveTo(toPt);   // in mm; no checking of result
      showAngles();
      Console.WriteLine("To Coord: " + getCoord() );
      openGripper(true);
    }
  }  // end of moveItem()



  // ------------------------------------ test rig --------------------------
  // MODIFY main() to test out different arm movements

  public static void Main(string[] args)
  {
    RobotArm robotArm = new RobotArm();

    // tests that cause errors:
    // boolean hasMoved = robotArm.moveTo(0, 300, 130);   // reaching too far
    // boolean hasMoved = robotArm.moveTo(150, 250, 130);  // too far
    // boolean hasMoved = robotArm.moveTo(0, 8, 65);    
                          // wrist and shoulder out of range
    // boolean hasMoved = robotArm.moveTo(0, 150, 65);      // elbow out of range

    // tests in methods:
    // moveInLine(robotArm);
    // shiftItem(robotArm);

    // move to, grab, move to, release
    //robotArm.openGripper(true);   // open gripper
    robotArm.moveTo(150, 250, 65);   // coord given in mm
    robotArm.wait(1000);
    robotArm.openGripper(false);    // close


    // robotArm.moveTo(-150, 200, 65);   // in mm
    robotArm.moveToZero();
    robotArm.openGripper(true);     // open

    // report angles and coords
    robotArm.showAngles();
    Console.WriteLine("Coord: " + robotArm.getCoord() );

    robotArm.close();
  }  // end of main()



  private static void shiftItem(RobotArm robotArm)
  // moves object from one coord to another
  {
    Coord3D fromPt =  new Coord3D(150, 150, 65);     // in mm
    Coord3D toPt =  new Coord3D(-150, 200, 65);

    robotArm.moveItem(fromPt, toPt);
  }  // end of shiftItem()



  private static void moveInLine(RobotArm robotArm)
  // moves arm in a straight line 
  {
     for(int x = -100; x <= 100; x += 20) {
       robotArm.moveTo(x, 200, 80);
       robotArm.wait(500);
     }
  }  // end of moveInLine()


}  // end of RobotArm class

}
