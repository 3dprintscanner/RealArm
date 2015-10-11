using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using LibUsbDotNet;
using LibUsbDotNet.Main;
using System.Windows;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Documents;

namespace TestOwi535
{

    // ArmCommunicator.java
    // Andrew Davison, ad@fivedots.coe.psu.ac.th, June 2011

    /* Class for rotating the Maplin Robot Arm
       (http://www.maplin.co.uk/robotic-arm-kit-with-usb-pc-interface-266257) 
       It's the UK version of the US OWI-535
       (http://www.owirobot.com/products/Robotic-Arm-Edge.html)
        with the OWI USB interface included

       --------
       The robot arm ops are spread over three bytes:
   
       First byte:
         Gripper close == 0x01     Gripper open == 0x02
         Wrist forwards == 0x04          Wrist backwards == 0x08
         Elbow forwards == 0x10          Elbow backwards == 0x20
         Shoulder forwards == 0x40       Shoulder backwards == 0x80
   
       Second byte:
         Base rotate right == 0x01  Base rotate left == 0x02
   
       Third byte:
         Light on == 0x01
   
       Set all bytes to zero for all off
       --------

       This Java coded utilizes the libraries 
          * libusbjava (http://libusbjava.sourceforge.net/)
          * libusb-win32 (http://sourceforge.net/apps/trac/libusb-win32/wiki)

       Other requirements:
         - the Robot Arm must be plugged into a USB port;

         - a libusb-win32 device driver for the arm must have been created and
           installed into Windows (use inf-wizard.exe)

       Usage:
          > compile ArmCommunicator.java
          > run ArmCommunicator
    */


    public class ArmCommunicator
    {
      public const int POSITIVE = 0;
      public const int NEGATIVE = 1;
          // +ve for forward/right turns; -ve for backwards/left turns


      private const short VENDOR_ID = (short)0x1267;
      private const short PRODUCT_ID = (short)0x0;
              // the IDs were obtained by looking at the robot arm using USBDeview

      private const int GRIPPER_PERIOD = 1600;   // ms time to open/close


      private UsbDevice dev = null;   // used to communicate with the USB device

      // start state for the light
      private bool isLightOn = false;


      public ArmCommunicator()
      { 
        Console.WriteLine("Looking for device: (vendor: " + toHexString(VENDOR_ID) + 
                                              "; product: " + toHexString(PRODUCT_ID) + ")");
        dev = UsbDevice.OpenUsbDevice(new UsbDeviceFinder(VENDOR_ID, PRODUCT_ID));

        if (dev == null)
        {
            Console.WriteLine("Device Not Found.");
            throw new ArmCommunicatorException("Device Not Found.");
        }

        try {
          Console.WriteLine("Opening device");
          IUsbDevice wholeUsbDevice = dev as IUsbDevice;
          if (!ReferenceEquals(wholeUsbDevice, null))
          {
              Console.WriteLine("SetConfiguration && ClaimInterface");
              // This is a "whole" USB device. Before it can be used, 
              // the desired configuration and interface must be selected.

              // Select config #1
              wholeUsbDevice.SetConfiguration(1);

              // Claim interface #0.
              wholeUsbDevice.ClaimInterface(0);
          }
          // open device with configuration 1, interface 0 and no alt interface
        }
        catch (Exception e) {
          Console.WriteLine(e);
          throw;
        }
      }  // end of ArmCommunicator()



	
      public void close()
      {
        Console.WriteLine("Closing device");
        try {
            if (dev != null)
            {
                if (dev.IsOpen)
                {
                    // If this is a "whole" usb device (libusb-win32, linux libusb-1.0)
                    // it exposes an IUsbDevice interface. If not (WinUSB) the 
                    // 'wholeUsbDevice' variable will be null indicating this is 
                    // an interface of a device; it does not require or support 
                    // configuration and interface selection.
                    IUsbDevice wholeUsbDevice = dev as IUsbDevice;
                    if (!ReferenceEquals(wholeUsbDevice, null))
                    {
                        // Release interface #0.
                        wholeUsbDevice.ReleaseInterface(0);
                    }

                    dev.Close();
                }
                dev = null;

                // Free usb resources
                UsbDevice.Exit();

            }
        }
        catch (Exception e) {
          Console.WriteLine(e);
          throw;
        }
      }  // end of close()
	

      // ------------------------------ command ops --------------------------

