namespace CardManagementAPI.Models
    // Class is used to log the values that were given by the Singleton, because the singleton
    // is not bound to the database.  It is not part of the solution, just a means of testing outcomes
{
    public class UFEFeeLogger
    {
        public int Id { get; set; }
        public DateTime PreviousTimeStamp { get; set; }
        public DateTime CurrentTimeStamp { get; set; }
        public decimal PreviousFee { get; set; }
        public decimal Fee { get; set; }

        public UFEFeeLogger(DateTime previousTimeStamp, decimal previousFee, 
                DateTime currentTimeStamp, decimal fee)
        {
            PreviousTimeStamp = previousTimeStamp;
            PreviousFee = previousFee;
            CurrentTimeStamp = currentTimeStamp; 
            Fee = fee;
        }
    }
}
