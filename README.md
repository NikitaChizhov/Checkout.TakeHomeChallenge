# Checkout Take-Home challenge

This is a primary documentation for the task, outlined in [this pdf](./Checkout.com%20Challenge.pdf)

### tldr 

1. Run `docker compose up` (or `docker-compose up`) from the root of repo 
2. Open 'http://localhost:5000/swagger'
3. Make a POST request to /payments (without changing MerchantId from default)
4. Copy PaymentId from response and make GET request to /payments/{id}

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

### Repo structure and style

 - Everything "code" is located in [src](src) directory and is separated into c# projects
 - All tests are also separated into projects and are located in [tests](tests) 
 - Everything related to repo/deployment is located in the root and places in solution folder called
"Solution Items" (not an actual directory - solution folder)
 - vars used over explicit types unless there is no choice (assumed help from IDE) 
 - classes, structs and records are internal sealed unless they have to be public/non-sealed, 
interfaces are internal unless they have to be public

### Bank Simulator

As agreed in a series of emails, the task does not specify a particular existing API of acquiring bank
that should be used in a simulator. Instead, it was up to me to choose how it would look like. 
Depending on the task intended reading (it _could_ be interpreted differently) the first "extra"
I did was to design a full web api and not just a client-stub. 

It's functionality in broad strokes:

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
transactions not being "repeated". I skip all the management nuance around usage of such keys (like expiry)

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
software product, let's establish some constraints:

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
    Real system will, of course, have the ability to create/update/delete merchants dynamically.
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

To satisfy those, POST to /payments will return right after data in the request was validated and saved to the
database. Successful response will have 202 Accepted status code and a json body with payment id + link to 
GET endpoint.

GET /payments/{id} will utilise a long polling technique. So that usual flow for the customer will be
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

Additional requirement that should be discussed with business are possible outage combinations. Depending on
database setup, we may have the following combinations:
 - Unable to POST, but able to GET
 - Able to POST, but unable to GET

All, one or neither could be acceptable, which will affect database configuration and/or choice.

The good news is, many established databases can work under these requirements with proper configuration. 
So outside considerations for other parts which are outside the task scope, it basically boils down to 
developers and devops familiarity with a DB.

Some candidates and their benefits (in no particular order):

1. Cassandra: fast writes, almost infinite scalability, leaderless writes. 

   But - harder to work with multiple transaction events

2. PostgreSQL: extremely well tested, robust and known. 
 
   But - horizontal scaling is harder for writes (sharding helps, but combine it with multi-region..)

3. MongoDB: easier to work with multiple different events while keeping a rich query language.

   But - similar story to postgres for scaling

4. EventStore: designed for Event Sourcing, use-case easily fits into what databases is optimised for

   But - relatively less tested, more prone to bugs (including due to new developers unfamiliarity)

Weighing the pros and cons, I will choose to use postgres for this task. 

