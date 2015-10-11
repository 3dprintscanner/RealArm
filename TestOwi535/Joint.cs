using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestOwi535
{
    /* Class for rotating a joint in the robot arm

   Each joint instance maintains state information about its joint:
     * its current rotation angle (currAngle);
     * its rotation rate (rotRate), so a rotation 
       angle can be translated into a rotation time;
     * rotation limits (posLimit, negLimit) beyond which a rotation
       is impossible

    The state information is read from (and written to) a file called
    <JointID>JI.txt, which has the form:
          rotate:  <rotTime>
          limits: <posLimit> <negLimit>
          angle: <currAngle>

    The loaded rotation time is converted into a rotation rate.
*/

public class Joint
{
  private const String JOINT_INFO_EXT = "JI.txt";

  private const int MIN_TIME = 200;   // ms
             // don't move if time is less than MIN_TIME

  private JointID jointID;
  private ArmCommunicator armComms;

  // data read in from joint info file
  private int rotTime;          
            // time (in ms) for the joint to move between its limits
  private int posLimit, negLimit;   // in degrees    (negLimit is -ve)
  private int currAngle;            // may range between posLimit and negLimit 

  private float rotRate;      // joint rotation rate (between limits)



  public Joint(JointID id, ArmCommunicator ac)
  {  
    jointID = id;
    armComms = ac;

    readJointInfo(getName() + JOINT_INFO_EXT);
    checkInfo();
  }  // end of Joint()


  public String getName()
  {  return jointID.ToString().ToLower(); }


  public int getCurrAngle()
  {  return currAngle; }


  public void saveState()
  {  writeJointInfo(getName() + JOINT_INFO_EXT);  }



  public void turnByOffset(int offsetAngle)
  /* turn by specfied offset, which may be
     positive or negative; +ve is forwards/right,
     -ve is backwards/left) */
  {
    int newAngle = withinLimits(currAngle + offsetAngle);
    timedAngleTurn(newAngle);
  }  // end of turnByOffset()



  private int withinLimits(int angle)
  // adjust angle if outside limits
  {
    if (angle >= posLimit) {
      Console.WriteLine("  " + jointID + ": Angle (" + angle +
                                ") exceeds +ve limit; turning to limit only");
      angle = posLimit-1;
    }
    else if (angle <= negLimit) {
      Console.WriteLine("  " + jointID + ": Angle (" + angle +
                                   ") exceeds -ve limit; turning to limit only");
      angle = negLimit+1;
    }
    return angle;
  }  // end of withinLimits()



  private void timedAngleTurn(int angle)
  // move to angle by executing a timed rotation
  {
    Console.WriteLine("  " + jointID + " angle turn to: " + angle);
    int offsetAngle = angle - currAngle;     // offset may be +ve or -ve

    if (offsetAngle < 0) {
      int time = Convert.ToInt32(Math.Round(-offsetAngle/rotRate));
      if (time < MIN_TIME)
        Console.WriteLine("  " + jointID + ": -ve turn time too short (" +
                             time + "); ignoring");
      else {
        armComms.turn(jointID, ArmCommunicator.NEGATIVE, time);
        currAngle = angle;
      }
    }
    else {    // offset is +ve
      int time = Convert.ToInt32(Math.Round( offsetAngle/rotRate ));
      if (time < MIN_TIME)
        Console.WriteLine("  " + jointID + ": +ve turn time too short (" +
                             time + "); ignoring");
      else {
        armComms.turn(jointID, ArmCommunicator.POSITIVE, time);
        currAngle = angle;
      }
    }
  }  // end of timedAngleTurn()

    private void compositeAngleTurn(int[] angles)
    {
        List<Move> moves = new List<Move>();
        for (int i = 0; i < angles.Length; i++)
        {
           
            var angle = angles[i];
            Console.WriteLine("  " + jointID + " angle turn to: " + angle);
            int offsetAngle = angle - currAngle;     // offset may be +ve or -ve

            if (offsetAngle < 0)
            {
                int time = Convert.ToInt32(Math.Round(-offsetAngle / rotRate));
                if (time < MIN_TIME)
                    Console.WriteLine("  " + jointID + ": -ve turn time too short (" +
                                         time + "); ignoring");
                else
                {
                    //armComms.turn(jointID, ArmCommunicator.NEGATIVE, time);
                    moves.Add(new Move(){JointId = jointID , direction = ArmCommunicator.NEGATIVE , time = time});
                    currAngle = angle;
                }
            }
            else
            {    // offset is +ve
                int time = Convert.ToInt32(Math.Round(offsetAngle / rotRate));
                if (time < MIN_TIME)
                    Console.WriteLine("  " + jointID + ": +ve turn time too short (" +
                                         time + "); ignoring");
                else
                {
                    //armComms.turn(jointID, ArmCommunicator.POSITIVE, time);
                    moves.Add(new Move(){JointId = jointID,direction = ArmCommunicator.POSITIVE, time = time});
                    currAngle = angle;
                }
            }
        }
        armComms.compositeTurn(moves);

    }

    public void turnToAngle(int angle)
  /* turn to specified angle (+ve is forwards/right,
     -ve is backwards/left) */
  {  timedAngleTurn( withinLimits(angle) );  }



  public bool isInRange(int angle)
  {  return ((angle > negLimit) && (angle < posLimit));  }



 // ------ read and write joint info to a file ---------------


  private void readJointInfo(String fnm)
  /* read joint info lines
          rotate:  <rotTime int>
          limits: <posLimit int> <negLimit int>
          angle: <currAngle int>
  */
  {
    Console.WriteLine("Opening joint info file: " + fnm);
    String line;
    String[] toks;
    try {
        var streamReader = 
            new System.IO.StreamReader(fnm);

      // get rotTime
      line = streamReader.ReadLine();
      toks = line.Split(' ');
      if(!int.TryParse(toks[1], out rotTime))
      {
          Console.WriteLine("Error reading line \"" + line + "\"");
      }
      

      // get pos and neg limits
      line = streamReader.ReadLine();
      toks = line.Split(' ');
      if(!int.TryParse(toks[1], out posLimit))
      {
          Console.WriteLine("Error reading line \"" + line + "\"");
      }
      if(!int.TryParse(toks[2], out negLimit))
      {
          Console.WriteLine("Error reading line \"" + line + "\"");
      }

      // get current angles
      line = streamReader.ReadLine();
      toks = line.Split(' ');
      if(!int.TryParse(toks[1], out currAngle))
      {
          Console.WriteLine("Error reading line \"" + line + "\"");
      }
      streamReader.Close();
/*
      System.out.println("  rotTime: " + rotTime);
      System.out.println("  posLimit: " + posLimit + "; negLimit: " + negLimit);
      System.out.println("  currAngle: " + currAngle);
      System.out.println("-------");
*/
    }
    catch (Exception e)
    { Console.WriteLine("Could not read joint info from " + fnm );
      throw;
    }
  }  // end of readJointInfo()



  private void checkInfo()
  {
    // check rotation time 
    if (rotTime == 0) {
      Console.WriteLine("Positive rotation time cannot be 0");
      throw new JointException("Positive rotation time cannot be 0");
    }
    else if (rotTime < 0) {
      Console.WriteLine("Rotation time must be positive");
      rotTime = -rotTime;
    }

    // check positive angle limit 
    if (posLimit == 0) {
      Console.WriteLine("Positive limit cannot be 0");
      throw new JointException("Positive limit cannot be 0");
    }
    else if (posLimit < 0) {
      Console.WriteLine("Positive limit must be positive");
      posLimit = -posLimit;
    }

    // check negative angle limit 
    if (negLimit == 0) {
      Console.WriteLine("Negative limit cannot be 0");
      throw new JointException("Negative limit cannot be 0");
    }
    else if (negLimit > 0) {
      Console.WriteLine("Negative limit must be negative");
      negLimit = -negLimit;     // don't negative value
    }

    rotRate = ((float)(posLimit-negLimit))/rotTime;

    // check current angle 
    if ((currAngle >= posLimit) || (currAngle <= negLimit)) {
      Console.WriteLine("Rest angle out of range; using 0");
      currAngle = 0;
    }
  }  // end of checkInfo()

    public void zeroJoint()
    {
        this.currAngle = 0;
    }

    private void writeJointInfo(String fnm)
  // write out three lines for the joint info
  {
    try {
      var lines = new [] {
          "rotate: " + rotTime,
          "limits: " + posLimit + " " + negLimit,
          "angle: " + currAngle,
      };
      System.IO.File.WriteAllLines(fnm, lines);
      Console.WriteLine("Saved joint info to " + fnm);
    }
    catch (Exception e)
    {  Console.WriteLine("Could not save joint info to " + fnm); }
  }  // end of writeJointInfo()


}  // end of Joint class

}
