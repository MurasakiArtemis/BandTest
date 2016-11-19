using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using System.Threading;
using Microsoft.Band;
using Microsoft.Band.Sensors;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using Windows.Storage.Pickers;
using Windows.Storage;
using Windows.Storage.Provider;
using Windows.UI.Popups;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace BandTest
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }

        int reading_SkinResponse, reading_HeartRate, reading_AmbientLight;
        double reading_SkinTemperature;
        double[] reading_accelerometer = new double[3], reading_Barometer = new double[2];
        int tiempo;

        List<double[]> lista;
        double[] currentElement;
        DateTime startDate;
        StorageFile archivo;

        private void Start_Measure_Clank(object sender, RoutedEventArgs e)
        {
            try
            {
                lista = new List<double[]>();
                currentElement = new double[9];
                tiempo = 0;
                reading_accelerometer = new double[3];
                reading_Barometer = new double[2];
                reading_SkinTemperature = 0;
                reading_SkinResponse = 0;
                reading_HeartRate = 0;
                reading_AmbientLight = 0;
                BandManager.Start_Measure_Clank();
                startDate = DateTime.Now;
                date.Text = "Fecha: " + startDate.ToString("dd/mm/yyyy hh:mm:fffffff");
            }
            catch (BandException ex)
            {
                TextBlock1.Text += " " + ex.Message;
            }
        }

        private List<string> convertList()
        {
            List<string> stringList = new List<string>();
            stringList.Add(startDate.ToString("dd/mm/yyyy hh:mm:fffffff") + ",Heart Rate, Galvanic Response, Skin Temperature, Accelometer X, Accelerometer Y, Accelerometer Z, Ambient Light, Air Pressure, Temperature");
            foreach (var item in lista)
            {
                stringList.Add("," + item[0] + "," + item[1] + "," + item[2] + "," + item[3] + "," + item[4] + "," + item[5] + "," + item[6] + "," + item[7] + "," + item[8]);
            }
            return stringList;
        }

        private async void StopSensor_Clink(object sender, RoutedEventArgs e)
        {
            TextBlock1.Text = "Guardado iniciado";
            await BandManager.BandClient.SensorManager.HeartRate.StopReadingsAsync();
            await BandManager.BandClient.SensorManager.SkinTemperature.StopReadingsAsync();
            await BandManager.BandClient.SensorManager.Gsr.StopReadingsAsync();
            await BandManager.BandClient.SensorManager.Accelerometer.StopReadingsAsync();
            await BandManager.BandClient.SensorManager.AmbientLight.StopReadingsAsync();
            await BandManager.BandClient.SensorManager.Barometer.StopReadingsAsync();
            if (archivo != null)
            {
                // write to file
                await FileIO.WriteLinesAsync(archivo, convertList());
                FileUpdateStatus status = await CachedFileManager.CompleteUpdatesAsync(archivo);
                if (status == FileUpdateStatus.Complete)
                {
                    TextBlock1.Text = "Operación completada";
                }
                else
                {
                    TextBlock1.Text = "Operación no completada";
                }
            }
        }

        private async void ButtonOpenCSV_Clonk(object sender, RoutedEventArgs e)
        {
            FileSavePicker open = new FileSavePicker();
            open.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
            open.FileTypeChoices.Add("Archivos de Excel", new List<String>() { ".csv" });
            open.SuggestedFileName = "MuestraBanda";
            archivo = await open.PickSaveFileAsync();
            if (archivo != null)
            {
                TextBoxRutaCSV.Text = archivo.Name;
                CachedFileManager.DeferUpdates(archivo);
            }
            else
                TextBoxRutaCSV.Text = "Imposible elegir archivo";
        }

        private async Task<bool> Connect_Sensor_HeartRate()
        {
            if (BandManager.BandClient.SensorManager.HeartRate.GetCurrentUserConsent() != UserConsent.Granted)
            {
                await BandManager.BandClient.SensorManager.HeartRate.RequestUserConsentAsync();
            }
            BandManager.BandClient.SensorManager.HeartRate.ReadingChanged += HeartRate_ReadingChanged;
            return true;
        }

        private async Task<bool> Connect_Sensor_SkinTemperature()
        {
            if (BandManager.BandClient.SensorManager.SkinTemperature.GetCurrentUserConsent() != UserConsent.Granted)
            {
                await BandManager.BandClient.SensorManager.SkinTemperature.RequestUserConsentAsync();
            }
            BandManager.BandClient.SensorManager.SkinTemperature.ReadingChanged += SkinTemperature_ReadingChanged;
            return true;
        }

        private async Task<bool> Connect_Sensor_GalvanicSkinResponse()
        {
            if (BandManager.BandClient.SensorManager.Gsr.GetCurrentUserConsent() != UserConsent.Granted)
            {
                await BandManager.BandClient.SensorManager.Gsr.RequestUserConsentAsync();
            }
            BandManager.BandClient.SensorManager.Gsr.ReadingChanged += SkinResponse_ReadingChanged;
            return true;
        }

        private async Task<bool> Connect_Sensor_AccelerometerResponse()
        {
            if (BandManager.BandClient.SensorManager.Accelerometer.GetCurrentUserConsent() != UserConsent.Granted)
            {
                await BandManager.BandClient.SensorManager.Accelerometer.RequestUserConsentAsync();
            }
            BandManager.BandClient.SensorManager.Accelerometer.ReadingChanged += Accelerometer_ReadingChanged;
            return true;
        }

        private async Task<bool> Connect_Sensor_AmbientLightResponse()
        {
            if (BandManager.BandClient.SensorManager.AmbientLight.GetCurrentUserConsent() != UserConsent.Granted)
            {
                await BandManager.BandClient.SensorManager.AmbientLight.RequestUserConsentAsync();
            }
            BandManager.BandClient.SensorManager.AmbientLight.ReadingChanged += AmbientLight_ReadingChanged;
            return true;
        }

        private async Task<bool> Connect_Sensor_BarometerResponse()
        {
            if (BandManager.BandClient.SensorManager.Barometer.GetCurrentUserConsent() != UserConsent.Granted)
            {
                await BandManager.BandClient.SensorManager.Barometer.RequestUserConsentAsync();
            }
            BandManager.BandClient.SensorManager.Barometer.ReadingChanged += Barometer_ReadingChanged;
            return true;
        }

        async void HeartRate_ReadingChanged(object sender, BandSensorReadingEventArgs<IBandHeartRateReading> e)
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                tiempo++;
                reading_HeartRate = e.SensorReading.HeartRate;
                HeartRate.Text = "Heart Rate: " + reading_HeartRate;
                time.Text = "Tiempo: " + tiempo;
                currentElement = new double[] { reading_HeartRate, reading_SkinResponse, reading_SkinTemperature, reading_accelerometer[0], reading_accelerometer[1], reading_accelerometer[2], reading_AmbientLight, reading_Barometer[0], reading_Barometer[1]};
                lista.Add(currentElement);
            });
        }

        async void SkinTemperature_ReadingChanged(object sender, BandSensorReadingEventArgs<IBandSkinTemperatureReading> e)
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                reading_SkinTemperature = e.SensorReading.Temperature;
                SkinTemperature.Text = "Skin Temperature: " + reading_SkinTemperature;
            });
        }

        async void SkinResponse_ReadingChanged(object sender, BandSensorReadingEventArgs<IBandGsrReading> e)
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                reading_SkinResponse = e.SensorReading.Resistance;
                SkinResponse.Text = "Galvanic Response: " + reading_SkinResponse;
            });
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            bool terminado = await BandManager.Connect_Band_Clank();
            terminado &= await Connect_Sensor_HeartRate();
            terminado &= await Connect_Sensor_SkinTemperature();
            terminado &= await Connect_Sensor_GalvanicSkinResponse();
            terminado &= await Connect_Sensor_AccelerometerResponse();
            terminado &= await Connect_Sensor_AmbientLightResponse();
            terminado &= await Connect_Sensor_BarometerResponse();
        }

        async void Accelerometer_ReadingChanged(object sender, BandSensorReadingEventArgs<IBandAccelerometerReading> e)
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                reading_accelerometer[0] = e.SensorReading.AccelerationX;
                reading_accelerometer[1] = e.SensorReading.AccelerationY;
                reading_accelerometer[2] = e.SensorReading.AccelerationZ;
                AccelerometerX.Text = "Accelerometer X: " + reading_accelerometer[0];
                AccelerometerY.Text = "Accelerometer Y: " + reading_accelerometer[1];
                AccelerometerZ.Text = "Accelerometer Z: " + reading_accelerometer[2];
            });
        }

        async void AmbientLight_ReadingChanged(object sender, BandSensorReadingEventArgs<IBandAmbientLightReading> e)
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                reading_AmbientLight = e.SensorReading.Brightness;
                AmbientLight.Text = "Ambient Light: " + reading_AmbientLight;
            });
        }

        async void Barometer_ReadingChanged(object sender, BandSensorReadingEventArgs<IBandBarometerReading> e)
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                reading_Barometer[0] = e.SensorReading.AirPressure;
                reading_Barometer[1] = e.SensorReading.Temperature;
                AirPressure.Text = "Air Pressure: " + reading_Barometer[0];
                Temperature.Text = "Temperature: " + reading_Barometer[1];
            });
        }
    }
}
