using System;
using System.Text;
using System.IO;
using System.Reflection;
using System.Diagnostics;
using System.Globalization;
using System.IO.Ports;
using System.Threading;

namespace UartCom
{
    /// <summary>
    /// Structure for error management in NI TestStand/LabView.
    /// </summary>
    public struct ErrorTestStand
    {
        public double Code;
        public string Msg;
        public bool Occurred;

        /// <summary>
        /// Set default value for the structure.
        /// </summary>
        /// <param name="defaultCode">Default error code</param>
        public static implicit operator ErrorTestStand(double defaultCode)
        {
            return new ErrorTestStand() { Code = defaultCode, Msg = null, Occurred = false };
        }
    }

    public class UartDriver
    {
        /////////////
        //Constants//
        /////////////

        //Serial Communication variable
        private string _comPort = "COM1";
        private int _baudRate = 115200;
        private const Parity _parity = Parity.None;
        private const int _dataBits = 8;
        private const StopBits _stopBits = StopBits.One;
        private SerialPort _serialPort;

        //Test Stand Error Management
        private const double _errorCode = 1000;
        private ErrorTestStand _errorTS = _errorCode;

        //Log
        private InternalLogger _log;
        private string _path = "C:\\Log\\defaultpath.log";

        ///////////////////////////////
        //Uart Initialization methods//
        ///////////////////////////////

        /// <summary>
        /// Explicit class constructor.
        /// </summary>
        public UartDriver(string comPort, int baudRate, string path)
        {
            _baudRate = baudRate;
            _comPort = comPort;
            _path = path;
            _log = new InternalLogger(_path);

            try
            {
                _serialPort = new SerialPort(_comPort, _baudRate, _parity, _dataBits, _stopBits);
                _serialPort.Open();
                _log.AppendLine("Boot >> Ready");
                _serialPort.DiscardInBuffer();
                _serialPort.DiscardOutBuffer();   
            }
            catch (Exception e)
            {
                _errorTS.Code = _errorCode + 10;
                _errorTS.Msg = string.Format("Unexpected exception.\r\nDEBUG INFO: {0}", e);
                _errorTS.Occurred = true;
            }
        }

        public ErrorTestStand LoopbackTest(string protocol)
        {
            string _protocol = "";
            string result = "";

            try
            {
                switch (protocol.ToLower())
                {
                    case "uart":
                        _protocol = "USB";
                        break;
                    case "bluetooth":
                        _protocol = "BLE";
                        break;
                    default:
                        _protocol = "ERROR";
                        break;
                }

                _log.AppendLine(string.Format("Loopback {0} >> Start", _protocol));

                //Request Loopback test to the DUT.
                _serialPort.Write(string.Format("LoopbackTest {0}\r\n", _protocol));
                result = _serialPort.ReadLine();
                _log.AppendLine(string.Format("Loopback {0} >> {1}", _protocol, result));

                //Send an echo.
                _serialPort.Write(string.Format("ECHO\r\n", _protocol));
                Thread.Sleep(100);
                result = _serialPort.ReadExisting();
                _log.AppendLine(string.Format("Loopback {0} >> {1}", _protocol, result));

                //Check if the DUT answer correctly.
                if (!result.Contains("ECHO"))
                {
                    _errorTS.Code = _errorCode + 10;
                    _errorTS.Msg = "UART communication with the DUT cannot be established.\r\n";
                    _errorTS.Occurred = true;
                }

                //Stop Loopback test.
                _serialPort.Write(string.Format("\x1bSTOP\r\n", _protocol));
                result = _serialPort.ReadLine();
                _log.AppendLine(string.Format("Loopback {0} >> {1}", _protocol, result));

            }
            catch (Exception e)
            {
                _errorTS.Code = _errorCode + 10;
                _errorTS.Msg = string.Format("Unexpected exception.\r\nDEBUG INFO: {0}", e);
                _errorTS.Occurred = true;
            }

            return _errorTS;
        }

