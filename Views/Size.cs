using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeRecorderWinUI3.Views
{
    public class Size
    {
        public int MinWidth = 1;

        public int MinHeight = 1;

        private int _width = 0;
        public int Width
        {
            get { return _width; }
            set { if (value < MinWidth) _width = 1; else _width = value; }
        }

        private int _height = 0;
        public int Height
        {
            get { return _height; }
            set { if (value < MinHeight) _height = 1; else _height = value; }
        }

        public Size()
        {

        }

        public Size(int width, int height)
        {
            this.Width = width;
            this.Height = height;
        }
    }
}
