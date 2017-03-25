using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace MazeEditor
{
    public class Particle : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        // This method is called by the Set accessor of each property.
        // The CallerMemberName attribute that is applied to the optional propertyName
        // parameter causes the property name of the caller to be substituted as an argument.
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private double x;
        public double X 
        {
            get
            {
                return x;
            }
            set
            {
                if (value != x)
                {
                    x = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private double y;
        public double Y
        {
            get
            {
                return y;
            }
            set
            {
                if (value != y)
                {
                    y = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private double probability;
        public double Probability
        {
            get
            {
                return probability;
            }
            set
            {
                if (value != probability)
                {
                    probability = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private double alfa;
        public double Alfa
        {
            get
            {
                return alfa;
            }
            set
            {
                if (value != alfa)
                {
                    alfa = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public List<double> CalculatedProbability { get; set; }

        public List<double> CountDistance { get; set; }
        public List<double> CountAlfa { get; set; }

        public Particle()
        {
            CalculatedProbability = new List<double>();

            CountDistance = new List<double>();
            CountAlfa = new List<double>();
        }
    }
}
