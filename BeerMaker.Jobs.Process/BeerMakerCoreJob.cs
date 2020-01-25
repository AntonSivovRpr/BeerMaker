using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BeerMaker.Core.Models.Settings;
using Microsoft.Extensions.Logging;
using Quartz;
using Unosquare.RaspberryIO;
using Unosquare.RaspberryIO.Abstractions;

namespace BeerMaker.Jobs.Process
{
    [DisallowConcurrentExecution]
    public class BeerMakerCoreJob : IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {
            var dataMap = context.MergedJobDataMap;

            var settings = (BearMakerSettings)dataMap["settings"];
            var log = (ILogger)dataMap["log"];
            //var sendEmailRepository = (ISendEmailRepository)dataMap["sendEmailRepository"];

            int deviceAddress = settings.TermoSettings.DeviceAddress;

            try
            {
                var termometer = Pi.I2C.Devices.Any(d => d.DeviceId == deviceAddress) ?
                    Pi.I2C.GetDeviceById(deviceAddress) : Pi.I2C.AddDevice(deviceAddress);

                var data = ReadI2CADC(termometer);
                var temprature = CalcTemperatur(data, settings);
                var tm = CalcTm(temprature);

                //var temprature = termometer.ReadAddressWord(0);
                log.LogInformation($"T={temprature}, Tm={tm}");
            }
            catch (Exception e)
            {
                log.LogError(e, e.Message);

                foreach (var device in Pi.I2C.Devices)
                {
                    log.LogInformation($"Registered I2C Device: {device.DeviceId}");
                }


            }

        }

        static List<double> _frame = new List<double>() {0, 0, 0, 0, 0, 0, 0};
        static List<double> _middle = new List<double>(20);

        private double CalcTm(double temprature)
        {
            _frame.RemoveAt(0);
            _frame.Add(temprature);
            var mid = _frame.OrderBy(v => v).ElementAt(3);
            _middle.Add(mid);
            if (_middle.Count() > 20)
            {
                _middle.RemoveAt(0);
            }

            return _middle.Sum(v => v)/_middle.Count;
        }

        private double CalcTemperatur(uint val, BearMakerSettings settings)
        {
            var volts = val / (4096 * settings.TermoSettings.ADCU / 5) * 5;
            var resistens = (settings.TermoSettings.Rp * volts) / (5 - volts);
            var result = (1 / ((1 / (settings.TermoSettings.T0 + 273)) + ((1 / (double)settings.TermoSettings.Betta) * Math.Log(resistens / settings.TermoSettings.R0)))) - 273;
            return Math.Round(result + settings.TermoSettings.Delta, 1);
        }

        private uint ReadI2CADC(II2CDevice termometer)
        {
            uint result = 0;
            ushort numBytes = 5;
            //var bytes = termometer.Read(numBytes*2); // Tell our ADC to send out its data MSB, than LSB 
            var array = new byte[2];
            var ra = new List<uint>(5);

            for (int i = 0; i < 5; i++)
            {
                var r = termometer.ReadAddressWord(0);
                array[0] = (byte)r;
                array[1] = (byte)(r >> 8);
                var ri = array[1] + (array[0] << 8);
                ra.Add(Convert.ToUInt32(ri));
            }

            ra = ra.OrderBy(rr => rr).ToList();
            ra.RemoveAt(0);

            for (int n = 0 ;n < numBytes; n++)
            {
                var r = termometer.ReadAddressWord(0);
                array[0] = (byte)r;
                array[1] = (byte)(r >> 8);
                var ri = array[1] + (array[0] << 8);
                ra.Add(Convert.ToUInt32(ri));
                ra = ra.OrderBy(rr => rr).ToList();
                result += ra[2];
                ra.RemoveAt(0);
            }

            return result / numBytes;
        }
    }
}