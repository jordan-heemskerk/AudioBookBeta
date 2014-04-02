using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Phone.BackgroundTransfer;
using System.IO.IsolatedStorage;
using System.Collections.ObjectModel;  

namespace AudioBookBeta
{
    public partial class LoadFile : PhoneApplicationPage
    {
        IEnumerable<BackgroundTransferRequest> transferRequests;
        ObservableCollection<AudioBookTransfer> audioBookTransfers;


        // Booleans for tracking if any transfers are waiting for user
        // action. 
        bool WaitingForExternalPower;
        bool WaitingForExternalPowerDueToBatterySaverMode;
        bool WaitingForNonVoiceBlockingNetwork;
        bool WaitingForWiFi;

        public LoadFile()
        {
            audioBookTransfers = new ObservableCollection<AudioBookTransfer>();
            using (IsolatedStorageFile isoStore = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (!isoStore.DirectoryExists("/shared/transfers"))
                {
                    isoStore.CreateDirectory("/shared/transfers");
                }
            }
            InitializeComponent();

        }

        private void UpdateUI()
        {
            // Update the list of transfer requests
            UpdateRequestsList();

            foreach (var transfer in transferRequests) {
                
            }

            // Update the TransferListBox with the list of transfer requests.
            TransferListBox.ItemsSource = audioBookTransfers;

            // If there are 1 or more transfers, hide the "no transfers"
            // TextBlock. IF there are zero transfers, show the TextBlock.
            if (TransferListBox.Items.Count > 0)
            {
                EmptyTextBlock.Visibility = Visibility.Collapsed;
            }
            else
            {
                EmptyTextBlock.Visibility = Visibility.Visible;
            }

            LoadForTextBlock.Text = "Loading for: " + App.player.selectedBook.BookTitle;

        }

        private void InitialTransferStatusCheck()
        {
            UpdateRequestsList();

            foreach (var transfer in transferRequests)
            {
                transfer.TransferStatusChanged += new EventHandler<BackgroundTransferEventArgs>(transfer_TransferStatusChanged);
                transfer.TransferProgressChanged += new EventHandler<BackgroundTransferEventArgs>(transfer_TransferProgressChanged);
                ProcessTransfer(transfer);
            }

          /*  if (WaitingForExternalPower)
            {
                MessageBox.Show("You have one or more file transfers waiting for external power. Connect your device to external power to continue transferring.");
            }
            if (WaitingForExternalPowerDueToBatterySaverMode)
            {
                MessageBox.Show("You have one or more file transfers waiting for external power. Connect your device to external power or disable Battery Saver Mode to continue transferring.");
            }
            if (WaitingForNonVoiceBlockingNetwork)
            {
                MessageBox.Show("You have one or more file transfers waiting for a network that supports simultaneous voice and data.");
            }
            if (WaitingForWiFi)
            {
                MessageBox.Show("You have one or more file transfers waiting for a WiFi connection. Connect your device to a WiFi network to continue transferring.");
            }*/
        }


        private void ProcessTransfer(BackgroundTransferRequest transfer)
        {
            switch (transfer.TransferStatus)
            {
                case TransferStatus.Completed:

                    // If the status code of a completed transfer is 200 or 206, the
                    // transfer was successful
                    if (transfer.StatusCode == 200 || transfer.StatusCode == 206)
                    {
                        // Remove the transfer request in order to make room in the 
                        // queue for more transfers. Transfers are not automatically
                        // removed by the system.
                        RemoveTransferRequest(transfer.RequestId);

                        // In this example, the downloaded file is moved into the root
                        // Isolated Storage directory
                        using (IsolatedStorageFile isoStore = IsolatedStorageFile.GetUserStoreForApplication())
                        {
                            string filename = transfer.Tag;
                            if (isoStore.FileExists(filename))
                            {
                                isoStore.DeleteFile(filename);
                            }
                            isoStore.MoveFile(transfer.DownloadLocation.OriginalString, filename);
                        }

                        
                        App.player.selectedBook.addFile(transfer.Tag, true);

                    }
                    else
                    {
                        // This is where you can handle whatever error is indicated by the
                        // StatusCode and then remove the transfer from the queue. 
                        RemoveTransferRequest(transfer.RequestId);

                        if (transfer.TransferError != null)
                        {
                            // Handle TransferError if one exists.
                        }
                    }
                    break;



                case TransferStatus.WaitingForExternalPowerDueToBatterySaverMode:
                    WaitingForExternalPowerDueToBatterySaverMode = true;
                    break;

                case TransferStatus.WaitingForWiFi:
                    WaitingForWiFi = true;
                    break;
            }
        }

        void transfer_TransferStatusChanged(object sender, BackgroundTransferEventArgs e)
        {
            ProcessTransfer(e.Request);
            UpdateUI();
        }

        void transfer_TransferProgressChanged(object sender, BackgroundTransferEventArgs e)
        {
            UpdateUI();
        }