      /*
       First byte:
         Gripper close == 0x01     Gripper open == 0x02
      */

      public void openGripper(bool isOpen)
      {  
        if (isOpen) {
          Console.WriteLine("  Gripper: open");
          sendCommand(0x02, 0x00, GRIPPER_PERIOD); 
        }
        else {
          Console.WriteLine("  Gripper: close");
          sendCommand(0x01, 0x00, GRIPPER_PERIOD); 
        }
      }  // end of openGripper()



      public void setLight(bool turnOn)
      // Third byte: Light on/off
      {  isLightOn = turnOn; 
         Console.WriteLine("  Light is on: " + isLightOn);
         sendControl(0x00, 0x00, getLightVal(isLightOn));
      }  


      private int getLightVal(bool isLightOn)
      // light on/off
      {  return (isLightOn)? 0x01 : 0x00;  }



      public void turn(JointID jid, int dir, int period)
      /* First byte:
         Wrist forwards == 0x04          Wrist backwards == 0x08
         Elbow forwards == 0x10          Elbow backwards == 0x20
         Shoulder forwards == 0x40       Shoulder backwards == 0x80
       * 
       * All Stop = 0x00
        Wrist Stop = 0x03, 0x0c
        Elbow Stop = 0x30
        Shoulder Stop = 0xc0
   
       Second byte:
         Base rotate right == 0x01  Base rotate left == 0x02
      */
      {
        int opCode1 = 0x00;
        int opCode2 = 0x00;

        if (jid == JointID.BASE)
          opCode2 = (dir == POSITIVE)? 0x01 : 0x02;
        else if (jid == JointID.SHOULDER)
          opCode1 = (dir == POSITIVE)? 0x80 : 0x40;
        else if (jid == JointID.ELBOW)
          opCode1 = (dir == POSITIVE)? 0x20 : 0x10;
        else if (jid == JointID.WRIST)
          opCode1 = (dir == POSITIVE)? 0x08 : 0x04;
        else
          Console.WriteLine("Unknown joint ID: " + jid);

        if (period < 0) {
          Console.WriteLine("Turn period cannot be negative");
          period = 0;
        }
  
        Console.WriteLine("  " + jid + " timed turn: " + dir + " " + period + "ms");
        sendCommand(opCode1, opCode2, period);
      }  // end of turn()



      private void sendCommand(int opCode1, int opCode2, int period)
      // execute the operation for period millisecs
      {
        int opCode3 = getLightVal(isLightOn);     // third byte == light on/off
        if (dev != null) {
          sendControl(opCode1, opCode2, opCode3);
          wait(period);
          sendControl(0, 0, opCode3);    // stop arm
        }
      }  // end of sendCommand()

        private void sendCommand(CodedMove codedMove)
        {
            sendControl(codedMove.opCode1,codedMove.opCode2,codedMove.opCode3);
            wait(codedMove.time);
        }




        private void sendControl(int opCode1, int opCode2, int opCode3)
      // send a USB control transfer
      {
    /*
        Console.WriteLine("Sending ops: <" + toHexString(opCode1) + ", " + 
                                              toHexString(opCode2) + ", " +
                                              toHexString(opCode3) + ">");
    */
        byte[] bytes = { Convert.ToByte(opCode1),
                         Convert.ToByte(opCode2),
                         Convert.ToByte(opCode3)
                       };
        try {
            UsbSetupPacket usbPacket;
            int transferred;
            IntPtr dat = Marshal.AllocHGlobal(3);
            Marshal.Copy(bytes, 0, dat, 3);

            usbPacket = new UsbSetupPacket((byte)UsbRequestType.TypeVendor, 6, 0x0100, 0, 0);
            dev.ControlTransfer(ref usbPacket, dat, bytes.Length, out transferred);


        }
        catch (Exception e) {
          Console.WriteLine(e);
        }
      }  // end of sendControl()



      private String toHexString(int b)
      // chanage the hexadecimal integer into "0x.." string format
      {  
        String hex = b.ToString("X2");  
        if (hex.Length == 1)
          return "0x0" + hex;
        else
          return "0x" + hex;
      }  // end of toHexString




      public void wait(int ms)
      // sleep for the specified no. of millisecs
      { 
        // Console.WriteLine("Waiting " + ms + " ms...");
        try {
          Thread.Sleep(ms);
        }
        catch(Exception e) {}
      }  // end of wait()


