using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UartCom;
using System.Threading;

namespace TestDLL
{
    class Program
    {
        static void Main(string[] args)
        {
            string sn = "";
            Console.WriteLine("Starting Test Sequence...");

            //Uart Init START
                ErrorTestStand errorTS = new ErrorTestStand();
                UartDriver SmartHalo = new UartDriver("COM4", 115200, "C:\\LOG\\dll-pretest3.log ");
                errorTS = SmartHalo.LoopbackTest("uart");
            //END
            
            //SN & info START
                errorTS = SmartHalo.SetDeviceSerialNumber("12", "key");
                errorTS = SmartHalo.GetDeviceSerialNumber(ref sn, "key");
                errorTS = SmartHalo.SetDeviceSerialNumber("34", "lock");
                errorTS = SmartHalo.GetDeviceSerialNumber(ref sn, "lock");
                errorTS = SmartHalo.SetDeviceSerialNumber("56", "product");
                errorTS = SmartHalo.GetDeviceSerialNumber(ref sn, "product");
                errorTS = SmartHalo.SetDeviceSerialNumber("78", "PCBA");
                errorTS = SmartHalo.GetDeviceSerialNumber(ref sn, "PCBA");
                errorTS = SmartHalo.GetFirmwareVersion(ref sn);
                errorTS = SmartHalo.GetSoftDeviceVersion(ref sn);
                errorTS = SmartHalo.GetBootloaderVersion(ref sn);
            //END

            //Magnetometer & Accelerometer START
                errorTS = SmartHalo.Selftest("magnetometer", 60);
                errorTS = SmartHalo.Selftest("accelerometer", 60);
            //END 

            //Bluetooth START
                //errorTS = SmartHalo.SetTxPower();
                //errorTS = SmartHalo.GetTxPower();
                //errorTS = SmartHalo.GetRSSI();
            //END

            //Led test START
                //RED
                errorTS = SmartHalo.TurnLedOn(1,25,"off"); 
                errorTS = SmartHalo.SetLedColor(1, 25, 255, 0, 0);
                for (byte i = 1; i <= 25; i++)
                {
                    errorTS = SmartHalo.TurnLedOn(i, "on");
                    Thread.Sleep(100);
                    errorTS = SmartHalo.TurnLedOn(i, "off");
                }
                //GREEN
                errorTS = SmartHalo.SetLedColor(1, 25, 0, 255, 0);
                for (byte i = 1; i <= 25; i++)
                {
                    errorTS = SmartHalo.TurnLedOn(i, "on");
                    Thread.Sleep(100);
                    errorTS = SmartHalo.TurnLedOn(i, "off");
                }
                //BLUE
                errorTS = SmartHalo.SetLedColor(1, 25, 0, 0, 255);
                for (byte i = 1; i <= 25; i++)
                {
                    errorTS = SmartHalo.TurnLedOn(i, "on");
                    Thread.Sleep(100);  
                    errorTS = SmartHalo.TurnLedOn(i, "off");
                }
                //FRONT
                errorTS = SmartHalo.SetFrontLedIntensity(255);
                Thread.Sleep(100);
                errorTS = SmartHalo.SetFrontLedIntensity(0);
            //END

            //errorTS = SmartHalo.ExitIntoShippingMode();
            Console.WriteLine("DONE");
            Console.ReadLine();
        }
    }
}

/* InternalLogger Snippet

            Console.WriteLine("Creating Log...");
            InternalLogger log = new InternalLogger("C:\\Log\\dll-07.log");
            log.AppendLine("01");
            log.AppendLine("02");
            log.AppendLine("03");
            Console.ReadLine();          
 */ 
