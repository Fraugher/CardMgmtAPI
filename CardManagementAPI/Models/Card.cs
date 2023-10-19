using System.Runtime.CompilerServices;
using System;

namespace CardManagementAPI.Models
{
    public class Card
    {
        public int Id { get; set; }
        public string AccountNumber { get; set; }
        public decimal AccountBalance { get; set; }

        public Card() => AccountNumber = new CardNumber().AccountNumber;
        public Card(string cardNumber) => AccountNumber = cardNumber;
        public Card(bool isNewAcccount = false) => AccountNumber = new CardNumber(isNewAccount: isNewAcccount).AccountNumber;

        public void PayWithCard(Decimal amount, Decimal feeFactor)
        {
            if (amount > 0) AccountBalance += amount * (1 + feeFactor);
        }
    }
}
