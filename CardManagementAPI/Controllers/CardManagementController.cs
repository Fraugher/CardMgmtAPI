using Microsoft.AspNetCore.Mvc;
using CardManagementAPI.Models;
using CardManagementAPI.Data;
using CardManagementAPI.Filters;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel;

namespace CardManagementAPI.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class CardManagementController : ControllerBase
    {
        private readonly UFEFee _feeCalculator;
        private readonly IDbContextFactory<ApiDataContext> _dbContextFactory;
        public CardManagementController(IDbContextFactory<ApiDataContext> contextFactory)
        {
            _dbContextFactory = contextFactory;
            _feeCalculator = UFEFee.Instance;
        }

        // CREATE CARD//
        //        [AuthorizeOnAnyOnePolicy("BankEmployee")]
        [Authorize]
        [HttpPost]
        [ActionName("Create Card with New Card Number")]
        [Route("CreateNew")]
        public JsonResult Create()
        {
            bool IsDuplicate = true;
            Card? card = null;
            // get a new unique Account Number
            while (IsDuplicate)
            {
                String newAccountNumber = new CardNumber(isNewAccount: true).AccountNumber;
                using (ApiDataContext context = _dbContextFactory.CreateDbContext())
                {
                    Card? cardExists = context.Cards.SingleOrDefault(c => c.AccountNumber == newAccountNumber);
                    IsDuplicate = (cardExists != null);
                }
                if (!IsDuplicate) card = new Card(newAccountNumber);
            }
            if (card != null)
            {
                ThreadPool.QueueUserWorkItem(state => AddCardToDb(card));
                var result = new
                {
                    Meta = new { Status = "Account Created!", Message = "A new card account was created with Account Number = " + card.AccountNumber},
                    Response = Ok(card)
                };
                return new JsonResult(result);
            }
            else
            {
                return new JsonResult(NotFound());
            }
        }

        // GET CARD BALANCE
        //[AuthorizeOnAnyOnePolicy("Customer,BankEmployee")]
        //[AllowAnonymous]
        [HttpGet]
        [Route("GetBalance", Name = "GetBalance")]
        [ActionName("Create Card Balance by Account Number")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(decimal))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public JsonResult GetAccountBalance(string accountNumber)
        {
            string VerifiedAccountNumber = CardNumber.VerifiedAccountNumber(accountNumber);
            if (VerifiedAccountNumber == CardNumber.INVALID_OR_UNITIALIZED)
                return new JsonResult(BadRequest());
            else
            {
                using (ApiDataContext _context = _dbContextFactory.CreateDbContext())
                {
                    Card? card = _context.Cards.FirstOrDefault(c => c.AccountNumber == VerifiedAccountNumber);
                    if (card == null)
                        return new JsonResult(NotFound());
                    else
                    {
                        var result = new
                        {
                            Meta = new
                            {
                                Status = "Account Balance",
                                Message = "The account balance on card #" + VerifiedAccountNumber +
                                        " is " + CurrencyFormat(card.AccountBalance) + "."
                            },
                            Response = Ok(card)
                        };
                        return new JsonResult(result);
                    }
                }
            }
        }

        // GET ALL EXISTING CARD NUMBERS
        //[AllowAnonymous]
//        [AuthorizeOnAnyOnePolicy("Administrator")]
        [HttpGet]
        [Route("GetAllCards", Name = "GetAllCards")]
        [ActionName("Get All Card Account Numbers and Balances")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(decimal))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public JsonResult GetAllCards()
        {
            using (ApiDataContext _context = _dbContextFactory.CreateDbContext())
            {
                List<Card> AllCards = _context.Cards.ToList();
                return (AllCards.Any()) ? new JsonResult(Ok(AllCards)) : new JsonResult(NotFound());
            }
        }

        // GET ALL FEES
        //        [AuthorizeOnAnyOnePolicy("Administrator")]
        //[AllowAnonymous]
        [HttpGet]
        [Route("GetAllFees", Name = "GetAllFees")]
        [ActionName("Get All Fees Request History")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(decimal))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public JsonResult GetAllFees()
        {
            using (ApiDataContext _context = _dbContextFactory.CreateDbContext())
            {
                List<UFEFeeLogger> UFEFees = _context.UFEFees.ToList();
                return (UFEFees.Any()) ? new JsonResult(Ok(UFEFees)) : new JsonResult(NotFound());
            }
        }

        // PAY WITH CARD
        //[AllowAnonymous]
//        [AuthorizeOnAnyOnePolicy("Customer")]
        [HttpPut]
        [Route("PayUsingCard", Name = "PayUsingCard")]
        [ActionName("Pay Using Card")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Card))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public JsonResult PayFromAccount(string accountNumber, decimal amount)
        {
            string VerifiedAccountNumber = CardNumber.VerifiedAccountNumber(accountNumber);
            string status, message;
            ActionResult response;
            if (amount < 0)
            {
                status = "Bad Request";
                message = "Payment amount must be greater than 0.";
                response = BadRequest();
            }
            else if (VerifiedAccountNumber == CardNumber.INVALID_OR_UNITIALIZED)
            {
                status = "Bad Request";
                message = "You have entered an invalid card number format.";
                response = BadRequest();
            }
            else
            {
                using (ApiDataContext _context = _dbContextFactory.CreateDbContext())
                {
                    Card? card = _context.Cards.FirstOrDefault(c => c.AccountNumber == VerifiedAccountNumber);
                    if (card != null)
                    {
                        DateTime previousDate = _feeCalculator.PreviousTimeStamp;
                        decimal previousFee = _feeCalculator.PreviousFee; // must capture before
                        decimal feeAmount = _feeCalculator.GetFee(); // because Getfee alters past values
                        // log the fee data in a separate thread
                        ThreadPool.QueueUserWorkItem(state => LogFee(previousDate, previousFee, _feeCalculator.CurrentTimeStamp, feeAmount));
                        card.PayWithCard(amount, feeAmount);
                        _context.SaveChanges();
                        decimal feePaid = (amount > 0) ? (amount * feeAmount) : 0;
                        status = "Payment Successful"; 
                        message = "Your charge of " + CurrencyFormat(amount) +
                            " was successful.  A fee of " + CurrencyFormat(feePaid) +
                            " was assessed for this transaction, resulting in a total of " +
                            CurrencyFormat(amount + feePaid) + ". Your new balance is " +
                            CurrencyFormat(card.AccountBalance);
                        response = Ok(card);
                    } 
                    else
                    {
                        status = "Card Account Not Found";
                        message = "An account with that card number does not exist.";
                        response = NotFound(card);
                    }
                }
            }
            var result = new
            {
                Meta = new { Status = status, Message = message },
                Response = response
            };
            return new JsonResult(result);
        }
        private void AddCardToDb(Card card)
        {
            using (ApiDataContext context = _dbContextFactory.CreateDbContext())
            {
                context.Cards.Add(card);
                context.SaveChanges();
            }
        }
        private void LogFee(DateTime previousDate, Decimal previousFee,
            DateTime currentDate, Decimal currentFee)
        {
            UFEFeeLogger thisFee = new UFEFeeLogger(previousDate, previousFee, currentDate, currentFee);
            using (ApiDataContext context = _dbContextFactory.CreateDbContext())
            {
                context.UFEFees.Add(thisFee);
                context.SaveChanges();
            }
        }
        private static string CurrencyFormat(decimal amount)
        {
            return string.Format("{0:C}", amount);           
        }
    }

}