        public ErrorTestStand UartDispose()
        {
            try
            {
                if (_serialPort.IsOpen)
                {
                    _serialPort.DiscardInBuffer();
                    _serialPort.DiscardOutBuffer();
                    _serialPort.Close();
                    _serialPort.Dispose();
                }
            }
            catch (Exception e)
            {
                _errorTS.Code = _errorCode + 10;
                _errorTS.Msg = string.Format("Unexpected exception.\r\nDEBUG INFO: {0}", e);
                _errorTS.Occurred = true;
            }

            return _errorTS;
        }

        ////////////////////////
        //Get-Set Info methods//
        ////////////////////////

        public ErrorTestStand GetDeviceSerialNumber(ref string sn, string component)
        {
            string sntype = "";
            try
            {
                switch (component.ToLower())
                {
                    case "lock": sntype = "Lock";
                        break;
                    case "key": sntype = "Key";
                        break;
                    case "product": sntype = "Product";
                        break;
                    case "pcba": sntype = "PCBA";
                        break;
                    default: sntype = "ERROR";
                        break;
                }
      
                _log.AppendLine(sntype + " S/N >> Request");
                _serialPort.Write(string.Format("GetSerialNumber {0}\r\n", component));
                sn = _serialPort.ReadLine();
                _log.AppendLine(sntype + " S/N >> " + sn);

            }
            catch (Exception e)
            {
                _errorTS.Code = _errorCode + 10;
                _errorTS.Msg = string.Format("Unexpected exception.\r\nDEBUG INFO: {0}", e);
                _errorTS.Occurred = true;
            }

            return _errorTS;
        }

        public ErrorTestStand SetDeviceSerialNumber(string sn, string component)
        {
            string result = "";
            string sntype = "";

            try
            {
                switch (component.ToLower())
                {
                    case "lock":
                        sntype = "Lock";
                        break;
                    case "key":
                        sntype = "Key";
                        break;
                    case "product":
                        sntype = "Product";
                        break;
                    case "pcba":
                        sntype = "PCBA";
                        break;
                    default:
                        sntype = "ERROR";
                        break;
                }
             
                _log.AppendLine(sntype + " S/N >> Set");
                _serialPort.Write(string.Format("SetSerialNumber {0} {1}\r\n", component, sn));
                result = _serialPort.ReadLine();
                _log.AppendLine(sntype + " Result >> " + result);
                
            }
            catch (Exception e)
            {
                _errorTS.Code = _errorCode + 10;
                _errorTS.Msg = string.Format("Unexpected exception.\r\nDEBUG INFO: {0}", e);
                _errorTS.Occurred = true;
            }

            return _errorTS;
        }

        public ErrorTestStand GetFirmwareVersion(ref string fv)
        {
            try
            {                
                _log.AppendLine("Firmware Version >> Request");
                _serialPort.Write("GetFirmwareVersion\r\n");
                fv = _serialPort.ReadLine();
                _log.AppendLine("Firmware Version >> " + fv);       
            }
            catch (Exception e)
            {
                _errorTS.Code = _errorCode + 10;
                _errorTS.Msg = string.Format("Unexpected exception.\r\nDEBUG INFO: {0}", e);
                _errorTS.Occurred = true;
            }

            return _errorTS;
        }

        public ErrorTestStand GetSoftDeviceVersion(ref string sv)
        {
            try
            {       
                _log.AppendLine("Softdevice Version >> Request");
                _serialPort.Write("GetSoftdeviceVersion\r\n");
                sv = _serialPort.ReadLine();
                _log.AppendLine("Softdevice Version >> " + sv);          
            }
            catch (Exception e)
            {
                _errorTS.Code = _errorCode + 10;
                _errorTS.Msg = string.Format("Unexpected exception.\r\nDEBUG INFO: {0}", e);
                _errorTS.Occurred = true;
            }

            return _errorTS;
        }

        public ErrorTestStand GetBootloaderVersion(ref string bv)
        {
            try
            {       
                _log.AppendLine("Bootloader Version >> Request");
                _serialPort.Write("GetBootloaderVersion\r\n");
                bv = _serialPort.ReadLine();
                _log.AppendLine("Bootloader Version >> " + bv);           
            }
            catch (Exception e)
            {
                _errorTS.Code = _errorCode + 10;
                _errorTS.Msg = string.Format("Unexpected exception.\r\nDEBUG INFO: {0}", e);
                _errorTS.Occurred = true;
            }

            return _errorTS;
        }

