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
                if (_selectedBookIndex > -1) 
                {
                    return _selectedBookIndex;
                } else {
                    throw new IndexOutOfRangeException();
                }
            }
            set {
                _selectedBookIndex = value;
            }

        }

        public AudioTrack currentTrack;

        public Book selectedBook {
            get {
                return books.ElementAt(selectedBookIndex);
            }
        }

       
        

    }
}
