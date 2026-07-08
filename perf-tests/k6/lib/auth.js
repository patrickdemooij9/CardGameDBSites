import http from 'k6/http';
import { check } from 'k6';

// Registers a throwaway member and returns a JWT usable for authenticated requests.
// Recaptcha verification is skipped server-side in Development (no secret configured),
// but the field is still required, so we send a dummy value.
export function registerAndLogin(baseUrl, host) {
  const headers = { 'Content-Type': 'application/json', Host: host };
  const suffix = `${Date.now()}_${Math.floor(Math.random() * 1e6)}`;
  const email = `perftest_${suffix}@example.com`;
  const password = 'PerfTest123!';

  const registerRes = http.post(
    `${baseUrl}/api/account/Register`,
    JSON.stringify({
      userName: `perftest_${suffix}`,
      email,
      password,
      recaptcha: 'dummy',
    }),
    { headers }
  );

  if (registerRes.status === 200) {
    return registerRes.body.replace(/"/g, '');
  }

  // Fall back to logging in, in case the email somehow already exists.
  const loginRes = http.post(
    `${baseUrl}/api/account/Login`,
    JSON.stringify({ email, password }),
    { headers }
  );
  check(loginRes, { 'login succeeded': (r) => r.status === 200 });
  return loginRes.body.replace(/"/g, '');
}