        ////////////////////
        //Halo LED methods//
        ////////////////////

        public ErrorTestStand SetLedColor(byte lednumber, byte red, byte green, byte blue)
        {
            string result = "";

            try
            {       
                _log.AppendLine(string.Format("LED {0} >> Set color", lednumber));
                _serialPort.Write(string.Format("SetLedColor {0} {1} {2} {3}\r\n", lednumber, red, green, blue));
                result = _serialPort.ReadLine();
                _log.AppendLine("Result >> " + result);              
            }
            catch (Exception e)
            {
                _errorTS.Code = _errorCode + 10;
                _errorTS.Msg = string.Format("Unexpected exception.\r\nDEBUG INFO: {0}", e);
                _errorTS.Occurred = true;
            }

            return _errorTS;
        }

        public ErrorTestStand SetLedColor(byte fromled, byte toled, byte red, byte green, byte blue)
        {
            string result = "";

            try
            {              
                _log.AppendLine(string.Format("LED {0}-{1} >> Set color", fromled, toled));
                _serialPort.Write(string.Format("SetLedColor {0} - {1} {2} {3} {4}\r\n", fromled, toled, red, green, blue));
                result = _serialPort.ReadLine();
                _log.AppendLine("Result >> " + result);               
            }
            catch (Exception e)
            {
                _errorTS.Code = _errorCode + 10;
                _errorTS.Msg = string.Format("Unexpected exception.\r\nDEBUG INFO: {0}", e);
                _errorTS.Occurred = true;
            }

            return _errorTS;
        }


        public ErrorTestStand TurnLedOn(byte lednumber, string state)
        {
            string result = "";
            string _state = state.ToLower();

            try
            {  
                _log.AppendLine(string.Format("LED {0} >> Set {1}", lednumber, _state));
                _serialPort.Write(string.Format("TurnLedOn {0} {1}\r\n", lednumber, _state));
                result = _serialPort.ReadLine();
                _log.AppendLine("Result >> " + result);   
            }
            catch (Exception e)
            {
                _errorTS.Code = _errorCode + 10;
                _errorTS.Msg = string.Format("Unexpected exception.\r\nDEBUG INFO: {0}", e);
                _errorTS.Occurred = true;
            }

            return _errorTS;
        }

        public ErrorTestStand TurnLedOn(byte fromled, byte toled, string state)
        {
            string result = "";
            string _state = state.ToLower();

            try
            {             
                _log.AppendLine(string.Format("LED {0}-{1} >> Set {2}", fromled, toled, _state));
                _serialPort.Write(string.Format("TurnLedOn {0} - {1} {2}\r\n", fromled, toled, _state));
                result = _serialPort.ReadLine();
                _log.AppendLine("Result >> " + result);       
            }
            catch (Exception e)
            {
                _errorTS.Code = _errorCode + 10;
                _errorTS.Msg = string.Format("Unexpected exception.\r\nDEBUG INFO: {0}", e);
                _errorTS.Occurred = true;
            }

            return _errorTS;
        }

        /////////////////////
        //Front LED methods//
        /////////////////////

        public ErrorTestStand SetFrontLedIntensity(byte intensity)
        {
            string result = "";

            try
            {           
                _log.AppendLine(string.Format("Front LED >> Set {0}", intensity));
                _serialPort.Write(string.Format("SetFrontLedIntensity {0}\r\n", intensity));
                result = _serialPort.ReadLine();
                _log.AppendLine("Result >> " + result);
            }
            catch (Exception e)
            {
                _errorTS.Code = _errorCode + 10;
                _errorTS.Msg = string.Format("Unexpected exception.\r\nDEBUG INFO: {0}", e);
                _errorTS.Occurred = true;
            }

            return _errorTS;
        }

        ///////////////
        //BLE methods//
        ///////////////

        //Loopback is done with uart methods loopback.