      // ------------------------------------ test rig --------------------------


      private const int DELAY = 250;


      public static void Main(string[] args)
      {
        ArmCommunicator arm = new ArmCommunicator();

        Console.WriteLine("Enter a single letter command (and <ENTER>:");
        printHelp();
        String line = null;
        char ch;

        Console.Write(">> ");
        while ((line = Console.ReadLine()) != null) {
          if (line.Length == 0)
            break;
          ch = line[0];
          if (ch == 'q')
            break;
          else if (ch == '?')
            printHelp();
          else
            doArmOp(ch, arm);
          Console.Write(">> ");
        }
         
        // restore start state
        // arm.openGripper(true);
        // arm.setLight(false);

        arm.close();
      }  // end of main()


      private static void printHelp()
      {
        Console.WriteLine("  Gripper: close == w ; open == s");
        Console.WriteLine("  Wrist:     fwd == e ; back == d");
        Console.WriteLine("  Elbow:     fwd == r ; back == f");
        Console.WriteLine("  Shoulder:  fwd == u ; back == j");
        Console.WriteLine("  Base:     left == k ; right == i");
        Console.WriteLine("  Light:      on == l ; off == p");
        Console.WriteLine("            quit == q ; help == ?");
      }  // end of printHelp()


        private static void doArmOp(char ch, ArmCommunicator arm)
            // use POSITIVE for forwards/right turns; NEGATIVE for backwards/left turns
        {
            if (ch == 'w') // gripper close
                arm.openGripper(false);
            else if (ch == 's') // gripper open
                arm.openGripper(true);
            else if (ch == 'e') // wrist forwards
                arm.turn(JointID.WRIST, ArmCommunicator.POSITIVE, DELAY);
            else if (ch == 'd') // wrist backwards
                arm.turn(JointID.WRIST, ArmCommunicator.NEGATIVE, DELAY);
            else if (ch == 'r') // elbow forwards
                arm.turn(JointID.ELBOW, ArmCommunicator.POSITIVE, DELAY);
            else if (ch == 'f') // elbow backwards
                arm.turn(JointID.ELBOW, ArmCommunicator.NEGATIVE, DELAY);
            else if (ch == 'u') // shoulder forwards
                arm.turn(JointID.SHOULDER, ArmCommunicator.POSITIVE, DELAY);
            else if (ch == 'j') // shoulder backwards
                arm.turn(JointID.SHOULDER, ArmCommunicator.NEGATIVE, DELAY);
            else if (ch == 'k') // base left
                arm.turn(JointID.BASE, ArmCommunicator.NEGATIVE, DELAY);
            else if (ch == 'i') // base right
                arm.turn(JointID.BASE, ArmCommunicator.POSITIVE, DELAY);
            else if (ch == 'l') // light on
                arm.setLight(true);
            else if (ch == 'p') // light off
                arm.setLight(false);
            else if (ch == 't')
            {
                arm.sendControl(0x04, 0x01, 0x01);
                arm.wait(1000);
                arm.sendControl(0x00, 0x00, 0x00);
            }
            else if (ch == '1')
            {
                arm.sendControl(0x31, 0x00, 0x00);
                arm.wait(1000);
                arm.sendControl(0x00, 0x00, 0x00);
            }
            else if (ch == '2')
            {
                arm.sendControl(0x07, 0x00, 0x00);
                arm.wait(1000);
                arm.sendControl(0x00, 0x00, 0x00);
            }
            else if (ch == '3'){
                arm.sendControl(0x03, 0x00, 0x00);
            arm.wait(1000);
            arm.sendControl(0x00, 0x00, 0x00);
        }

    else if (ch == '4')
    {
        arm.sendControl(0x04, 0x00, 0x00);
        arm.wait(1000);
        arm.sendControl(0x00, 0x00, 0x00);
    }
    else if (ch == '5')
    {
        arm.sendControl(0x20, 0x01, 0x00);
        Console.WriteLine("First Command");
        arm.wait(1000);
        arm.sendControl(0x30, 0x01, 0x00);
        Console.WriteLine("Second Command");
        arm.wait(1000);
        arm.sendControl(0x00, 0x00, 0x00);
    }
            else if (ch == '6')
            {
                arm.sendControl(0x58, 0x01, 0x00);
                arm.wait(1000);
                arm.sendControl(0x00, 0x00, 0x00);
            }
            else if (ch == '7')
            {
                arm.sendControl(0x40, 0x01, 0x00);
                Console.WriteLine("First Command");
                arm.wait(1000);
                arm.sendControl((0xc0+ 0x10), 0x01, 0x00);
                Console.WriteLine("Second Command");
                arm.wait(1000);
                arm.sendControl(0x00,0x00,0x00);
            }
    else
        Console.WriteLine("Unknown command: " + ch);
        }  // end of doArmOp() 

