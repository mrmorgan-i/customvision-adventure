using System.ComponentModel;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using LandmarksAI_App.Classes;
using Microsoft.Win32;
using Newtonsoft.Json;

namespace LandmarksAI_App
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private string _status = "Ready to predict!";
        public string Status
        {
            get => _status;
            set
            {
                _status = value;
                OnPropertyChanged("Status");
            }
        }

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            Status = "Loading image...";
            // Clear the list view
            predictionsListView.ItemsSource = null;

            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Image files (*.png; *.jpg)|*.png; *jpg; *jpeg|All files (*.*)|*.*";
            dialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);

            if (dialog.ShowDialog() == true)
            {
                string filename = dialog.FileName;
                selectedImage.Source = new BitmapImage(new Uri(filename));

                MakePredictionsAsync(filename);
            }
        }

        private async void MakePredictionsAsync(string filename)
        {
            string url = "https://southcentralus.api.cognitive.microsoft.com/customvision/v3.0/Prediction/89f51d2f-6319-41a3-9c09-6d59161437e8/classify/iterations/LandmarksAI/image";
            string prediction_key = "d4e578803b0243dbbd43201cd3b5cabe";
            string content_type = "application/octet-stream";
            var file = File.ReadAllBytes(filename);

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Prediction-Key", prediction_key);

                using (var content = new ByteArrayContent(file))
                {
                    content.Headers.ContentType = new MediaTypeHeaderValue(content_type);
                    var response = await client.PostAsync(url, content);
                    var responseString = await response.Content.ReadAsStringAsync();

                    List<Prediction> predictions = JsonConvert.DeserializeObject<CustomVision>(responseString).Predictions;

                    predictionsListView.ItemsSource = predictions;
                }
            }
            Status = "Prediction done!";
        }

        private async void LoadImageFromUrl_OnClick(object sender, RoutedEventArgs e)
        {
            string imageUrl = imageUrlTextBox.Text;
            if (string.IsNullOrEmpty(imageUrl))
            {
                MessageBox.Show("Please enter a valid image URL.", "Invalid Url", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            selectedImage.Source = new BitmapImage(new Uri(imageUrl));

            await MakePredictionsFromUrlAsync(imageUrl);
        }

        private async Task MakePredictionsFromUrlAsync(string imageUrl)
        {
            string url = "https://southcentralus.api.cognitive.microsoft.com/customvision/v3.0/Prediction/89f51d2f-6319-41a3-9c09-6d59161437e8/classify/iterations/LandmarksAI/url";
            string prediction_key = "d4e578803b0243dbbd43201cd3b5cabe";
            string content_type = "application/json";
            var requestBody = new { Url = imageUrl };

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Prediction-Key", prediction_key);

                var content = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, content_type);
                var response = await client.PostAsync(url, content);
                var responseString = await response.Content.ReadAsStringAsync();

                List<Prediction> predictions = JsonConvert.DeserializeObject<CustomVision>(responseString).Predictions;

                predictionsListView.ItemsSource = predictions;
            }
            Status = "Prediction done!";
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