        public ErrorTestStand GetTxPower()
        {
            return _errorTS;
        }

        public ErrorTestStand SetTxPower()
        {
            return _errorTS;
        }

        public ErrorTestStand GetRSSI()
        {
            return _errorTS;
        }


        /////////////////////////
        //Accelerometer methods//
        /////////////////////////

        public ErrorTestStand Selftest(string device, int timeout)
        {
            string _device = "";
            string result = "";

            try
            {
                switch (device.ToLower())
                {
                    case "accelerometer":
                        _device = "acc";
                        break;
                    case "magnetometer":
                        _device = "mag";
                        break;
                    default:
                        _device = "ERROR";
                        break;
                }
              
                _log.AppendLine(string.Format("Selftest {0} >> Start", _device));

                _serialPort.Write(string.Format("Selftest {0} {1}\r\n", _device, timeout));

                result = _serialPort.ReadLine();
                _log.AppendLine(string.Format("Selftest Request {0} >> {1}", _device, result));

                result = _serialPort.ReadLine();
                _log.AppendLine(string.Format("Selftest Axis Value {0} >> {1}", _device, result));

                result = _serialPort.ReadLine();
                _log.AppendLine(string.Format("Selftest Pass/Fail {0} >> {1}", _device, result));
                
            }
            catch (Exception e)
            {
                _errorTS.Code = _errorCode + 10;
                _errorTS.Msg = string.Format("Unexpected exception.\r\nDEBUG INFO: {0}", e);
                _errorTS.Occurred = true;
            }

            return _errorTS;
        }


        ////////////////
        //Exit methods//
        ////////////////

        public ErrorTestStand ExitFactoryMode()
        {
            try
            {            
                _log.AppendLine(" Exit >> IntoFactory");
                _serialPort.Write("ExitFactoryMode\r\n");
            }
            catch (Exception e)
            {
                _errorTS.Code = _errorCode + 10;
                _errorTS.Msg = string.Format("Unexpected exception.\r\nDEBUG INFO: {0}", e);
                _errorTS.Occurred = true;
            }

            return _errorTS;
        }

        public ErrorTestStand ExitIntoShippingMode()
        {
            try
            {               
                _log.AppendLine(" Exit >> IntoShipping");
                _serialPort.Write("ExitIntoShippingMode\r\n");
            }
            catch (Exception e)
            {
                _errorTS.Code = _errorCode + 10;
                _errorTS.Msg = string.Format("Unexpected exception.\r\nDEBUG INFO: {0}", e);
                _errorTS.Occurred = true;
            }

            return _errorTS;
        }
    }

    public class InternalLogger
    {
        private FileVersionInfo _fvi = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location);
        private FileStream _log = null;
        private DateTime _time = DateTime.Now;
        private string _path = "Empty";

        public InternalLogger(string path)
        {
            _time = DateTime.Now;
            _path = path;

            if (!File.Exists(_path))
            {
                using (FileStream _log = File.Create(_path))
                {
                    string header =         "╔═════ SmartHalo DLL Log ═════╗\r\n";
                    header +=               "║                             ║\r\n";
                    header += String.Format("║ Date : {0}           ║\r\n", _time.ToString("d",CultureInfo.CreateSpecificCulture("fr-CA")));
                    header += String.Format("║ DLL version : {0}       ║\r\n", _fvi.FileVersion);
                    header +=               "║                             ║\r\n";
                    header +=               "╚═════════════════════════════╝\r\n";

                    Byte[] textline = new UTF8Encoding(true).GetBytes(header);
                    _log.Write(textline, 0, textline.Length);
                }
            }
        }

        public void AppendLine(string s)
        {
            _time = DateTime.Now;
            string timestamp = _time.TimeOfDay.ToString();
            timestamp += " SmartHalo DUT: ";

            using (_log = File.Open(_path, FileMode.Append, FileAccess.Write, FileShare.None))
            { 
                Byte[] textline = new UTF8Encoding(true).GetBytes(timestamp + s + "\r\n");
                _log.Write(textline, 0, textline.Length);
            }     
        }
    }
}