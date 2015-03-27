using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YoutubeThumbnailGrabber
{
    public class Options
    {
        public string SaveImagePath { get; set; }
        public bool AutoSaveImages { get; set; }

        public Options()
        {
            SaveImagePath = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
        }         

    }
}
