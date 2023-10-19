using System.Runtime.CompilerServices;
using System;

namespace CardManagementAPI.Models
{
    public class CardNumber
    {
        // Card Number is a separate class because it involves all sorts of verification of string lengths
        // and formats, and must conform to individual bank's numbering logic , etc.  That logic is beyond 
        // the scope of this project, but the scaffolding exists here for demonstration purposes
        public Int32 Id { get; set; }
        public string AccountNumber { get; set; } 
        private string  _personalAccountCode = "";

        public const string INVALID_OR_UNITIALIZED = "000000";
        private const string CHECK_CODE = "1";
        private const string MII = "37";
        private const string BANK_BIN = "237297";
        private const int ACCOUNT_CODE_LENGTH = 6;
        private const int ACCOUNT_NUMBER_LENGTH = 15; // specification
        
        public CardNumber(bool isNewAccount = false)
        {
            AccountNumber = (isNewAccount) ?
                AccountNumberString(newAccount: true) :
                INVALID_OR_UNITIALIZED;
        }
        public CardNumber(string accountNumber) => AccountNumber = VerifiedAccountNumber(accountNumber);
        public static string VerifiedAccountNumber(string accountNumber)
        {
            //  strip sapces
            string validAccountNumber = accountNumber.Replace(" ", "");
            //  confirm is correct length and is all numeric
            bool isValid = (validAccountNumber.Length == ACCOUNT_NUMBER_LENGTH) && 
                validAccountNumber.All(c => c >= '0' && c <= '9');
            return (isValid) ? validAccountNumber : INVALID_OR_UNITIALIZED;        
        }

        private string AccountNumberString(bool newAccount = false)
        {
            string accountCode = (newAccount) ? GenerateRandomAccountCode() : _personalAccountCode.Replace(" ","");
            if (accountCode.Length != ACCOUNT_CODE_LENGTH)
                return INVALID_OR_UNITIALIZED;
            else
                return MII + BANK_BIN + accountCode + CHECK_CODE;
        }
        private static string GenerateRandomAccountCode()
        {
            Random generator = new Random();
            String randomCode6 = generator.Next(0, 999999).ToString("D6");
            return randomCode6;
        }
    }
}
