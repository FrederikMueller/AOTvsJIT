import { check } from 'k6';
import http from 'k6/http';

export const options = {
    vus: 100,
    duration: '1m',
    insecureSkipTLSVerify: true
};

export default () => {
    const url = 'http://localhost:5201/todos/lead';
    
    const res = http.post(url, {
        "firstName": "John Doe",
        "lastName": "Doe",
        "email": "f@test.de",
        "phone": "1234567890",
        "company": "Test"
    });

    check(res, {
        'is status 200': (r) => r.status === 200,
    });
};
