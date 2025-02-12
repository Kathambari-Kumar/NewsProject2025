namespace NewsProject.Models.API
{
    public class Electricity
    {
        public string date { get; set; }
        public SE1[] SE1 { get; set; } //Zone 1
        public SE2[] SE2 { get; set; } //Zone 2
        public SE3[] SE3 { get; set; } //Zone 3
        public SE4[] SE4 { get; set; } //Zone 4
    }

    public class SE1
    {
        public int hour { get; set; }
        public float price_eur { get; set; }
        public float price_sek { get; set; }
        public int kmeans { get; set; }  //cluster value:
                                         //0: Very low prices
                                         //1: Moderate prices.
                                         //2: Higher prices.
                                         //3: Peak prices.
    }

    public class SE2
    {
        public int hour { get; set; }
        public float price_eur { get; set; }
        public float price_sek { get; set; }
        public int kmeans { get; set; }
    }

    public class SE3
    {
        public int hour { get; set; }
        public float price_eur { get; set; }
        public float price_sek { get; set; }
        public int kmeans { get; set; }
    }

    public class SE4
    {
        public int hour { get; set; }
        public float price_eur { get; set; }
        public float price_sek { get; set; }
        public int kmeans { get; set; }
    }

}
