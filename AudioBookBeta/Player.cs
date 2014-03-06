using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using Microsoft.Phone.BackgroundAudio;

namespace AudioBookBeta
{
    public partial class Player
    {
        

        public ObservableCollection<Book> books = new ObservableCollection<Book>();
        private int _selectedBookIndex = -1;
        public int selectedBookIndex {
            get {
                return _selectedBookIndex;
            }
            set {
                _selectedBookIndex = value;
            }

        }

        public AudioTrack currentTrack;

        public Book selectedBook {
            get {
                if (selectedBookIndex > -1)
                {
                    return books.ElementAt(selectedBookIndex);
                }
                else
                {
                    return null;
                }
            }
        }

       
        

    }
}