        public void CompositeTurn(List<Move> moves)
        {
            // For each move, order the moves by time, work out the gaps between the move times and create a scheduler to stop the moves
            // or create a thread which calls a function after a certain amount of time which sends the stop byte for the given joint
            var moveCodes = ProcessedMovesList(moves);
            foreach (var move in moveCodes)
            {
                sendCommand(move);
            }

            //sendCommand(opCode1, opCode2, period);
        }

        public List<CodedMove> ProcessedMovesList(List<Move> moves)
        {
            IOrderedEnumerable<Move> sortedMoves = moves.OrderBy(x => x.time);

            // create a set of all the moves required
            List<CodedMove> opCodedMoves = new List<CodedMove>();

            
            for (int i = 0; i < sortedMoves.Count(); i++)
            {
                var opCodes = GenerateOpCodes(sortedMoves.ElementAt(i).JointId, sortedMoves.ElementAt(i).direction,sortedMoves.ElementAt(i).time);
                var moveTime = (i == 0) ? sortedMoves.ElementAt(i).time : sortedMoves.ElementAt(i).time - sortedMoves.ElementAt(i -1).time;
                var codedMove = new CodedMove()
                {
                    opCode1 = opCodes[0],
                    opCode2 = opCodes[1],
                    opCode3 = 0x00,
                    time = moveTime
                };
                
                for (int j = i + 1; j < sortedMoves.Count(); j++)
                {
                    // get the opcodes of the rest of the sortedMoves
                    var opcode = GenerateOpCodes(sortedMoves.ElementAt(j).JointId, sortedMoves.ElementAt(j).direction,sortedMoves.ElementAt(j).time);
                    codedMove.opCode1 += opcode[0];
                    codedMove.opCode2 += opcode[1];
                }
            
                if (i > 0)
                {
                    codedMove.opCode1 += GetCancelOpcode(sortedMoves.ElementAt(i - 1).JointId)[0];
                    codedMove.opCode2 += GetCancelOpcode(sortedMoves.ElementAt(i - 1).JointId)[1];
                }

                // the first byte value is the sum of the opcodes for the moves at a time
                opCodedMoves.Add(codedMove);
            }
            var finalTime = 10;
            var finalMove = new CodedMove(){opCode1 = 0x00, opCode2 = 0x00, opCode3 = 0x00, time = finalTime};
            opCodedMoves.Add(finalMove);
            
            return opCodedMoves;

        }

        private int[] GetCancelOpcode(JointID jointId)
        {
            switch (jointId)
            {
                case (JointID.BASE):
                    return new int[]{0x00,0x03};                   
                case(JointID.ELBOW):
                    return new int[]{0x30,0x00};
                case(JointID.WRIST):
                    return new int[]{0x0c,0x00};
                case(JointID.SHOULDER):
                    return new int[]{0xc0,0x00};
                default:
                    return new int[]{0x00,0x00};
            }
        }

        private int[] GenerateOpCodes(JointID jid, int dir, int time)
        {
            int opCode1 = 0x00;
            int opCode2 = 0x00;
            
            if(time == 0) return new int[]{0x00, 0x00};

            if (jid == JointID.BASE)
                opCode2 = (dir == POSITIVE) ? 0x01 : 0x02;
            else if (jid == JointID.SHOULDER)
                opCode1 = (dir == POSITIVE) ? 0x80 : 0x40;
            else if (jid == JointID.ELBOW)
                opCode1 = (dir == POSITIVE) ? 0x20 : 0x10;
            else if (jid == JointID.WRIST)
                opCode1 = (dir == POSITIVE) ? 0x08 : 0x04;
            else
                Console.WriteLine("Unknown joint ID: " + jid);


            var opCodes = new int[] {opCode1, opCode2};
            return opCodes;

        }
    }


// end of ArmCommunicator class

}
