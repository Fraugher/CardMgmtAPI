using Microsoft.AspNetCore.Mvc;
using CardManagementAPI.Models;
using CardManagementAPI.Data;
using CardManagementAPI.Filters;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace CardManagementAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CardManagementAPIController : ControllerBase
    {
        private readonly UFEFee _feeCalculator;
        private readonly IDbContextFactory<ApiDataContext> _dbContextFactory;
        public CardManagementAPIController(IDbContextFactory<ApiDataContext> contextFactory)
        {
            _dbContextFactory = contextFactory;
            _feeCalculator = UFEFee.Instance;
        }

        // CREATE CARD//
        [AuthorizeOnAnyOnePolicy("Administrator, Employee")]
        [HttpPost]
        [ActionName("Create Card with New Card Number")]
        [Route("CreateNew")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Card))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public JsonResult Create()
        {
            bool IsDuplicate = true;
            Card? card = null;
            while (IsDuplicate)
            {
                // get a new unique Account Number
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
                ThreadPool.QueueUserWorkItem(state =>
                {
                    try
                    {
                        AddCardToDb(card);
                    }
                    catch (Exception ex) {}
                });
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

        // CREATE DEMO CARD//
        [AuthorizeOnAnyOnePolicy("Customer")]
        [HttpPost]
        [ActionName("Create Card so Customer can Use the Demo")]
        [Route("CreateCardForDemoCustomerOnly")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Card))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public JsonResult CreateDemo()
        {
            bool IsDuplicate = true;
            Card? card = null;
            // get a new unique Account Number
            while (IsDuplicate)
            {
                const String newAccountNumber = "372372974500000"; // DEMO Number
                using (ApiDataContext context = _dbContextFactory.CreateDbContext())
                {
                    Card? cardExists = context.Cards.SingleOrDefault(c => c.AccountNumber == newAccountNumber);
                    IsDuplicate = (cardExists != null);
                }
                if (!IsDuplicate)
                {
                    card = new Card(newAccountNumber);
                    card.AccountBalance = 540.75M;
                }
                else
                {
                    var result = new
                    {
                        Meta = new
                        {
                            Status = "Account Already Exists!",
                            Message = "Your demo card already exists with Account Number = "
                                + card.AccountNumber + " and a balance of $540.75.  You cannot create any more cards."
                        },
                        Response = BadRequest()
                    };
                    return new JsonResult(result);
                }
            }
            if (card != null)
            {
                ThreadPool.QueueUserWorkItem(state =>
                {
                    try
                    {
                        AddCardToDb(card);
                    }
                    catch (Exception ex) { }
                });
                var result = new
                {
                    Meta = new { Status = "Account Created!", Message = "A new demo card account was created with Account Number = " 
                        + card.AccountNumber + " and a balance of $540.75;"
                    },
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
        [AuthorizeOnAnyOnePolicy("Customer, Employee, Customer")]
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
        [AuthorizeOnAnyOnePolicy("Administrator, Employee")]
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
        [AuthorizeOnAnyOnePolicy("Administrator")]
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
        [AuthorizeOnAnyOnePolicy("Administrator, Customer")]
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

        [AllowAnonymous]
        [HttpGet, Route("auth/denied")]
        [ApiExplorerSettings(IgnoreApi = true)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public JsonResult AuthorizationFailed()
        {
            var result = new
            {
                Meta = new { Status = "Unauthorized!", Message = "Your user type is restricted from access to this resource." },
                Response = Unauthorized()
            };
            return new JsonResult(result);
        }

        [AllowAnonymous]
        [HttpGet, Route("auth/login")]
        [NonAction]
        [ApiExplorerSettings(IgnoreApi = true)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public JsonResult NotLogged()
        {
            var result = new
            {
                Meta = new { Status = "Please Log In", Message = "You must be logged in to access this resource." },
                Response = Unauthorized()
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
            try
            {
                UFEFeeLogger thisFee = new UFEFeeLogger(previousDate, previousFee, currentDate, currentFee);
                using (ApiDataContext context = _dbContextFactory.CreateDbContext())
                {
                    context.UFEFees.Add(thisFee);
                    context.SaveChanges();
                }
            }
            catch { }
        }
        private static string CurrencyFormat(decimal amount)
        {
            return string.Format("{0:C}", amount);           
        }
    }

}
