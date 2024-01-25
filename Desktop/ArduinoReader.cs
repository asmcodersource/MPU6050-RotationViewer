using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Windows.Markup;

namespace SystemMPU6050
{
    public class ArduinoReader
    {
        String lastValue = "";
        private SerialPort serialPort;

        // Define an event to notify subscribers when new data is received
        public event EventHandler<string> DataReceivedEvent;
        public Queue<string> Data = new Queue<string>();

        public ArduinoReader(string portName, int baudRate = 9600)
        {
            // Initialize the SerialPort object
            serialPort = new SerialPort(portName, baudRate);
        }

        public void StartReading()
        {
            try
            {
                // Open the COM port
                serialPort.Open();

                // Attach the DataReceived event handler
                serialPort.DataReceived += SerialPort_DataReceived;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        public void StopReading()
        {
            // Close the COM port and detach the event handler
            if (serialPort.IsOpen)
                serialPort.Close();
            serialPort.DataReceived -= SerialPort_DataReceived;
        }

        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            // This event is called when data is received on the COM port
            try
            {
                // Read the data from the COM port
                string data = serialPort.ReadLine();
                data = data.TrimEnd('\r');
                lastValue = data;

                // Raise the DataReceivedEvent with the received data
                OnDataReceived(data);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading data: {ex.Message}");
            }
        }

        public string GetData()
        {
            return new String(lastValue);
        }

        protected virtual void OnDataReceived(string data)
        {
            // Raise the DataReceivedEvent with the received data
            DataReceivedEvent?.Invoke(this, data);
        }
    }
}
