using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using AudioBookBeta.Resources;
using System.Collections.ObjectModel;
using Microsoft.Phone.BackgroundAudio;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.IO.IsolatedStorage;
using System.IO;
using Windows.System.Threading;
using System.Windows.Threading;

namespace AudioBookBeta
{
    public partial class MainPage : PhoneApplicationPage
    {
        Boolean shouldResume = false;
        DispatcherTimer timer = new DispatcherTimer();
        
        // Constructor
        public MainPage()
        {
            InitializeComponent();
            Boolean found = false;
            

            BackgroundAudioPlayer.Instance.PlayStateChanged += new EventHandler(Instance_PlayStateChanged);

            PositionSlider.ManipulationStarted += new EventHandler<System.Windows.Input.ManipulationStartedEventArgs>(PosSliderDown);
            PositionSlider.ManipulationCompleted += new EventHandler<System.Windows.Input.ManipulationCompletedEventArgs>(PosSliderUp);
            PositionSlider.ManipulationDelta += new EventHandler<System.Windows.Input.ManipulationDeltaEventArgs>(PosSliderDelta);


            foreach (var i in App.player.books)
            {
                if (i.BookTitle == "Select a book...") found = true;
            }
            if (!found && App.player.books.Count == 0)
            {
                App.player.books.Insert(0, new Book("Select a book..."));
            }
            this.bookPicker.ItemsSource = App.player.books;

            loadXmlData();

            timer = new DispatcherTimer { Interval = new TimeSpan(0,0,1) };
            timer.Tick += timer_Tick;
            timer.Start();


            updateUI();
            // Sample code to localize the ApplicationBar
            //BuildLocalizedApplicationBar();
        }

        void PosSliderDown(object sender, EventArgs e)
        {
            if (BackgroundAudioPlayer.Instance.PlayerState == PlayState.Playing) shouldResume = true;
            BackgroundAudioPlayer.Instance.Pause();
            timer.Stop();
        }

        void PosSliderDelta(object sender, EventArgs e)
        {
            TimeSpan delta = new TimeSpan(0, 0, (int)PositionSlider.Value) ;
            CurrentPositionText.Text = delta.ToString("hh\\:mm\\:ss");
            RemainingTimeText.Text = "-" + (BackgroundAudioPlayer.Instance.Track.Duration - delta).ToString("hh\\:mm\\:ss");
           
        }

        void PosSliderUp(object sender, EventArgs e)
        {
            
            BackgroundAudioPlayer.Instance.Position = new TimeSpan(0, 0, (int)PositionSlider.Value);
            App.player.selectedBook.setPosition((int)BackgroundAudioPlayer.Instance.Position.TotalSeconds);
           
            timer.Start();
           if (shouldResume) BackgroundAudioPlayer.Instance.Play();
           
        }

        

        void timer_Tick(object sender, EventArgs e)
        {
            CurrentFileText.Text = BackgroundAudioPlayer.Instance.Track.Title;
            CurrentPositionText.Text = BackgroundAudioPlayer.Instance.Position.ToString("hh\\:mm\\:ss");
            RemainingTimeText.Text = "-" + (BackgroundAudioPlayer.Instance.Track.Duration - BackgroundAudioPlayer.Instance.Position).ToString("hh\\:mm\\:ss");
            PositionSlider.Maximum = BackgroundAudioPlayer.Instance.Track.Duration.TotalSeconds;
            PositionSlider.Minimum = 0;
            PositionSlider.Value = BackgroundAudioPlayer.Instance.Position.TotalSeconds;
        }

        private void AddNewBookApplicationBarMenuItem_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/NewBookPage.xaml", UriKind.Relative));
        }

        private void EditBookApplicationBarIconButton_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/LoadFile.xaml", UriKind.Relative));
        }

        private void bookPicker_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            App.player.selectedBookIndex = bookPicker.SelectedIndex;
            try 
            {
                BackgroundAudioPlayer.Instance.Pause();
                BackgroundAudioPlayer.Instance.Track = new AudioTrack(new Uri(App.player.selectedBook.files.First(), UriKind.Relative),
                    App.player.selectedBook.BookTitle, App.player.selectedBook.Author, "", null);
                BackgroundAudioPlayer.Instance.Position = new TimeSpan(0, 0, App.player.selectedBook.getPosition());
                timer.Stop();
                timer.Start();
                BackgroundAudioPlayer.Instance.Pause();
            }
            catch (Exception error)
            {
                
            }
            
            updateUI();
        }


        private void updateUI()
        {
            if (bookPicker.SelectedIndex > -1)
            {
                this.AuthorText.Text = App.player.books.ElementAt(bookPicker.SelectedIndex).Author;

            }



        }

        void Instance_PlayStateChanged(object sender, EventArgs e)
        {
            switch (BackgroundAudioPlayer.Instance.PlayerState)
            {
                case PlayState.Playing:
                    PlayButton.Content = "Pause";
                    break;

                case PlayState.Paused:
                case PlayState.Stopped:
                    PlayButton.Content = "Play";
                    break;
            }
            
            
        }

        private void playButton_Click(object sender, RoutedEventArgs e)
        {
           
            if (PlayState.Playing == BackgroundAudioPlayer.Instance.PlayerState)
            {
                App.player.selectedBook.setPosition((int)BackgroundAudioPlayer.Instance.Position.TotalSeconds);
                BackgroundAudioPlayer.Instance.Pause();
                timer.Stop();
            }
            else
            {
                BackgroundAudioPlayer.Instance.Position = new TimeSpan(0, 0, App.player.selectedBook.getPosition());
                timer.Start();
                BackgroundAudioPlayer.Instance.Play();
            }
        }

        private void loadXmlData()
        {
            App.player.books.Clear();
            XDocument xml;
            using (IsolatedStorageFile isoStore = IsolatedStorageFile.GetUserStoreForApplication())
            {
                try {
                    IsolatedStorageFileStream isoStream = new IsolatedStorageFileStream("manifest.xml", FileMode.Open, isoStore);
                    xml = XDocument.Load(isoStream);
                    IEnumerable<XElement> books = xml.Element("Books").Elements("Book");
                    for (int j = 0; j < books.Count(); j++) {
                        // Create new book object from XML
                        Book newBook = new Book(books.ElementAt(j).Attribute("title").Value,
                            books.ElementAt(j).Attribute("author").Value,
                            Convert.ToInt32(books.ElementAt(j).Attribute("current_position").Value),
                            false // Not a new book
                            );
                        App.player.books.Add(newBook);

                        //Load books files
                        IEnumerable<XElement>  bookFiles = books.ElementAt(j).Elements("File");
                        for (int i = 0; i < bookFiles.Count(); i++) {
                            App.player.books.Last().addFile(bookFiles.ElementAt(i).Value, false); // Not a new book
                        }
                    }
                    isoStream.Close();
                } catch (Exception e) {}
            }
        }

        private void PreviousButton_Click(object sender, RoutedEventArgs e)
        {
            TimeSpan interval = new TimeSpan(0,0,30);
            BackgroundAudioPlayer.Instance.Position = BackgroundAudioPlayer.Instance.Position - interval;
            App.player.selectedBook.setPosition((int)BackgroundAudioPlayer.Instance.Position.TotalSeconds);
        }

        private void ForwardButton_Click(object sender, RoutedEventArgs e)
        {
            TimeSpan interval = new TimeSpan(0, 0, 30);
            BackgroundAudioPlayer.Instance.Position = BackgroundAudioPlayer.Instance.Position + interval;
            App.player.selectedBook.setPosition((int)BackgroundAudioPlayer.Instance.Position.TotalSeconds);
        }
    }
}
