# Card Management API
Card Management and Payment Fees Modules

This Web API authorizes users and exposes 6 possible Card management RESTful actions. The actions are connected to an in-memory database.

There are 3 sample users mocked for use in this demo, with roles matching their login names.  

***The logins are*** **Administrator, Employee, and Customer**.  The Employee ostensibly being a bank employee.  ***The password for all is*** **"password"**.

These are the six actions below along with their Authorizations to role. 

* CreateNew (Administrator, Employee) - creates new Card accounts one at a time (many may be created)
* CreateCardForDemoCustomerOnly (Customer) - allows the Demo Customer to mock a card account to test other features Customer is authorized to use (Customer can only create one card account)
* GetBalance (All) - get a Card account balance by account
* GetAllCards (Administrator, Employee) - returns a List of all existing Card accounts in the database
* GetAllFees (Administrator) - this is a helper for another developer to see the history of Fee calculations as they were requested
* PayUsingCard (Administrator, Customer) - even an employee should not be able to charge a card, so this is restricted to Customer.  This action calculates a UFE fee and debits a total amount against the Card Account

# To Use
First, login using the AuthLogin login (api/AuthLogin/login.  Enter credentials in the form:
{
  "userName": "Customer",
  "password": "!MyCleverSecret!"
}
After submitting credentials successfully, look for the token in the response and Copy it.  

Then submit the token using the Authorize Bearer.  From there, an Authorized user should have access to the actions corresponding to her role.

* **api/CardManagementAPI/CreateNew**
* **api/CardManagementAPI/CreateCardForDemoCustomerOnly**
* **api/CardManagementAPI/GetBalance (accountNumber)**
* **api/CardManagementAPI/GetAllCards**
* **api/CardManagementAPI/GetAllFees**
* **api/CardManagementAPI/PayUsingCard (accountNumber)**


  
  
