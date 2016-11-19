using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Band;
using Microsoft.Band.Sensors;

namespace BandTest
{
    public static class BandManager
    {
        private static String errors;
        public static String Errors
        {
            get { return BandManager.errors; }
            set { BandManager.errors += value; }
        }

        static IBandInfo selectedBand;
        public static IBandInfo SelectedBand
        {
            get { return BandManager.selectedBand; }
            set { BandManager.selectedBand = value; }
        }

        private static IBandClient bandClient;
        public static IBandClient BandClient
        {
            get { return BandManager.bandClient; }
            set
            {
                BandManager.bandClient = value;
            }
        }
        
        public static bool IsConnected
        {
            get
            {
                return BandClient != null;
            }
        }

        public static async Task<bool> Find_Band()
        {
            try
            {
                var bands = await BandClientManager.Instance.GetBandsAsync();
                if (bands != null && bands.Length > 0)
                {
                    SelectedBand = bands[0];
                }
                else
                {
                    BandManager.Errors = "Band not found";
                    return false;
                }
            }
            catch(BandException ex)
            {
                BandManager.Errors = ex.Message;
                return false;
            }
            return true;
        }

        public static async Task<bool> InitBand()
        {
            try
            {
                if (IsConnected)
                    return true;
                await Find_Band();
                if (SelectedBand != null)
                {
                    BandClient = await BandClientManager.Instance.ConnectAsync(SelectedBand);
                    await BandManager.BandClient.NotificationManager.VibrateAsync(Microsoft.Band.Notifications.VibrationType.ThreeToneHigh);
                }
            }
            catch (Exception x)
            {
                BandManager.Errors = x.Message;
                return false;
            }
            return true;
        }

        public static async Task<bool> Connect_Band_Clank()
        {
            bool terminado = await BandManager.Find_Band();
            terminado &= await BandManager.InitBand();
            return terminado;
        }

        public static async void Start_Measure_Clank()
        {
            await BandManager.BandClient.SensorManager.HeartRate.StartReadingsAsync();
            await BandManager.BandClient.SensorManager.SkinTemperature.StartReadingsAsync();
            await BandManager.BandClient.SensorManager.Gsr.StartReadingsAsync();
            await BandManager.BandClient.SensorManager.Accelerometer.StartReadingsAsync();
            await BandManager.BandClient.SensorManager.AmbientLight.StartReadingsAsync();
            await BandManager.BandClient.SensorManager.Barometer.StartReadingsAsync();
        }
    }
}
