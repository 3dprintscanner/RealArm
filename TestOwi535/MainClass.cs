using System;
using System.Runtime.InteropServices;
using System.Threading;

using LibUsbDotNet;
using LibUsbDotNet.Main;
using MonoLibUsb;

namespace TestOwi535
{
    //First byte
    //Gripper close == 0x01
    //Gripper open == 0x02
    //Wrist Up == 0x04
    //Wrist Down == 0x08
    //Elbow up == 0x10
    //Elbow down == 0x20
    //Shoulder up == 0x40
    //Shoulder down == 0x80

    //Second byte
    //Base rotate right == 0x01
    //Base rotate left == 0x02

    //Third byte
    //Light on == 0x01

    class MainClass
    {
        public static void Main(string[] args)
        {
            MoveWith_LibUsbDotNet();
        }

        private static void MoveWith_LibUsbDotNet()
        {
            ErrorCode ec = ErrorCode.None;
            UsbDevice robotArm = null;
            try
            {
                // Find and open the usb device.
                robotArm = UsbDevice.OpenUsbDevice(new UsbDeviceFinder(0x1267, 0x0000));

                // If the device is open and ready7
                if (robotArm == null)
                {
                    Console.WriteLine("Device Not Found.");
                    return;
                }


                // If this is a "whole" usb device (libusb-win32, linux libusb)
                // it will have an IUsbDevice interface. If not (WinUSB) the 
                // variable will be null indicating this is an interface of a 
                // device.
                IUsbDevice wholeUsbDevice = robotArm as IUsbDevice;
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


                UsbSetupPacket usbPacket;
                int transferred;
                byte[] data;

                //data = new byte[] { 4, 0, 0 };
                data = new byte[] { 0, 0, 1 };
                IntPtr dat = Marshal.AllocHGlobal(3);
                Marshal.Copy(data, 0, dat, 3);

                usbPacket = new UsbSetupPacket((byte)UsbRequestType.TypeVendor, 6, 0x0100, 0, 0);
                robotArm.ControlTransfer(ref usbPacket, dat, data.Length, out transferred);
                Thread.Sleep(1000);

                data[0] = 0;
                data[1] = 0;
                data[2] = 0;
                Marshal.Copy(data, 0, dat, 3);
                robotArm.ControlTransfer(ref usbPacket, dat, data.Length, out transferred);

            }
            catch (Exception ex)
            {
                Console.WriteLine();
                Console.WriteLine((ec != ErrorCode.None ? ec + ":" : String.Empty) + ex.Message);
            }
            finally
            {
                if (robotArm != null)
                {
                    if (robotArm.IsOpen)
                    {
                        // If this is a "whole" usb device (libusb-win32, linux libusb-1.0)
                        // it exposes an IUsbDevice interface. If not (WinUSB) the 
                        // 'wholeUsbDevice' variable will be null indicating this is 
                        // an interface of a device; it does not require or support 
                        // configuration and interface selection.
                        IUsbDevice wholeUsbDevice = robotArm as IUsbDevice;
                        if (!ReferenceEquals(wholeUsbDevice, null))
                        {
                            // Release interface #0.
                            wholeUsbDevice.ReleaseInterface(0);
                        }

                        robotArm.Close();
                    }
                    robotArm = null;

                    // Free usb resources
                    UsbDevice.Exit();

                }
            }
        }
    }
}