### Dependencies used

 - [Be.Vlaanderen.Basisregisters.Generators.Guid.Deterministic](https://www.nuget.org/packages/Be.Vlaanderen.Basisregisters.Generators.Guid.Deterministic/): 
a package for generating deterministic GUIDv5
 - [Hellang.Middleware.ProblemDetails](https://www.nuget.org/packages/Hellang.Middleware.ProblemDetails): 
middleware for formatting exceptions and errors into a ProblemDetails format
 - [Microsoft.EntityFrameworkCore](https://www.nuget.org/packages/Microsoft.EntityFrameworkCore) and 
 - [Npgsql.EntityFrameworkCore.PostgreSQL](https://www.nuget.org/packages/Npgsql.EntityFrameworkCore.PostgreSQL): 
for interacting with postgres
 - [StronglyTypedId](https://www.nuget.org/packages/StronglyTypedId): (beta!) for generating with Source Generators
boilerplate code that allows using strongly typed ids 
(like [PaymentId](src/Checkout.TakeHomeChallenge.Contracts/Requests/SupportingTypes/PaymentId.cs))
 - [Swashbuckle.AspNetCore](https://www.nuget.org/packages/swashbuckle.aspnetcore/): for api documentation
For tests:
 - [FluentAssertions](https://www.nuget.org/packages/FluentAssertions): 
for cleaner test code and assertions that look like `response.Should().BeEquivalentTo(expected);`
 - [Moq](https://www.nuget.org/packages/Moq): for mocking 

### Tests

For testing the Payment Gateway I have 
[Integration](tests/Checkout.TakeHomeChallenge.PaymentGateway.IntegrationTests) and 
[Load](tests/Checkout.TakeHomeChallenge.PaymentGateway.LoadTest) tests (with k6).

Integration tests do not require any dependencies that are run separately 
(Bank API and Postgres are mocked) and can be started as usual unit tests from IDE.

The load tests can be started as a task by running `docker compose run k6` from the root of the repository. 
The result of the run will be a statistical report, where first 3 lines are the most relevant:

Results are calculated as such:

* _make_payment - duration of a POST request to /payments
* _get_payment_immediately - duration of GET request to /payments/{id}, 
which is made right after POST has returned
* _get_payment_later - duration of GET request to /payments/{id}, which is made after 5 seconds of POST

```bash
_get_payment_immediately.......: avg=2.52s    min=1.01s   med=2.51s    max=3.97s    p(90)=3.81s    p(95)=3.86s   
_get_payment_later.............: avg=6.04ms   min=1.31ms  med=5.63ms   max=79ms     p(90)=8.44ms   p(95)=8.82ms  
_make_payment..................: avg=78.58ms  min=2.27ms  med=10.2ms   max=341.29ms p(90)=332.55ms p(95)=338.34ms
```

This is a measurement from my laptop with 32 VUs running for 30 seconds. 
Even with the correction for limited resources, MakePayment operation is a bit inconsistent - its
duration can be much higher than initial goals. The bottleneck is most likely Postgres writes, 
but deeper investigation is needed. 

### Deployment / how to run

The simplest way to run the entire project is with `docker compose up`. This will start:
1. Bank Simulator
2. Payment Gateway
3. Postgres

You can instead opt-out of using docker(-compose) and start Payment Gateway via terminal or your IDE.
In which case, without changing default appsettings, there has to be an empty postgres instance running
on localhost:5432 with "postgres" user and password "123". This can be achieved via docker with

`docker run -it --rm -e POSTGRES_PASSWORD=123 -p 5432:5432 postgres:14`

For the production deployment I would normally prefer Kubernetes. 
Additional considerations that will then be required:
 - Create deployment+service files 
 - Create/configure secrets

The latter point deserves elaboration: while Kubernetes have "secrets", where 
developers normally would place api keys, passwords and so on, those secrets are not stored securely.
As far as I am aware, it is impossible to limit access to secrets while giving access 
to cluster for other operations. In small teams that is not a huge issue, but with 
larger teams and more sensitive data, another solution is required. I have not deeply investigated 
the issue, but I heard about [Conjur](https://www.conjur.org/) and briefly used 
[1Password operator](https://github.com/1Password/onepassword-operator)

Also, before scaling the number of instances as usual, the api will benefit from having 
an idempotencyId-aware proxy. Otherwise, there needs to be a reworking of concurrency control. 

### What can be improved

In random order:

 * Strongly typed ids can and should used in database layer 
(They could be defined by a single additional flag in Contracts, but it would require 
Contracts to reference EntityFrameworkCore, which is not perfect)
 * Better annotations (for public types used by swagger, for API error responses)
 * All errors should be returned as ProblemDetails
 * Card data handling should be made into separate types. For example, this will make extraction of
4 last digits more secure and robust to accidental bugs and changes.
 * If service was restarted in the middle of transaction, there should be a way to continue it after 
its back online
 * Bank API's failed responses can be handled in a more robust manner 
(not just by throwing 502 Bad Gateway)
 * If API is expanding, a cleaner separation between request-application-database models 
can be beneficial. Mapping from one model to another can be centralised and simplified by
introducing a package like [Automapper](https://automapper.org/)
 * Project can be covered better with tests (first and foremost - unit tests)
 * Docker images can be improved from the default (mainly - making them smaller by configuring `dotnet publish`
and thus faster to start/load)
 * HTTP versions can be better supported and controlled 
(at the very least HTTP 2 support, better yet - HTTP 3)
 * The [Result](src/Checkout.TakeHomeChallenge.PaymentGateway/Model/Application/Result.cs)
class is a rather crude substitute for one-of unions, a library like 
[OneOf](https://github.com/mcintyre321/OneOf) could be used instead
 * Configure HTTPS and figure out certificate management  