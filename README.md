# Checkout Take-Home challenge

This is a primary documentation for the task, outlined in [this pdf](./Checkout.com%20Challenge.pdf)

## Assumptions and shortcuts

### Git

1. As it is rather ubiquitous in software development, deliverable for the task was decided to be 
sent via a link to a private git repository. There are no limitations placed by the author on usage 
of the link or code in this repository, Checkout Ltd may use the link or the code in this repository 
in any way according to [MIT licence](./LICENCE.md)

2. I tried to follow realistic practices when it comes to commits content and messages, but skipped 
everything related to branch management. In a real project, I would likely use either 
[git flow](https://www.atlassian.com/git/tutorials/comparing-workflows/gitflow-workflow)
or the accepted branching model in an organisation

3. In addition to standard git, this repository uses [git Large File Storage (lfs)](https://git-lfs.github.com/) 
extension for a pdf file with the task. This is, of course, primarily for a demonstration and not for a real benefit.

### Bank Simulator

In order to simulate an acquiring bank, I created a simple REST API. It's functionality in broad strokes:

1. Ability to create a "transaction" - i.e. to transfer money from some credit card to a bank account. 
2. Ability to retrieve transaction status by it's id
3. Use simple API key auth (mainly to showcase working with secrets in Payment Gateway)

This service is implemented in two projects - 
[Checkout.TakeHomeChallenge.BankSimulator](src/Checkout.TakeHomeChallenge.BankSimulator) and
[Checkout.TakeHomeChallenge.Contracts](src/Checkout.TakeHomeChallenge.Contracts)

[Checkout.TakeHomeChallenge.Contracts](src/Checkout.TakeHomeChallenge.Contracts) is a library, which defines important
API details like routes and request-response models. By moving them into a separate project, it is
easier to see when the change is breaking and when it is internal. It has an additional possible benefit of
allowing clients to use exact request-response objects by referencing a small library.

[Checkout.TakeHomeChallenge.BankSimulator](src/Checkout.TakeHomeChallenge.BankSimulator) is the main project,
it has just a few additions on top of template ASP NET Core Web API:

1. Instead of ASP NET's authorization, there is a custom 
[AuthMiddleware](src/Checkout.TakeHomeChallenge.BankSimulator/Middlewares/AuthMiddleware.cs) 
that extracts an API key from Headers and returns 401 for anything not matching a hard-coded string. 
This is done so that there I can demonstrate and discuss how to work with secrets in the Payment Gateway

2. Service uses Hellang.Middleware.ProblemDetails nuget package, which is an easy addition of good practice 
(until we finally get decent ProblemDetails support in .NET 7). In real service, this would not be enough - 
every 'failure' response should be standardised with ProblemDetails. 
But for this simulator I thought it would be sufficient.

3. The most involved part is the 
[TransactionService](src/Checkout.TakeHomeChallenge.BankSimulator/Services/TransactionsService.cs).
There I complicated my life with non-essential concurrency requirements. They could have been avoided without 
visible differences from outside, but I thought they can be a nice demonstration

Now, there were several different options for how this API could be designed. I decided on the following:

1. POST /transactions will "transfer" money and return after the transaction has been finalised. It will take 
between ~ 1 and 4 seconds. Response will contain the transaction id and its status. 
2. GET /transactions/{id:guid} will retrieve the transaction status by id

API uses a form of idempotency control with optional 'Idempotency-Key' Header. 
Making POST requests with the same value of that header will result in the same id and 
transactions not being "repeated".

Post request takes a flat [TransactionRequest](src/Checkout.TakeHomeChallenge.Contracts/Requests/TransactionRequest.cs) 
json as input. It expects minimal information and validates the format using System.ComponentModel.DataAnnotations.
Response model is even less verbose - it contains transaction id, status and datetimes.

Possible alternatives/improvements:

 - Returning transaction id immediately via response headers, instead of in the body
 - Notifying of status changes via callbacks to a specified url

Simulator is checked with [integration tests](tests/Checkout.TakeHomeChallenge.BankSimulator.IntegrationTests)