using CardManagementAPI.Interfaces;

namespace CardManagementAPI.Models
{
        public sealed class UFEFee : IPaymentSingleton, IPaymentSingletonInstance
    {
        private static readonly Lazy<UFEFee> singleton =
            new Lazy<UFEFee>(() => new UFEFee(Guid.Empty));
        public static UFEFee Instance { get { return singleton.Value; } }
                
        Guid _guid;
        private int hourCurrent;
        private int hourPrevious;

        public UFEFee() : this(Guid.NewGuid()) { }
        public Guid PaymentId => _guid;

        // these members are only made public so that I can log them in the database
        // for a service, they could be private members
        public DateTime PreviousTimeStamp { get; set; }
        public DateTime CurrentTimeStamp { get; set; }
        public decimal PreviousFee { get; set; }
        private decimal Fee { get; set; }

        public decimal GetFee() // gets the fee and winds the current values into past values
        {
            CurrentTimeStamp = DateTime.UtcNow;
            hourCurrent = CurrentTimeStamp.Hour;
            hourPrevious = PreviousTimeStamp.Hour;
            Fee = NewFee(PreviousFee);
            PreviousTimeStamp = CurrentTimeStamp;
            PreviousFee = Fee;
            return Fee;
        }
        private UFEFee(Guid guid)
        {
            _guid = guid;

            if (guid == Guid.Empty)
            {
                // Adds initial "previous values" for the fee structrue
                PreviousTimeStamp = DateTime.UtcNow.AddDays(-1);
                PreviousFee = 1.0M;
            }
        }
        private decimal NewFee(decimal newFee)
        {
            // The assumption is that the fee stays the same for the full hour,
            // until another random decimal is chosen.
            int feeCycles;
            int daysPassed = (CurrentTimeStamp - PreviousTimeStamp).Days;
            // partial day hourly cycles handled
            if ((daysPassed > 0) & (hourCurrent < hourPrevious))
                feeCycles = ((daysPassed - 1) * 24) + (24 - hourPrevious) + hourCurrent;
            else
                feeCycles = daysPassed * 24 + hourCurrent - hourPrevious;
            for (int i = 1; i <= feeCycles; i++)
                newFee *= RandomUFEFactor();
            return newFee;
        }
        private static decimal RandomUFEFactor()
        {
            Random rnd = new Random();
            return new decimal(rnd.NextDouble() * 2.0);
        }
    }
}
