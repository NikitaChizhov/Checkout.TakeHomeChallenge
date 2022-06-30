# Checkout Take-Home challenge

This is a primary documentation for the task, outlined in [this pdf](./Checkout.com%20Challenge.pdf)

## Intro - Assumptions and shortcuts

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

## Payment Gateway

### Constraints

As it is possible to spend unlimited amount of time for developing and polishing any complex 
software product, lets establish some constraints:

1. Payment Gateway will constitute a single service + database.
<br/><br/>
   In practice, gateway would likely consist of several services, here are some possible ones:

   * API gateway(s), perhaps for multiple protocols (REST, Web Sockets, gRPC, GraphQL, ..?) 
   * Coordinator, that governs the process by sending asynchronous messages
   * Validation service(s)
   * Merchants management service
   * Anti-fraud service(s)
   * Audit/other logging service
   * Reporting service
<br/><br/>

2. Service will have hard-coded merchant information as opposed to extracting it from database
or another service. Merchant info will be defined by id and contain its Bank Account details.
When making a request to the Gateway, merchant should be identified by id instead of providing 
bank details in the request.
<br/><br/>
    Real system will, of course, have ability to create/update/delete merchants dynamically.
<br/><br/>

3. There will be no authentication/authorization. In practice one could use similar auth as in 
Bank Simulator (with API key) - perhaps via custom attribute instead of middleware. And/or 
outsource auth to a dedicated service (for example [Apigee](https://cloud.google.com/apigee)). 
However, I believe that it creates unnecessary complexity for the task without demonstrating 
skills commensurable to the effort.
<br/><br/>

4. While outlining database choice reasons, I will not make a full market analysis and 
spend no effort on proper database setup - it will be a one instance docker container with default configuration

### API design

API will consist of two REST endpoints - for making a payment and getting a previously made payment:
POST /payments
GET /payments/{id}

API is designed under these 3 assumptions (all timings assume there is minimal latency between user and gateway):
- Making a payment takes up to several seconds 
- User wants to get immediate feedback that payment was accepted and is processing (< 50 ms)
- User wants to know that payment was made as soon as it is made (< 50 ms after payment is made)

To satisfy those, POST to /payments will return right after data in the request was validated and saved to 
database. Successful response will have 202 Accepted status code and a json body with payment id + link to 
GET endpoint.

GET /payments/{id} will utilize long polling technique. So that usual flow for the customer will be
POST /payments -> receive id right away, perhaps update UI -> GET /payments/{id} -> wait for response

We can use long polling here, cause expected waiting times are short (<< 20 seconds). It is also 
easier to use on the merchant side compared to providing callback urls or using bidirectional 
connection like web sockets (although last point is arguable)

### Database choice

Let's list requirements first:
1. Strongest possible guarantees on data persistence on successful write 
(i.e. write is replicated before success reported, write is made on disc and not stored in memory) 
2. Read-after-write consistency - if a user made a payment, GET can never return 404 regardless of how fast it is
3. Low latency writes are more important then low latency reads
4. Ability to handle multi region deployment

Additional requirement that should be discussed with business is possible outage combinations. Depending on
database setup, we may have the following combinations:
 - Unable to POST, but able to GET
 - Able to POST, but unable to GET

All, one or neither could be acceptable, which will affect database configuration and/or choice.

The good news is, many established databases can work under this requirements with proper configuration. 
So outside considerations for other parts which are outside the task scope, it basically boils down to 
developers and devops familiarity with a DB.

Some candidates and their benefits (in no particular order):

1. Cassandra: fast writes, almost infinite scalability, leaderless writes. 

   But - harder to work with multiple transaction events

2. PostgreSQL: extremely well tested, robust and known. 
 
   But - horizontal scaling is harder for writes (sharding helps, but combine it with multi-region..)

3. MongoDB: easier to work with multiple different events while keeping rich query language.

   But - similar story to postgres for scaling

4. EventStore: designed for Event Sourcing, use-case easily fits into what databases is optimized for

   But - relatively less tested, more prone to bugs (including due to new developers unfamiliarity)

Weighing pros and cons, I will choose to use postgres for this task. 