        private void CancelAllButton_Click(object sender, EventArgs e)
        {
            // Loop through the list of transfer requests
            foreach (var transfer in BackgroundTransferService.Requests)
            {
                RemoveTransferRequest(transfer.RequestId);
            }

            // Refresh the list of file transfer requests.
            UpdateUI();
        }

        private Boolean validURL(string url)
        {
            if (url == null || url == "") return false;
            if (url == "http://") return false;

            Uri uriResult;
            bool result = Uri.TryCreate(url, UriKind.Absolute, out uriResult) && uriResult.Scheme == Uri.UriSchemeHttp;
            if (!result) return false;

            return true;
        }

        private void addButton_Click(object sender, RoutedEventArgs e)
        {
            // Check to see if the maximum number of requests per app has been exceeded.
            if (BackgroundTransferService.Requests.Count() >= 25)
            {
                // Note: Instead of showing a message to the user, you could store the
                // requested file URI in isolated storage and add it to the queue later.
                MessageBox.Show("The maximum number of background file transfer requests for this application has been exceeded. ");
                return;
            }

            // Get the URI of the file to be transferred from the Tag property
            // of the button that was clicked.

            string transferFileName = UrlTextBox.Text;

            if (!validURL(transferFileName))
            {
                MessageBox.Show("Invalid URL");
                return;
            }

            Uri transferUri = new Uri(Uri.EscapeUriString(transferFileName), UriKind.RelativeOrAbsolute);



            // Create the new transfer request, passing in the URI of the file to 
            // be transferred.
            BackgroundTransferRequest transferRequest = new BackgroundTransferRequest(transferUri);

            transferRequest.TransferStatusChanged += new EventHandler<BackgroundTransferEventArgs>(transfer_TransferStatusChanged);
            transferRequest.TransferProgressChanged += new EventHandler<BackgroundTransferEventArgs>(transfer_TransferProgressChanged);

            // Set the transfer method. GET and POST are supported.
            transferRequest.Method = "GET";

            // Get the file name from the end of the transfer URI and create a local URI 
            // in the "transfers" directory in isolated storage.
            string downloadFile = transferFileName.Substring(transferFileName.LastIndexOf("/") + 1);
            Uri downloadUri = new Uri("shared/transfers/" + downloadFile, UriKind.RelativeOrAbsolute);
            transferRequest.DownloadLocation = downloadUri;

            // Pass custom data with the Tag property. In this example, the friendly name
            // is passed.
            transferRequest.Tag = downloadFile;
            
            

            // If the Wi-Fi-only check box is not checked, then set the TransferPreferences
            // to allow transfers over a cellular connection.
            /*if (wifiOnlyCheckbox.IsChecked == false)
            {
                transferRequest.TransferPreferences = TransferPreferences.AllowCellular;
            }
            if (externalPowerOnlyCheckbox.IsChecked == false)
            {
                transferRequest.TransferPreferences = TransferPreferences.AllowBattery;
            }
            if (wifiOnlyCheckbox.IsChecked == false && externalPowerOnlyCheckbox.IsChecked == false)
            {
                transferRequest.TransferPreferences = TransferPreferences.AllowCellularAndBattery;
            }
            */
            // Add the transfer request using the BackgroundTransferService. Do this in 
            // a try block in case an exception is thrown.
            try
            {
                BackgroundTransferService.Add(transferRequest);
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show("Unable to add background transfer request. " + ex.Message);
            }
            catch (Exception)
            {
                MessageBox.Show("Unable to add background transfer request.");
            }

            

            UpdateUI();

        }


        private void RemoveTransferRequest(string transferID)
        {
            // Use Find to retrieve the transfer request with the specified ID.
            BackgroundTransferRequest transferToRemove = BackgroundTransferService.Find(transferID);

            // Try to remove the transfer from the background transfer service.
            try
            {
                BackgroundTransferService.Remove(transferToRemove);
            }
            catch (Exception e)
            {
                // Handle the exception.
            }
        }


        private void CancelButton_Click(object sender, EventArgs e)
        {
            // The ID for each transfer request is bound to the
            // Tag property of each Remove button.
            string transferID = ((Button)sender).Tag as string;

            // Delete the transfer request
            RemoveTransferRequest(transferID);

            // Refresh the list of file transfers
            UpdateUI();
        }


        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            // Reset all of the user action Booleans on page load.
            WaitingForExternalPower = false;
            WaitingForExternalPowerDueToBatterySaverMode = false;
            WaitingForNonVoiceBlockingNetwork = false;
            WaitingForWiFi = false;



            // When the page loads, refresh the list of file transfers.
            InitialTransferStatusCheck();
            UpdateUI();
        }   

        


        private void UpdateRequestsList()
        {
            // The Requests property returns new references, so make sure that
            // you dispose of the old references to avoid memory leaks.
            if (transferRequests != null)
            {
                foreach (var request in transferRequests)
                {
                    request.Dispose();
                }
            }
            audioBookTransfers.Clear();
            transferRequests = BackgroundTransferService.Requests;
            foreach (var v in transferRequests) {
                AudioBookTransfer t = new AudioBookTransfer(v);
                audioBookTransfers.Add(t);
            }

        }

       

        

     


    }
}