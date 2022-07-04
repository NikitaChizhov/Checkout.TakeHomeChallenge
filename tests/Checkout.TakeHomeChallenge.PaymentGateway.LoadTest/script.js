import { check, sleep } from 'k6';
import http from 'k6/http';
import { Trend } from 'k6/metrics';
import { uuidv4 } from "https://jslib.k6.io/k6-utils/1.0.0/index.js";

export const options = {
    vus: 32,
    duration: '30s',
};

const makePayment = new Trend('_make_payment', true);
const getPaymentImmediately = new Trend('_get_payment_immediately', true);
const getPaymentLater = new Trend('_get_payment_later', true);

export default function () {

    const params = {
        headers: {
            "Content-Type": "application/json"
        }
    }
    
    const data = JSON.stringify({
        IdempotencyId: uuidv4(),
        MerchantId: "3fa85f64-5717-4562-b3fc-2c963f66afa6",
        CardNumber: "4593 4460 1631 8149",
        Name: "Nikita Chizhov",
        CardExpiryDate: "04/2024",
        CardVerificationValue: "123",
        Value: {
            Amount: 1000,
            Currency: "EUR"
        }
    });
    
    let response = http.post('http://payment-gateway/payments', data, params);
    makePayment.add(response.timings.waiting);
    check(response, {
        'Http code is 202': (r) => r.status === 202,
    });
    
    let obj = JSON.parse(response.body);

    response = http.get(`http://payment-gateway${obj.location}`, params);
    getPaymentImmediately.add(response.timings.waiting);
    check(response, {
        'Http code is 200': (r) => r.status === 200,
    });
    sleep(5);
    
    response = http.get(`http://payment-gateway${obj.location}`, params);
    getPaymentLater.add(response.timings.waiting);
    check(response, {
        'Http code is 200': (r) => r.status === 200,
    });
}