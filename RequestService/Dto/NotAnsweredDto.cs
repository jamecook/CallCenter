using System;
using System.Windows.Media;

namespace RequestServiceImpl.Dto
{
    public class NotAnsweredDto
    {
        public string UniqueId { get; set; }

        public string CallerId { get; set; }
        public string ServiceCompany { get; set; }
        public string Prefix { get; set; }
        public int? IvrDtmf { get; set; }
        public Brush Color
        {
            get
            {
                if (!IvrDtmf.HasValue)
                    return new SolidColorBrush(Colors.Blue);
                switch (IvrDtmf.Value)
                {
                    case 1:
                        return new SolidColorBrush(Colors.Red);
                    case 3:
                        return new SolidColorBrush(Colors.Green);
                }
                return new SolidColorBrush(Colors.Blue);
            }
        }
        public DateTime? CreateTime { get; set; }
    }
}