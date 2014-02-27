using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Phone.BackgroundTransfer;
using System.Windows.Data;
using System.ComponentModel;

namespace AudioBookBeta
{
    public class AudioBookTransfer : INotifyPropertyChanged
    {
        private String _PercentComplete;
        private String _Tag;
        private TransferStatus _TransferStatus;
        private string _TransferId;

        // Declare the PropertyChanged event.
        public event PropertyChangedEventHandler PropertyChanged;

        // Create the property that will be the source of the binding.
        public String PercentComplete
        {
            get { return _PercentComplete; }
            set
            {
                _PercentComplete = value;
                // Call NotifyPropertyChanged when the source property 
                // is updated.
                NotifyPropertyChanged("PercentComplete");
            }
        }

        public String Tag
        {
            get { return _Tag; }
            set
            {
                _Tag = value;
                // Call NotifyPropertyChanged when the source property 
                // is updated.
                NotifyPropertyChanged("Tag");
            }
        }

        public TransferStatus TransferStatus
        {
            get { return _TransferStatus; }
            set
            {
                _TransferStatus = value;
                // Call NotifyPropertyChanged when the source property 
                // is updated.
                NotifyPropertyChanged("TransferStatus");
            }
        }


        public void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this,
                    new PropertyChangedEventArgs(propertyName));
            }
        }

        public string TransferId
        {
            get { return _TransferId; }
            set
            {
                _TransferId = value;
                // Call NotifyPropertyChanged when the source property 
                // is updated.
                NotifyPropertyChanged("TransferId");
            }
        }



        
        public AudioBookTransfer(BackgroundTransferRequest input) {
            int percent = 0;
            double l_percent = 0;
            Tag = input.Tag;
            if (input.TotalBytesToReceive != 0)
            {
                 l_percent = ((double)input.BytesReceived / (double)input.TotalBytesToReceive) * 100;
                 percent = (int)l_percent;
            }
            else
            {
                percent = 0;
            }
            PercentComplete = percent.ToString();
            TransferStatus = input.TransferStatus;
            TransferId = input.RequestId;
         
        }

        

        
    }
}
